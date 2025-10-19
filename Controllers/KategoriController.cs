using Elde_Tarif.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Elde_Tarif.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KategoriController : ControllerBase
    {
        private readonly AppDbContext _dbcontext;
    

    public KategoriController(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        [HttpGet]
        // GET: api/Kategori 
        public async Task<ActionResult<IEnumerable<GetKategoriDTO>>> GetKategoriler()
        {
            var kategoriler = await _dbcontext.Kategoriler
                .OrderBy(k => k.Sira)
                .Select(k => new GetKategoriDTO
                {
                    Id = k.Id,
                    Ad = k.Ad,
                    KategoriUrl = k.KategoriUrl
                })
                .ToListAsync();

            if (kategoriler == null || kategoriler.Count == 0)
                return NotFound("Hiç kategori bulunamadı.");

            return Ok(kategoriler);
        }

    } 
}