using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif
{
    [Table("TabloOduncIslemler")]
    public class OduncIslem
    {
        [Key]
        public int IslemID { get; set; }
        public string? KullaniciEmail { get; set; } 
        public int? KitapID { get; set; }
        public DateTime? AlisTarihi { get; set; }
        public DateTime? TeslimTarihi { get; set; }
        public DateTime? SonTeslimTarihi { get; set; } //14 gün olarak ayarlanacak
    }
}