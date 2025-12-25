using Akıllı_Kütüphane_Yönetim_Sistemi.db_kriter;
using Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif;
using Akıllı_Kütüphane_Yönetim_Sistemi.Data; 
using Microsoft.AspNetCore.Mvc;

namespace Akıllı_Kütüphane_Yönetim_Sistemi.Controllers
{
    public class HomeController : Controller
    {
        
        private readonly ApplicationDbContext _context; //DB bağlantısını tutan değişken

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Giriş yapmamışsa Login'e git
            if (HttpContext.Session.GetString("UserSession") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }
        
        public IActionResult KitapListesi()
        {
            var kitapYonetimi = new Akıllı_Kütüphane_Yönetim_Sistemi.db_özellikler.KitapOzellikleri(_context);
            var gosterilecekKitaplar = kitapYonetimi.TumKitaplariGetir();

            return View(gosterilecekKitaplar);
        }
        public IActionResult KitapEkle()
        {
            return View();
        }

        [HttpPost]
        public IActionResult KitapEkle(Kitap gelenKitap)
        {
            var kitapYonetimi = new KitapKriterleri(_context);

            kitapYonetimi.KitabiKontrolEtVeEkle(gelenKitap);
            return RedirectToAction("Index");
        }

        public IActionResult KitapSil(int id)
        {   
            var kitapYonetimi = new KitapKriterleri(_context);

            kitapYonetimi.KitabiSistemdenKaldir(id);
            return RedirectToAction("Index");
        }

        public IActionResult KitapDuzenle(int id)
        {
            var kitapYonetimi = new KitapKriterleri(_context);

            var bulunanKitap = kitapYonetimi.DuzenlenecekKitabiGetir(id);

            if (bulunanKitap == null) return RedirectToAction("Index");

            return View(bulunanKitap);
        }

        [HttpPost]
        public IActionResult KitapDuzenle(Kitap gelenKitap)
        {
            var kitapYonetimi = new KitapKriterleri(_context);

            kitapYonetimi.KitapBilgileriniGuncelle(gelenKitap);
            return RedirectToAction("Index");
        }
    }
}