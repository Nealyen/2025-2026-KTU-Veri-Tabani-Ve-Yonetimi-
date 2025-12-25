using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif
{
    [Table("TabloRezervasyonLog")]
    public class RezervasyonLog
    {
        [Key]
        public int LogID { get; set; }
        public int EskiRezervasyonID { get; set; }
        public string? KullaniciEmail { get; set; }
        public int? KitapID { get; set; }
        public DateTime? TalepTarihi { get; set; }
        public DateTime? OnayTarihi { get; set; }
        public string? OnayDurumu { get; set; }
        public DateTime ArsivlenmeTarihi { get; set; }
    }
}