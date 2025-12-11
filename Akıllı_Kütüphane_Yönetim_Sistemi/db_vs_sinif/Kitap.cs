using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif
{
    [Table("TabloKitap")] //DB üzerindeki TabloKitap isimli tabloyu kullanmak için
    public class Kitap
    {
        [Key]
        public int KitapID { get; set; }
        public string? KitapAdi { get; set; }
        public int? KitapAdedi { get; set; }
        public bool? KitapAlindiMi { get; set; }
        public int? KategoriID { get; set; }
        public int? YazarID { get; set; }
        
        
        
    }
}