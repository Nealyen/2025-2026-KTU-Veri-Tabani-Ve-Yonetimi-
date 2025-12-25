using Akıllı_Kütüphane_Yönetim_Sistemi.Data;
using Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif;
using Akıllı_Kütüphane_Yönetim_Sistemi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Akıllı_Kütüphane_Yönetim_Sistemi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KullaniciController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public KullaniciController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("kayit")]
        public IActionResult KayitOl([FromBody] Kullanici yeniKullanici)
        {
            if (_context.Kullanicilar.Any(u => u.Email == yeniKullanici.Email))
            {
                return BadRequest(new { mesaj = "Bu e-posta zaten kayıtlı!" });
            }

            yeniKullanici.Sifre = SifreIslemleri.Sifrele(yeniKullanici.Sifre);
            yeniKullanici.IsAdmin = false;
            _context.Kullanicilar.Add(yeniKullanici);

            // Veritabanına kaydet
            _context.SaveChanges();

            // --- MAIL GÖNDERME KISMI (YENİ) ---
            string mailIcerik = $@"
        <h3>Aramıza Hoş Geldin, {yeniKullanici.Ad}! 🌸</h3>
        <p>Kütüphane yönetim sistemimize kaydın başarıyla oluşturuldu.</p>
        <p>Artık kitapları inceleyebilir ve rezervasyon yapabilirsin.</p>
        <br>
        <b>Pastel Kütüphane Yönetimi</b>
    ";

            // Arka planda gönder (Kullanıcıyı bekletmesin)
            Task.Run(() => MailGonderici.Gonder(yeniKullanici.Email, "Hoş Geldin! 🎉", mailIcerik));
            // ----------------------------------

            return Ok(new { mesaj = "Kayıt başarılı!" });
        }

        // --- GÜNCELLENEN GİRİŞ KISMI ---
        [HttpPost("giris")]
        public IActionResult GirisYap([FromBody] Kullanici girisYapan)
        {
            var kullanici = _context.Kullanicilar.FirstOrDefault(x => x.Email == girisYapan.Email);

            if (kullanici == null || !SifreIslemleri.Dogrula(girisYapan.Sifre, kullanici.Sifre))
            {
                return Unauthorized(new { mesaj = "E-posta veya şifre hatalı!" });
            }

            // 1. Session (Hafıza) Ayarları - İSİMLER ARTIK UNUTULMAYACAK
            HttpContext.Session.SetString("UserSession", kullanici.Email);
            HttpContext.Session.SetString("UserAd", kullanici.Ad ?? "");
            HttpContext.Session.SetString("UserSoyad", kullanici.Soyad ?? "");

            if (kullanici.IsAdmin == true)
                HttpContext.Session.SetString("AdminRole", "Admin");

            // 2. Loglama (Yeni Format: Ad ve Soyad ile)
            Loglayici.Kaydet(_context, kullanici.Email, kullanici.Ad, kullanici.Soyad, "Giriş", "API üzerinden giriş yapıldı.");

            return Ok(new
            {
                mesaj = "Giriş başarılı",
                ad = kullanici.Ad,
                email = kullanici.Email,
                isAdmin = kullanici.IsAdmin
            });
        }

        [HttpGet("bilgi/{email}")]
        public IActionResult BilgileriGetir(string email)
        {
            var kullanici = _context.Kullanicilar.FirstOrDefault(x => x.Email == email);
            if (kullanici == null) return NotFound("Kullanıcı bulunamadı.");

            return Ok(new { kullanici.Ad, kullanici.Soyad, kullanici.Email, kullanici.IsAdmin });
        }

        // --- GÜNCELLENEN PROFİL GÜNCELLEME KISMI ---
        [HttpPut("guncelle")]
        public IActionResult ProfilGuncelle([FromBody] KullaniciGuncelleModel gelenVeri)
        {
            var kullanici = _context.Kullanicilar.FirstOrDefault(x => x.Email == gelenVeri.Email);
            if (kullanici == null) return NotFound(new { mesaj = "Kullanıcı bulunamadı." });

            if (!SifreIslemleri.Dogrula(gelenVeri.MevcutSifre, kullanici.Sifre))
            {
                // HATA LOGU (Yeni Format)
                Loglayici.Kaydet(_context, gelenVeri.Email, kullanici.Ad, kullanici.Soyad, "Hata", "Profil güncellemede yanlış şifre girildi.");
                return BadRequest(new { mesaj = "Mevcut şifrenizi yanlış girdiniz!" });
            }

            List<string> degisiklikler = new List<string>();

            if (kullanici.Ad != gelenVeri.Ad) degisiklikler.Add($"Adını '{gelenVeri.Ad}' yaptı");
            if (kullanici.Soyad != gelenVeri.Soyad) degisiklikler.Add($"Soyadını '{gelenVeri.Soyad}' yaptı");

            kullanici.Ad = gelenVeri.Ad;
            kullanici.Soyad = gelenVeri.Soyad;

            // Session'ı da güncelle ki sayfa yenilenince eski isim gelmesin
            HttpContext.Session.SetString("UserAd", kullanici.Ad);
            HttpContext.Session.SetString("UserSoyad", kullanici.Soyad);

            bool sifreDegistiMi = false;
            if (!string.IsNullOrEmpty(gelenVeri.YeniSifre))
            {
                if (SifreIslemleri.Dogrula(gelenVeri.YeniSifre, kullanici.Sifre))
                    return BadRequest(new { mesaj = "Yeni şifre eskisiyle aynı olamaz." });

                kullanici.Sifre = SifreIslemleri.Sifrele(gelenVeri.YeniSifre);
                sifreDegistiMi = true;
                degisiklikler.Add("Şifresini değiştirdi");
            }

            _context.SaveChanges();

            //LOG KISMI
            if (degisiklikler.Count > 0)
            {
                string logMesaji = string.Join(", ", degisiklikler) + ".";
                Loglayici.Kaydet(_context, gelenVeri.Email, kullanici.Ad, kullanici.Soyad, "Profil Güncelleme", logMesaji);
            }
            if (sifreDegistiMi)
            {
                Loglayici.Kaydet(_context, gelenVeri.Email, kullanici.Ad, kullanici.Soyad, "Çıkış", "Şifre değişikliği nedeniyle otomatik çıkış yapıldı.");

                // Backend tarafındaki oturumu temizler
                HttpContext.Session.Clear();
            }

            return Ok(new { mesaj = "Profil güncellendi.", sifreDegisti = sifreDegistiMi });
        }
    }
}