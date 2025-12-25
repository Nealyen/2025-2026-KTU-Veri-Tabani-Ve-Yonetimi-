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
            _context = context; //bağlantı için context'i kullanıyoruz
        }

        [HttpGet]
        public List<Kitap> TumKitaplariGetir()
        {
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
        public IActionResult KitapEkle(Kitap yeniKitap)
        {
            // 1. KONTROL: Veritabanında aynı isimde kitap var mı? (Büyük/küçük harf duyarsız olması için ToLower kullanılır)
            var ayniIsimliKitap = _context.Kitaplar
                .FirstOrDefault(k => k.KitapAdi.ToLower() == yeniKitap.KitapAdi.ToLower());

            if (ayniIsimliKitap != null)
            {
                // Varsa hata fırlat ve işlemi durdur
                return BadRequest(new { mesaj = "Bu isimde bir kitap zaten sistemde mevcut!" });
            }

            // Yoksa ekleme işlemine devam et
            var kitapServisi = new KitapKriterleri(_context);
            kitapServisi.KitabiKontrolEtVeEkle(yeniKitap);

            return Ok(new { mesaj = "Kitap başarıyla eklendi! 🎉" });
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
        
        
        [HttpGet("ceza-hesapla")] 
        // Tarayıcıdan api/kitaplar/ceza-hesapla deyince çalışacak //Buranın var olması gereksiz , ceza kısmı ödünc alınanlar kısmında gözükecek
        public string CezalariGuncelle()
        {
            var kitapServisi = new KitapKriterleri(_context);
            var ozellikler = new KitapOzellikleri(_context);
            ozellikler.GecikmeVeCezaKontrolu();

            return "Gecikmiş kitaplar kontrol edildi ve cezalar kesildi/güncellendi.";  //burası silinecek
        }
    }
}