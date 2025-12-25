namespace Akıllı_Kütüphane_Yönetim_Sistemi.Models 
{
    public class KullaniciGuncelleModel
    {
        public string Email { get; set; }    //Kullanıcı güncellemede kısmında (Frontda Profil kısmı) istenen veriler 
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string MevcutSifre { get; set; } // İşlem onayı için şart
        public string? YeniSifre { get; set; }  // Opsiyonel kısısm şifre değiştirmek istenirse
    }
}