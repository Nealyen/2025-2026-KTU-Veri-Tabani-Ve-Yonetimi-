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
            var kitapServisi = new KitapKriterleri(_context);

            var gosterilecekKitaplar = kitapServisi.ListelemekIcinKitaplariHazirla();
            return View(gosterilecekKitaplar);
        }

        public IActionResult KitapEkle()
        {
            return View();
        }

        [HttpPost]
        public IActionResult KitapEkle(Kitap gelenKitap)
        {
            var kitapServisi = new KitapKriterleri(_context);

            kitapServisi.KitabiKontrolEtVeEkle(gelenKitap);
            return RedirectToAction("Index");
        }

        public IActionResult KitapSil(int id)
        {
            var kitapServisi = new KitapKriterleri(_context);

            kitapServisi.KitabiSistemdenKaldir(id);
            return RedirectToAction("Index");
        }

        public IActionResult KitapDuzenle(int id)
        {
            var kitapServisi = new KitapKriterleri(_context);

            var bulunanKitap = kitapServisi.DuzenlenecekKitabiGetir(id);

            if (bulunanKitap == null) return RedirectToAction("Index");

            return View(bulunanKitap);
        }

        [HttpPost]
        public IActionResult KitapDuzenle(Kitap gelenKitap)
        {
            var kitapServisi = new KitapKriterleri(_context);

            kitapServisi.KitapBilgileriniGuncelle(gelenKitap);
            return RedirectToAction("Index");
        }
    }
}