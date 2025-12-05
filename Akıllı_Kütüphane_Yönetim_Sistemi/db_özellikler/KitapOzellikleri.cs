using Akıllı_Kütüphane_Yönetim_Sistemi.Data; 
using Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif; 

namespace Akıllı_Kütüphane_Yönetim_Sistemi.db_özellikler
{
    public class KitapOzellikleri
    {
        public List<Kitap> TumKitaplariGetir()
        {
            using (var context = new ApplicationDbContext())
            {
                
                var kitapListesi = context.Kitaplar.ToList();   //Tüm kitapları Liste yapısına çevirmek içins

                return kitapListesi;
            }
        }
    }
}