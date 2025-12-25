using Microsoft.AspNetCore.Mvc;
using Akıllı_Kütüphane_Yönetim_Sistemi.Data;
using Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif;

namespace Akıllı_Kütüphane_Yönetim_Sistemi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KitapController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public KitapController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult TumKitaplariGetir()
        {
            // Veritabanındaki tüm kitapları çek
            var kitaplar = _context.Kitaplar.ToList();
            return Ok(kitaplar);
        }
    }
}