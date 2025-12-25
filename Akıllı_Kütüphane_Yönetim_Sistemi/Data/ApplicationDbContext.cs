using Microsoft.EntityFrameworkCore;
// Modellerin olduğu klasörü tanıtıyoruz:
using Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif;

namespace Akıllı_Kütüphane_Yönetim_Sistemi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        //SQL Deki TABLOLARIN BELİRTİLDİĞİ KISIM
        public DbSet<Kitap> Kitaplar { get; set; }
        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<OduncIslem> OduncIslemler { get; set; }
        public DbSet<Ceza> Cezalar { get; set; }
        public DbSet<Rezervasyon> Rezervasyonlar { get; set; }
        public DbSet<RezervasyonLog> RezervasyonLoglari { get; set; }
        public DbSet<OduncLog> OduncLoglari { get; set; }
    }
}