using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif
{
    [Table("TabloKullanici")]
    public class Kullanici
    {
        [Key]
        public string Email { get; set; } 
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public string? Sifre { get; set; }
        public bool? IsAdmin { get; set; }  // Kullanıcı Türü
    }
}