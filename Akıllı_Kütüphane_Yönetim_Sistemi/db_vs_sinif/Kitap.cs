using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif
{
    [Table("TabloKitap")]
    public class Kitap
    {
        [Key]
        public int KitapID { get; set; }
        public string? KitapAdi { get; set; }
        public int? KalanKitap { get; set; }
        public int? AlinanKitapSayisi { get; set; }
        public string? ResimData { get; set; }  
        //frondaki kitap kapaklarının sql de tutulacak olan yerini belirtiyor , fotograflar base64 e çevirilecek 
    }
}