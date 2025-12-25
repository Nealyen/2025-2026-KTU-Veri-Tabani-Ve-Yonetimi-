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
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string sifre)
        {
            var user = _context.Kullanicilar
                               .FirstOrDefault(u => u.Email == email && u.Sifre == sifre);

            if (user != null)
            {
                HttpContext.Session.SetString("UserSession", user.Email);

                if (user.IsAdmin == true)
                {
                    HttpContext.Session.SetString("AdminRole", "Admin");
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Message = "Hatalı Email veya Şifre!";
                return View();
            }
        }

        
        [HttpGet]
        public IActionResult Register() //Kayıt Olma Kısmı sayfası
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(Kullanici yeniKullanici)
        {
            
            yeniKullanici.IsAdmin = false;  //kayıt olan account otomatik olarak standart profil oluyor

           
            var emailVarMi = _context.Kullanicilar.Any(u => u.Email == yeniKullanici.Email);
            if (emailVarMi)
            {
                ViewBag.Message = "Bu Email adresi zaten kayıtlı!";
                return View();
            }

            _context.Kullanicilar.Add(yeniKullanici);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}