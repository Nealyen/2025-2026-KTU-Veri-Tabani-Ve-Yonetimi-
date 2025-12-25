using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif
{
    [Table("TabloSistemLog")]
    public class SistemLog
    {
        [Key]
        public int LogID { get; set; }

        public string? KullaniciEmail { get; set; }
        public string? Ad { get; set; }      
        public string? Soyad { get; set; }   
        public string? IslemTuru { get; set; }
        public string? Aciklama { get; set; }
        public DateTime Tarih { get; set; }
    }
}