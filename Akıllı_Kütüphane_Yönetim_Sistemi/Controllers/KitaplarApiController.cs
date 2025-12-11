using Akıllı_Kütüphane_Yönetim_Sistemi.Data;
using Akıllı_Kütüphane_Yönetim_Sistemi.db_kriter;
using Akıllı_Kütüphane_Yönetim_Sistemi.db_özellikler;
using Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif;
using Microsoft.AspNetCore.Mvc;

namespace Akıllı_Kütüphane_Yönetim_Sistemi.Controllers
{
    [Route("api/kitaplar")]
    [ApiController]
    public class KitaplarApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public KitaplarApiController(ApplicationDbContext context)
        {
            _context = context; //bağlantı için context'i alıyoruz
        }

        [HttpGet]
        public List<Kitap> TumKitaplariGetir()
        {
            // Context'i alıp Kriter sınıfında kullanıyoruz
            var kitapServisi = new KitapKriterleri(_context);
            return kitapServisi.ListelemekIcinKitaplariHazirla();
        }

        [HttpGet("{id}")]
        public Kitap TekKitapGetir(int id)
        {
            var kitapServisi = new KitapKriterleri(_context);
            return kitapServisi.DuzenlenecekKitabiGetir(id);
        }

        [HttpPost]
        public void KitapEkle(Kitap yeniKitap)
        {
            var kitapServisi = new KitapKriterleri(_context);
            kitapServisi.KitabiKontrolEtVeEkle(yeniKitap);
        }

        [HttpPut]
        public void KitapGuncelle(Kitap guncellenecekKitap)
        {
            var kitapServisi = new KitapKriterleri(_context);
            kitapServisi.KitapBilgileriniGuncelle(guncellenecekKitap);
        }

        [HttpDelete("{id}")]
        public void KitapSil(int id)
        {
            var kitapServisi = new KitapKriterleri(_context);
            kitapServisi.KitabiSistemdenKaldir(id);
        }
        //----------------------------------------------------------- geçici 
        
        [HttpGet("ceza-hesapla")] // Tarayıcıdan api/kitaplar/ceza-hesapla deyince çalışacak
        public string CezalariGuncelle()
        {
            var kitapServisi = new KitapKriterleri(_context);

            // Buraya erişmek için Kriter sınıfına da bir köprü (metot) yazmamız gerekecek
            // Ama şimdilik doğrudan mantığı test etmek için şöyle bir hile yapalım:
            // (Normalde bunu KitapKriterleri üzerinden çağırmalıyız, şimdi hızlıca test ediyoruz)

            var ozellikler = new KitapOzellikleri(_context);
            ozellikler.GecikmeVeCezaKontrolu();

            return "Gecikmiş kitaplar kontrol edildi ve cezalar kesildi/güncellendi.";
        }
    }
}