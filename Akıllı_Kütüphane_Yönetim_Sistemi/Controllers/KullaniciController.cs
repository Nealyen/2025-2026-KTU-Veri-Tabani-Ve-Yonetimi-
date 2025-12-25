using Microsoft.AspNetCore.Mvc;
using Akıllı_Kütüphane_Yönetim_Sistemi.Data;
using Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif;
using System.Data.SqlClient;

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

        // KAYIT OLMA KISMI 
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
            _context.SaveChanges();
            return Ok(new { mesaj = "Kayıt başarılı!" });
        }

        // GİRİŞ YAPMA KISMI
        [HttpPost("giris")]
        public IActionResult GirisYap([FromBody] Kullanici girisYapan)
        {
            // önce E-Posta ile kullanıcı bulunur
            var kullanici = _context.Kullanicilar.FirstOrDefault(x => x.Email == girisYapan.Email);

            // Kullanıcı yoksa hata versin
            if (kullanici == null)
            {
                return Unauthorized(new { mesaj = "E-posta veya şifre hatalı!" });
            }

            // Şifre Kontrolu hash kısmındaki dogrulama methodu çalısır ve
            // Girilen şifreyi, veritabanındaki hash ile karşılaştırırız
            if (!SifreIslemleri.Dogrula(girisYapan.Sifre, kullanici.Sifre))
            {
                return Unauthorized(new { mesaj = "E-posta veya şifre hatalı!" });
            }

            // Şifre doğruysa çalışmaya devam eder
            return Ok(new
            {
                mesaj = "Giriş başarılı",
                ad = kullanici.Ad,
                soyad = kullanici.Soyad,
                email = kullanici.Email,
                isAdmin = kullanici.IsAdmin
            });
        }

        [HttpGet("bilgi/{email}")]      // Profil bilgilerinin gösterilme kısmı
        public IActionResult BilgileriGetir(string email)
        {
            var kullanici = _context.Kullanicilar.FirstOrDefault(x => x.Email == email);
            if (kullanici == null) return NotFound("Kullanıcı bulunamadı.");

            return Ok(new
            {
                kullanici.Ad,
                kullanici.Soyad,
                kullanici.Email,
                kullanici.Sifre,
                kullanici.IsAdmin
            });
        }

        // GÜNCELLEME YAPMA KISMI
        [HttpPut("guncelle")]
        public IActionResult ProfilGuncelle([FromBody] KullaniciGuncelleModel gelenVeri)
        {
            if (gelenVeri == null || string.IsNullOrEmpty(gelenVeri.Email))
            {
                return BadRequest(new { mesaj = "Email bilgisi eksik." });
            }

            var kullanici = _context.Kullanicilar.FirstOrDefault(x => x.Email == gelenVeri.Email);

            if (kullanici == null)
            {
                return NotFound(new { mesaj = "Kullanıcı bulunamadı." });
            }

            // isim Güncelleme alanı
            if (!string.IsNullOrEmpty(gelenVeri.AdSoyad))
            {
                string[] isimler = gelenVeri.AdSoyad.Trim().Split(' ');
                if (isimler.Length >= 2)
                {
                    kullanici.Ad = isimler[0];
                    kullanici.Soyad = string.Join(" ", isimler.Skip(1));
                }
                else
                {
                    kullanici.Ad = isimler[0];
                    kullanici.Soyad = "";
                }
            }

            //  sifre güncelleme alanı
            if (!string.IsNullOrEmpty(gelenVeri.Sifre))
            {
                kullanici.Sifre = SifreIslemleri.Sifrele(gelenVeri.Sifre);
            }

            try
            {
                _context.SaveChanges();
                return Ok(new { mesaj = "Profil ve Şifre başarıyla güncellendi!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mesaj = "Hata oluştu.", detay = ex.Message });
            }
        }

    }
}