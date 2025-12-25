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

        

        [HttpPut]
        public void KitapGuncelle(Kitap guncellenecekKitap)
        {
            var kitapServisi = new KitapKriterleri(_context);
            kitapServisi.KitapBilgileriniGuncelle(guncellenecekKitap);
        }

        [HttpPost]
        public IActionResult KitapEkle(Kitap yeniKitap)
        {
            var ayniIsimliKitap = _context.Kitaplar
                .FirstOrDefault(k => k.KitapAdi.ToLower() == yeniKitap.KitapAdi.ToLower());

            if (ayniIsimliKitap != null)
            {
                return BadRequest(new { mesaj = "Bu isimde bir kitap zaten sistemde mevcut!" });
            }

            var kitapServisi = new KitapKriterleri(_context);
            kitapServisi.KitabiKontrolEtVeEkle(yeniKitap);

            // --- LOGLAMA ---
            string email = HttpContext.Session.GetString("UserSession") ?? "Admin";
            string ad = HttpContext.Session.GetString("UserAd") ?? "Admin";
            string soyad = HttpContext.Session.GetString("UserSoyad") ?? "";

            Loglayici.Kaydet(_context, email, ad, soyad, "İşlem", $"Sisteme '{yeniKitap.KitapAdi}' adlı yeni bir kitap ekledi.");

            return Ok(new { mesaj = "Kitap başarıyla eklendi! 🎉" });
        }

        [HttpDelete("{id}")]
        public void KitapSil(int id)
        {
            // Silinmeden önce kitabın adını alalım ki loga yazabilelim
            var kitap = _context.Kitaplar.Find(id);
            string kitapAdi = kitap != null ? kitap.KitapAdi : "Bilinmeyen Kitap";

            var kitapServisi = new KitapKriterleri(_context);
            kitapServisi.KitabiSistemdenKaldir(id);

            //LOG KISMI
            string email = HttpContext.Session.GetString("UserSession") ?? "Admin";
            string ad = HttpContext.Session.GetString("UserAd") ?? "Admin";
            string soyad = HttpContext.Session.GetString("UserSoyad") ?? "";

            if (kitap != null) // Sadece gerçekten silindiyse logla
            {
                Loglayici.Kaydet(_context, email, ad, soyad, "İşlem", $"'{kitapAdi}' adlı kitabı sistemden sildi.");
            }
        }
    }
}