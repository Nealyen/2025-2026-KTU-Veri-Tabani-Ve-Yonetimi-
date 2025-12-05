using Microsoft.EntityFrameworkCore;
using Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif; // Kitap tarifini buradan alıyoruz

namespace Akıllı_Kütüphane_Yönetim_Sistemi.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Burası bizim "Kitaplar" tablomuzun tanımı
        public DbSet<Kitap> Kitaplar { get; set; }

        // İŞTE ARADIĞIN AYAR BURASI:
        // Bu ayar sayesinde program her çalıştığında veritabanına nereye bağlanacağını bilecek.
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Eğer daha önce ayarlanmadıysa, SQL Server'a bu adresten bağlan diyoruz.
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=AkilliKutuphaneDB;Trusted_Connection=True;TrustServerCertificate=True");
            }
        }
    }
}