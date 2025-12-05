
using Akıllı_Kütüphane_Yönetim_Sistemi.db_özellikler;
using Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif;  
namespace Akıllı_Kütüphane_Yönetim_Sistemi.db_kriter
{
    public class KitapKriterleri
    {
        public List<Kitap> ListelemekIcinKitaplariHazirla()
        {
            var kilerSorumlusu = new KitapOzellikleri();

            var tumKitaplar = kilerSorumlusu.TumKitaplariGetir();   

            return tumKitaplar;     //Özellik eklemeyi unutma 
        }
    }
}