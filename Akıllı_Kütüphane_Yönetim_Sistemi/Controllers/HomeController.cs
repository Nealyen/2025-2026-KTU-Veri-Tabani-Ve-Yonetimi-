using Akıllı_Kütüphane_Yönetim_Sistemi.db_kriter; // Yardımcı aşçıyı (Kriterleri) çağırmak için bunu eklemelisin
using Microsoft.AspNetCore.Mvc;

namespace Akıllı_Kütüphane_Yönetim_Sistemi.Controllers
{
    public class HomeController : Controller
    {
        // Sitenin ana sayfasına girildiğinde burası çalışır
        public IActionResult Index()
        {
            var yardimciAsci = new KitapKriterleri();

            var gosterilecekKitaplar = yardimciAsci.ListelemekIcinKitaplariHazirla();

            return View(gosterilecekKitaplar);
        }
    }
}