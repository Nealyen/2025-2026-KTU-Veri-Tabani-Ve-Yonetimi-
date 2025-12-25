
//PROJENİN HTML KISMI İLE İLGİLENİYOR İŞLEMLER ORASI İLE ALAKALI İŞLEMLER BURADA YAPILIYOR KULLANICICONTROLLERDEN FARKLI 
using Microsoft.AspNetCore.Mvc;
using Akıllı_Kütüphane_Yönetim_Sistemi.Data;
using Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif;

namespace Akıllı_Kütüphane_Yönetim_Sistemi.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Zaten giriş yapmışsa direkt anasayfaya at
            if (HttpContext.Session.GetString("UserSession") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string sifre)
        {
            // Önce sadece E-Posta ile kullanıcıyı bul
            var user = _context.Kullanicilar.FirstOrDefault(u => u.Email == email);

            // Kullanıcı varsa Ve şifresi (Hashli hali) doğruysa
            if (user != null && SifreIslemleri.Dogrula(sifre, user.Sifre))
            {
                HttpContext.Session.SetString("UserSession", user.Email);
                HttpContext.Session.SetString("UserAd", user.Ad ?? "");      
                HttpContext.Session.SetString("UserSoyad", user.Soyad ?? ""); 

                if (user.IsAdmin == true)
                {
                    HttpContext.Session.SetString("AdminRole", "Admin");
                }
                Loglayici.Kaydet(_context, user.Email, user.Ad, user.Soyad, "Giriş", "Sisteme giriş yapıldı.");

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Message = "Hatalı Email veya Şifre!";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(Kullanici yeniKullanici)
        {
            // Admin olamaz, standart kullanıcı
            yeniKullanici.IsAdmin = false;

            var emailVarMi = _context.Kullanicilar.Any(u => u.Email == yeniKullanici.Email);
            if (emailVarMi)
            {
                ViewBag.Message = "Bu Email adresi zaten kayıtlı!";
                return View();
            }

            yeniKullanici.Sifre = SifreIslemleri.Sifrele(yeniKullanici.Sifre);

            _context.Kullanicilar.Add(yeniKullanici);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            // Çıkış yapmadan önce hafızadaki bilgileri alalım log işlemleri için
            var email = HttpContext.Session.GetString("UserSession");
            var ad = HttpContext.Session.GetString("UserAd") ?? "";      
            var soyad = HttpContext.Session.GetString("UserSoyad") ?? ""; 

            if (email != null)
            {
               
                Loglayici.Kaydet(_context, email, ad, soyad, "Çıkış", "Kullanıcı çıkış yaptı.");
            }

            HttpContext.Session.Clear(); 
            return RedirectToAction("Login");
        }
    }
}