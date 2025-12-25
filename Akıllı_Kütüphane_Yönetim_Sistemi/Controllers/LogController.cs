using Microsoft.AspNetCore.Mvc;
using Akıllı_Kütüphane_Yönetim_Sistemi.Data;
using Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif;

namespace Akıllı_Kütüphane_Yönetim_Sistemi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LogController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("sayfa-goruntuleme")]
        public IActionResult SayfaLog([FromBody] SayfaLogModel model)
        {
            if (string.IsNullOrEmpty(model.Email)) return BadRequest();

            
            var kullanici = _context.Kullanicilar.FirstOrDefault(x => x.Email == model.Email);

            string ad = kullanici != null ? kullanici.Ad : "Bilinmiyor";
            string soyad = kullanici != null ? kullanici.Soyad : "";

            Loglayici.Kaydet(_context, model.Email, ad, soyad, "Gezinti", $"{model.SayfaAdi} görüntülendi.");

            return Ok();
        }
    }

    public class SayfaLogModel
    {
        public string Email { get; set; }
        public string SayfaAdi { get; set; }
    }
}