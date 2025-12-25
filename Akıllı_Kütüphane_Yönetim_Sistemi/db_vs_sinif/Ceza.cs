using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif
{
    [Table("TabloCeza")]
    public class Ceza
    {
        [Key]
        public int CezaID { get; set; }

        public string? KullaniciEmail { get; set; } // ID yerine Email kullanıyoruz
        public int? IslemID { get; set; }
        public int? GecikmeGunSayisi { get; set; }

        [Column(TypeName = "decimal(18, 2)")] // Resimdeki (18, 2) ayarı
        public decimal? CezaTutari { get; set; }

        public bool? OdendiMi { get; set; }
    }
}