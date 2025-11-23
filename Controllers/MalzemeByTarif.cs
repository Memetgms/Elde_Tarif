using Elde_Tarif.DTO;
using Elde_Tarif.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Elde_Tarif.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MalzemeByTarifController : ControllerBase
    {
        private readonly AppDbContext _context;
        public MalzemeByTarifController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Seçilen malzemelere göre tarif önerir.
        /// Ağırlıklı benzerlik (Malzeme.OnemKatsayisi) kullanır.
        /// </summary>
        [HttpPost()]
        public async Task<ActionResult<List<TarifOneriSonucDto>>> OneriGetir(
            [FromBody] TarifOneriIstekDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.MalzemeIdler == null || !model.MalzemeIdler.Any())
                return BadRequest("En az bir malzeme seçmelisiniz.");

            var secilenIdSet = model.MalzemeIdler.ToHashSet();

            var minSkor = model.MinimumSkorYuzde ?? 0.0;
            var minEslesen = model.MinimumEslesenMalzemeSayisi ?? 1;
            var maxSonuc = model.MaksimumSonuc ?? 20;

            // 1) Yayında olan tarifleri, malzemeleriyle birlikte çek
            var tarifler = await _context.Tarifler
                .Include(t => t.TarifMalzemeleri)
                    .ThenInclude(tm => tm.Malzeme)
                .ToListAsync();

            // 2) Her tarif için ağırlıklı skor hesaplama
            var tumSonuclar = tarifler
                .Select(t =>
                {
                    var malzemeler = t.TarifMalzemeleri
                        .Where(tm => tm.Malzeme != null)
                        .Select(tm => tm.Malzeme!)
                        .GroupBy(m => m.Id)
                        .Select(g => g.First())
                        .ToList();


                    var toplamAgirlik = malzemeler.Sum(m => m.OnemKatsayisi);

                    var eslesenMalzemeler = malzemeler
                        .Where(m => secilenIdSet.Contains(m.Id))
                        .ToList();

                    var eslesenAgirlik = eslesenMalzemeler.Sum(m => m.OnemKatsayisi);

                    var skor = toplamAgirlik > 0
                        ? (eslesenAgirlik / toplamAgirlik) * 100.0
                        : 0.0;

                    return new TarifOneriSonucDto
                    {
                        TarifId = t.Id,
                        Baslik = t.Baslik,
                        SkorYuzde = Math.Round(skor, 2),
                        EslesenAgirlik = eslesenAgirlik,
                        ToplamAgirlik = toplamAgirlik,
                        EslesenMalzemeSayisi = eslesenMalzemeler.Count,
                        TarifToplamMalzemeSayisi = malzemeler.Count,
                        ClusterId = t.ClusterId,
                        EslesenMalzemeler = eslesenMalzemeler
                            .Select(m => m.Ad)
                            .ToList(),
                        TarifFoto=t.KapakFotoUrl

                    };
                })
                // Temel filtre: skor ve eşleşen malzeme sayısı
                .Where(x => x.SkorYuzde >= minSkor && x.EslesenMalzemeSayisi >= minEslesen)
                .ToList();

            // 
            if (model.SadeceEnIyiClusterdanMi)
            {
                // ClusterId'si olan tarifler arasında, cluster ortalama skorlarına bak
                var clusterSkorlari = tumSonuclar
                    .Where(x => x.ClusterId.HasValue)
                    .GroupBy(x => x.ClusterId!.Value)
                    .Select(g => new
                    {
                        ClusterId = g.Key,
                        OrtalamaSkor = g.Average(x => x.SkorYuzde)
                    })
                    .OrderByDescending(x => x.OrtalamaSkor)
                    .ToList();

                if (clusterSkorlari.Any())
                {
                    var enIyiClusterId = clusterSkorlari.First().ClusterId;

                    tumSonuclar = tumSonuclar
                        .Where(x => x.ClusterId == enIyiClusterId)
                        .ToList();
                }
                // ClusterId'si olmayan tarifler varsa, bunlar bu modda devre dışı kalır.
            }

            // 4) Sonuçları skoruna göre sırala ve sınırla
            var sonucList = tumSonuclar
                .OrderByDescending(x => x.SkorYuzde)
                .ThenBy(x => x.TarifId)
                .Take(maxSonuc)
                .ToList();

            return Ok(sonucList);
        }
    }
}
