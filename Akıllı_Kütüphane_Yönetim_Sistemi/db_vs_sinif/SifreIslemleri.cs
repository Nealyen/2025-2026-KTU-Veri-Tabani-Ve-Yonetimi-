using BCrypt.Net;

namespace Akıllı_Kütüphane_Yönetim_Sistemi
{
    public static class SifreIslemleri
    {
        // şifreleri hashleyip geri döndürür
        public static string Sifrele(string ciplakSifre)
        {
            return BCrypt.Net.BCrypt.HashPassword(ciplakSifre);
        }

        // yazılı şifre ile veritabanındaki hashli şifreyi kıyaslar
        public static bool Dogrula(string girilenSifre, string veritabaniHash)
        {
            return BCrypt.Net.BCrypt.Verify(girilenSifre, veritabaniHash);
        }
    }
}