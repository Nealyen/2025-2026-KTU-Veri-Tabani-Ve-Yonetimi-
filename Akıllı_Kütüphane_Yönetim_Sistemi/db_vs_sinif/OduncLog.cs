using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif
{
    [Table("TabloOduncLog")]
    public class OduncLog
    {
        [Key]
        public int LogID { get; set; }
        public int EskiIslemID { get; set; }
        public string? KullaniciEmail { get; set; }
        public int? KitapID { get; set; }
        public DateTime? AlisTarihi { get; set; }
        public DateTime? TeslimTarihi { get; set; }
        public DateTime? SonTeslimTarihi { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? ParaCezasi { get; set; }

        public DateTime ArsivlenmeTarihi { get; set; }
    }
}