using Elde_Tarif.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Elde_Tarif
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Kategori> Kategoriler => Set<Kategori>();
        public DbSet<Sef> Sefler => Set<Sef>();
        public DbSet<Malzeme> Malzemeler => Set<Malzeme>();
        public DbSet<Tarif> Tarifler => Set<Tarif>();
        public DbSet<YapimAdimi> YapimAdimlari => Set<YapimAdimi>();
        public DbSet<TarifMalzemesi> TarifMalzemeleri => Set<TarifMalzemesi>();
        public DbSet<Yorum> Yorumlar => Set<Yorum>();
        public DbSet<Favori> Favoriler => Set<Favori>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            //Tablo isimleri
            b.Entity<Kategori>().ToTable("Kategori");
            b.Entity<Sef>().ToTable("Sef");
            b.Entity<Malzeme>().ToTable("Malzeme");
            b.Entity<Tarif>().ToTable("Tarif");
            b.Entity<YapimAdimi>().ToTable("YapimAdimi");
            b.Entity<TarifMalzemesi>().ToTable("TarifMalzemesi");
            b.Entity<Yorum>().ToTable("Yorum");
            b.Entity<Favori>().ToTable("Favori");

            // Kısıtlar & Hassasiyetler
            b.Entity<Tarif>(e =>
            {
                e.HasOne(x => x.Kategori)
                    .WithMany(k => k.Tarifler)
                    .HasForeignKey(x => x.KategoriId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Sef)
                    .WithMany(s => s.Tarifler)
                    .HasForeignKey(x => x.SefId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            b.Entity<Yorum>(e =>
            {
                e.HasOne(y => y.Tarif)
                    .WithMany(t => t.Yorumlar)
                    .HasForeignKey(y => y.TarifId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(y => y.Kullanici)
                    .WithMany(u => u.Yorumlar)
                    .HasForeignKey(y => y.KullaniciId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<YapimAdimi>(e =>
            {
                e.HasOne(a => a.Tarif)
                    .WithMany(t => t.YapimAdimlari)
                    .HasForeignKey(a => a.TarifId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<TarifMalzemesi>(e =>
            {
                e.HasOne(tm => tm.Tarif)
                    .WithMany(t => t.TarifMalzemeleri)
                    .HasForeignKey(tm => tm.TarifId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(tm => tm.Malzeme)
                    .WithMany(m => m.TarifMalzemeleri)
                    .HasForeignKey(tm => tm.MalzemeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            b.Entity<Favori>(e =>
            {
                e.HasKey(f => new { f.KullaniciId, f.TarifId });

                e.HasOne(f => f.Kullanici)
                    .WithMany(u => u.Favoriler)
                    .HasForeignKey(f => f.KullaniciId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(f => f.Tarif)
                    .WithMany(t => t.Favoriler)
                    .HasForeignKey(f => f.TarifId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Basit index örnekleri
            b.Entity<Kategori>().HasIndex(x => x.Ad);
            b.Entity<Sef>().HasIndex(x => x.Ad);
            b.Entity<Malzeme>().HasIndex(x => x.Ad);
            b.Entity<Tarif>().HasIndex(x => x.Baslik);
        }
    }
}
