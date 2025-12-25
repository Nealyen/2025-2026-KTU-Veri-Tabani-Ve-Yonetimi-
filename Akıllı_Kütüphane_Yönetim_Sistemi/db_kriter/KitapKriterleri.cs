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
            var kitapIslemleri = new KitapOzellikleri(_context); //dısarıdan gelen context özellikler kısmında kullanılsın diye cagırıyoruz
            return kitapIslemleri.TumKitaplariGetir();
        }

        public void KitabiKontrolEtVeEkle(Kitap yeniKitap)
        {
            var kitapIslemleri = new KitapOzellikleri(_context);   //Kitap listele kısmında kitap ekleme işlemi için kullanılıyor 
            kitapIslemleri.KitapEkle(yeniKitap);
        }

        public void KitabiSistemdenKaldir(int id)
        {
            var kitapIslemleri = new KitapOzellikleri(_context);   //Kitap listele kısmında kitap silme işlemi için kullanılıyor
            kitapIslemleri.KitapSil(id);
        }

        public Kitap DuzenlenecekKitabiGetir(int id)
        {
            var kitapIslemleri = new KitapOzellikleri(_context);    //kitap düzenleme kısmında düzenlenecek kitabı getirme işlemi için kullanılıyor
            return kitapIslemleri.TekKitapGetir(id);
        }

        public void KitapBilgileriniGuncelle(Kitap kitap)           //kitap düzenleme kısmında kitap bilgilerini güncelleme işlemi için kullanılıyor
        {
            var kitapIslemleri = new KitapOzellikleri(_context);
            kitapIslemleri.KitapGuncelle(kitap);
        }
    }
}