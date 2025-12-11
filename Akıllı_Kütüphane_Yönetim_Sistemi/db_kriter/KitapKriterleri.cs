using Akıllı_Kütüphane_Yönetim_Sistemi.db_özellikler;
using Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif;
using Akıllı_Kütüphane_Yönetim_Sistemi.Data;

namespace Akıllı_Kütüphane_Yönetim_Sistemi.db_kriter
{
    public class KitapKriterleri
    {
        private readonly ApplicationDbContext _context;

        
        public KitapKriterleri(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Kitap> ListelemekIcinKitaplariHazirla()
        {
            var kitapSorumlusu = new KitapOzellikleri(_context); //dısarıdan gelen context özellikler kısmında kullanılsın diye cagırıyoruz
            return kitapSorumlusu.TumKitaplariGetir();
        }

        public void KitabiKontrolEtVeEkle(Kitap yeniKitap)
        {
            var kitapSorumlusu = new KitapOzellikleri(_context);
            kitapSorumlusu.KitapEkle(yeniKitap);
        }

        public void KitabiSistemdenKaldir(int id)
        {
            var kitapSorumlusu = new KitapOzellikleri(_context);
            kitapSorumlusu.KitapSil(id);
        }

        public Kitap DuzenlenecekKitabiGetir(int id)
        {
            var kitapSorumlusu = new KitapOzellikleri(_context);
            return kitapSorumlusu.TekKitapGetir(id);
        }

        public void KitapBilgileriniGuncelle(Kitap kitap)
        {
            var kitapSorumlusu = new KitapOzellikleri(_context);
            kitapSorumlusu.KitapGuncelle(kitap);
        }
    }
}