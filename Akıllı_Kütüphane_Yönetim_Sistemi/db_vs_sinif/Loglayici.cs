using Akıllı_Kütüphane_Yönetim_Sistemi.Data;
using Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif;

namespace Akıllı_Kütüphane_Yönetim_Sistemi
{
    public static class Loglayici
    {
        // Artık Ad ve Soyad parametrelerini de alıyor
        public static void Kaydet(ApplicationDbContext context, string email, string ad, string soyad, string islemTuru, string mesaj)
        {
            try
            {
                var yeniLog = new SistemLog
                {
                    KullaniciEmail = email,
                    Ad = ad,        
                    Soyad = soyad, 
                    IslemTuru = islemTuru,
                    Aciklama = mesaj,
                    Tarih = DateTime.Now
                };

                context.SistemLoglari.Add(yeniLog);
                context.SaveChanges();
            }
            catch
            {
                // Hata olursa program durmasın
            }
        }
    }
}