using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif
{
    [Table("TabloRezervasyon")]
    public class Rezervasyon
    {
        [Key]
        public int RezervasyonID { get; set; }
        public string? KullaniciEmail { get; set; } 
        public int? KitapID { get; set; }
        public DateTime? TalepTarihi { get; set; }
        public string? OnayDurumu { get; set; } //Beklemede reddedildi Onaylandı mesajlarından biri verilecek
    }
}