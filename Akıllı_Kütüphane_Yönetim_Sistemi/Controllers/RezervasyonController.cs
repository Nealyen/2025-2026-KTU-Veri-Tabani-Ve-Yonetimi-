using Microsoft.AspNetCore.Mvc;
using Akıllı_Kütüphane_Yönetim_Sistemi.Data;
using Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif;

namespace Akıllı_Kütüphane_Yönetim_Sistemi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RezervasyonController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RezervasyonController(ApplicationDbContext context)
        {
            _context = context;
        }

        // İşlemi yapan profilin adını soyadını bul
        private (string Ad, string Soyad) KimYapiyor(string email)
        {
            // Önce Session'a bakılır
            string ad = HttpContext.Session.GetString("UserAd");
            string soyad = HttpContext.Session.GetString("UserSoyad");

            // Session boşsa veritabanından bulunur , nolur nolmaz diye kontrol
            if (string.IsNullOrEmpty(ad))
            {
                var user = _context.Kullanicilar.FirstOrDefault(u => u.Email == email);
                if (user != null) return (user.Ad, user.Soyad);
                return ("Bilinmiyor", "");
            }
            return (ad, soyad);
        }

        [HttpPost("olustur")]
        public IActionResult TalepOlustur([FromBody] Rezervasyon yeniTalep)
        {
            var aktifTalep = _context.Rezervasyonlar
                .FirstOrDefault(r => r.KullaniciEmail == yeniTalep.KullaniciEmail
                                  && (r.OnayDurumu == "Onaylandı" || r.OnayDurumu == "Bekliyor"));

            if (aktifTalep != null)
            {
                string hataMesaji = aktifTalep.OnayDurumu == "Bekliyor"
                    ? "Zaten onay bekleyen bir talebiniz var!"
                    : "Elinizde zaten teslim etmediğiniz bir kitap var!";
                return BadRequest(new { mesaj = hataMesaji });
            }

            var kitap = _context.Kitaplar.Find(yeniTalep.KitapID);
            if (kitap == null) return NotFound(new { mesaj = "Kitap bulunamadı." });
            if (kitap.KalanKitap <= 0) return BadRequest(new { mesaj = "Stokta kitap kalmadı!" });

            kitap.KalanKitap -= 1;

            yeniTalep.TalepTarihi = DateTime.Now;
            yeniTalep.OnayDurumu = "Bekliyor";
            _context.Rezervasyonlar.Add(yeniTalep);
            _context.SaveChanges();

            // LOG KISMI
            var (ad, soyad) = KimYapiyor(yeniTalep.KullaniciEmail);
            Loglayici.Kaydet(_context, yeniTalep.KullaniciEmail, ad, soyad, "Rezervasyon", $"{kitap.KitapAdi} kitabı için talep oluşturuldu.");

            return Ok(new { mesaj = "Talep alındı! Admin onayı bekleniyor." });
        }

        [HttpGet("listele")]
        public IActionResult Listele([FromQuery] string email, [FromQuery] bool isAdmin)
        {
            if (isAdmin)
            {
                var liste = _context.Rezervasyonlar
                    .ToList()
                    .OrderBy(x => x.OnayDurumu == "Bekliyor" ? 0 : 1)
                    .ThenByDescending(x => x.TalepTarihi)
                    .Select(r => new {
                        r.RezervasyonID,
                        r.KullaniciEmail,
                        r.TalepTarihi,
                        r.OnayDurumu,
                        KitapAdi = _context.Kitaplar.FirstOrDefault(k => k.KitapID == r.KitapID)?.KitapAdi ?? "Silinmiş Kitap",
                        r.KitapID
                    }).ToList();
                return Ok(liste);
            }
            else
            {
                var liste = _context.Rezervasyonlar
                    .Where(r => r.KullaniciEmail == email)
                    .OrderByDescending(r => r.TalepTarihi)
                    .Select(r => new {
                        r.RezervasyonID,
                        r.KullaniciEmail,
                        r.TalepTarihi,
                        r.OnayDurumu,
                        KitapAdi = _context.Kitaplar.FirstOrDefault(k => k.KitapID == r.KitapID).KitapAdi
                    }).ToList();
                return Ok(liste);
            }
        }

        // Admin Tarafından Onyalama kısmı
        [HttpPost("onayla/{id}")]
        public IActionResult Onayla(int id)
        {
            var talep = _context.Rezervasyonlar.Find(id);
            if (talep == null) return NotFound("Talep bulunamadı.");

            talep.OnayDurumu = "Onaylandı";
            talep.OnayTarihi = DateTime.Now;

            var yeniOdunc = new OduncIslem
            {
                KitapID = talep.KitapID,
                KullaniciEmail = talep.KullaniciEmail,
                AlisTarihi = DateTime.Now,
                SonTeslimTarihi = DateTime.Now.AddDays(14)
            };
            _context.OduncIslemler.Add(yeniOdunc);

            var kitap = _context.Kitaplar.Find(talep.KitapID);
            if (kitap != null) kitap.AlinanKitapSayisi += 1;

            _context.SaveChanges();

            // Loglama kısmı
            string adminEmail = HttpContext.Session.GetString("UserSession") ?? "Admin";
            var (adminAd, adminSoyad) = KimYapiyor(adminEmail);
            Loglayici.Kaydet(_context, adminEmail, adminAd, adminSoyad, "İşlem", $"{talep.KullaniciEmail} kullanıcısının {kitap?.KitapAdi} talebini onayladı.");

            // Mail Gönderme Kısmı
            string mailIcerik = $@"
                <h3>Müjde! Kitap Talebin Onaylandı ✅</h3>
                <p>Merhaba, talep ettiğin <b>{kitap?.KitapAdi}</b> kitabı senin için ayrıldı.</p>
                <p><b>14 Gün</b> süren başladı. Lütfen zamanında teslim etmeyi unutma.</p>
                <br>
                <b>İyi Okumalar!</b>
            ";
            Task.Run(() => MailGonderici.Gonder(talep.KullaniciEmail, "Kitap Talebin Onaylandı", mailIcerik));

            return Ok(new { mesaj = "Rezervasyon onaylandı ve kitap ödünç verildi! ✅" });
        }

        // Admin Tarafından Reddetme kısmı
        [HttpPost("reddet/{id}")]
        public IActionResult Reddet(int id)
        {
            var talep = _context.Rezervasyonlar.Find(id);
            if (talep == null) return NotFound("Talep bulunamadı.");

            talep.OnayDurumu = "Reddedildi";

            var kitap = _context.Kitaplar.Find(talep.KitapID);
            if (kitap != null) kitap.KalanKitap += 1;

            _context.SaveChanges();

            // Loglama Kısmı
            string adminEmail = HttpContext.Session.GetString("UserSession") ?? "Admin";
            var (adminAd, adminSoyad) = KimYapiyor(adminEmail);
            Loglayici.Kaydet(_context, adminEmail, adminAd, adminSoyad, "İşlem", $"{talep.KullaniciEmail} kullanıcısının {kitap?.KitapAdi} talebini reddetti.");

            // Mail Gönderme 
            string mailIcerik = $@"
                <h3>Kitap Talebin Reddedildi ❌</h3>
                <p>Merhaba, üzgünüz ama <b>{kitap?.KitapAdi}</b> kitabı için talebin şu an onaylanamadı.</p>
                <p>Lütfen daha sonra tekrar dene veya başka bir kitap seç.</p>
            ";
            Task.Run(() => MailGonderici.Gonder(talep.KullaniciEmail, "Talep Durumu Hakkında", mailIcerik));

            return Ok(new { mesaj = "Talep reddedildi, kitap stoğa geri döndü. ❌" });
        }

        [HttpGet("odunc-listele")]
        public IActionResult OduncListele(string email, bool isAdmin)
        {
            var sorgu = from o in _context.OduncIslemler
                        join k in _context.Kitaplar on o.KitapID equals k.KitapID
                        select new { o, k };

            if (!isAdmin) sorgu = sorgu.Where(x => x.o.KullaniciEmail == email);

            var sonucListesi = sorgu.OrderByDescending(x => x.o.AlisTarihi)
                .Select(x => new {
                    x.o.IslemID,
                    x.o.KullaniciEmail,
                    KitapAdi = x.k.KitapAdi,
                    x.o.AlisTarihi,
                    x.o.SonTeslimTarihi,
                    x.o.TeslimTarihi,
                    x.o.ParaCezasi
                }).ToList();

            return Ok(sonucListesi);
        }

        [HttpDelete("iptal/{id}")]
        public IActionResult TalepIptal(int id)
        {
            var talep = _context.Rezervasyonlar.Find(id);
            if (talep == null) return NotFound(new { mesaj = "Talep bulunamadı." });

            if (talep.OnayDurumu != "Bekliyor") return BadRequest(new { mesaj = "Sadece 'Bekliyor' durumundaki talepler iptal edilebilir." });

            var kitap = _context.Kitaplar.Find(talep.KitapID);
            if (kitap != null) kitap.KalanKitap += 1;

            // Silinmeden önce bilgiler alınır
            string kullaniciEmail = talep.KullaniciEmail;
            string kitapAdi = kitap != null ? kitap.KitapAdi : "Kitap";

            _context.Rezervasyonlar.Remove(talep);
            _context.SaveChanges();

            // Loglama Kısmı
            var (ad, soyad) = KimYapiyor(kullaniciEmail);
            Loglayici.Kaydet(_context, kullaniciEmail, ad, soyad, "Rezervasyon", $"{kitapAdi} için oluşturduğu talebi iptal etti.");

            // Mail Gönderme 
            string mailIcerik = $@"
                <h3>Rezervasyon İptali Başarılı 🗑️</h3>
                <p>Merhaba,</p>
                <p><b>{kitapAdi}</b> kitabı için oluşturduğun rezervasyon talebi, isteğin üzerine iptal edilmiştir.</p>
                <p>Başka kitapları incelemek istersen her zaman bekleriz.</p>
                <br>
                <b>Pastel Kütüphane</b>
            ";
            Task.Run(() => MailGonderici.Gonder(kullaniciEmail, "Rezervasyon İptal Edildi", mailIcerik));

            return Ok(new { mesaj = "Talep iptal edildi ve silindi. 🗑️" });
        }

        // İade Alma kısmı (Admin Özel)
        [HttpPost("iade-et/{id}")]
        public IActionResult IadeEt(int id)
        {
            var kayit = _context.OduncIslemler.Find(id);
            if (kayit == null) return NotFound(new { mesaj = "Kayıt bulunamadı!" });

            kayit.TeslimTarihi = DateTime.Now;

            // Ceza Hesaplama
            if (kayit.TeslimTarihi > kayit.SonTeslimTarihi)
            {
                TimeSpan fark = kayit.TeslimTarihi.Value - kayit.SonTeslimTarihi.Value;
                int gecikenGun = (int)Math.Ceiling(fark.TotalDays);
                kayit.ParaCezasi = gecikenGun > 0 ? gecikenGun * 100 : 0;
            }
            else kayit.ParaCezasi = 0;

            // Stok Güncelleme
            var kitap = _context.Kitaplar.Find(kayit.KitapID);
            if (kitap != null)
            {
                kitap.KalanKitap += 1;
                if (kitap.AlinanKitapSayisi > 0) kitap.AlinanKitapSayisi -= 1;
            }

            var rezervasyon = _context.Rezervasyonlar.FirstOrDefault(r => r.KitapID == kayit.KitapID && r.KullaniciEmail == kayit.KullaniciEmail && r.OnayDurumu == "Onaylandı");
            if (rezervasyon != null)
            {
                rezervasyon.OnayDurumu = "İade Edildi";
                rezervasyon.OnayTarihi = DateTime.Now;
            }

            _context.SaveChanges();

            //Loglama
            string adminEmail = HttpContext.Session.GetString("UserSession") ?? "Admin";
            var (adminAd, adminSoyad) = KimYapiyor(adminEmail);
            Loglayici.Kaydet(_context, adminEmail, adminAd, adminSoyad, "İşlem", $"{kayit.KullaniciEmail} kullanıcısından {kitap?.KitapAdi} kitabını iade aldı.");

            // Mail Gönderme Kısmı
            string mailIcerik = $@"
                <h3>Teşekkürler! Kitap İadesi Alındı 📚</h3>
                <p><b>{kitap?.KitapAdi}</b> kitabını başarıyla teslim ettin.</p>
                {(kayit.ParaCezasi > 0 ? $"<p style='color:red;'><b>⚠️ Gecikme Cezası: {kayit.ParaCezasi} TL</b></p>" : "<p>Zamanında teslim ettiğin için teşekkürler.</p>")}
            ";
            Task.Run(() => MailGonderici.Gonder(kayit.KullaniciEmail, "İade İşlemi Tamamlandı", mailIcerik));

            return Ok(new { mesaj = "İade başarıyla alındı.", ceza = kayit.ParaCezasi, teslimTarihi = kayit.TeslimTarihi });
        }

        // ÖDÜNÇ SAYFASINDAKİ TURUNCU BUTON
        [HttpPost("odunc-arsivle")]
        public IActionResult OduncArsivle()
        {
            var bitmisIslemler = _context.OduncIslemler.Where(x => x.TeslimTarihi != null).ToList();
            if (!bitmisIslemler.Any()) return Ok(new { mesaj = "Arşivlenecek tamamlanmış işlem bulunamadı." });

            foreach (var islem in bitmisIslemler)
            {
                var log = new OduncLog
                {
                    EskiIslemID = islem.IslemID,
                    KullaniciEmail = islem.KullaniciEmail,
                    KitapID = islem.KitapID,
                    AlisTarihi = islem.AlisTarihi,
                    TeslimTarihi = islem.TeslimTarihi,
                    SonTeslimTarihi = islem.SonTeslimTarihi,
                    ParaCezasi = islem.ParaCezasi,
                    ArsivlenmeTarihi = DateTime.Now
                };
                _context.OduncLoglari.Add(log);
                _context.OduncIslemler.Remove(islem);
            }
            _context.SaveChanges();

            string adminEmail = HttpContext.Session.GetString("UserSession") ?? "Admin";
            var (adminAd, adminSoyad) = KimYapiyor(adminEmail);

            Loglayici.Kaydet(_context, adminEmail, adminAd, adminSoyad, "Arşivleme", "Ödünç alınmıs kitaplar Loglandı");
            

            return Ok(new { mesaj = $"{bitmisIslemler.Count} adet işlem arşivlendi! 📦" });
        }

        // REZERVASYON SAYFASINDAKİ TURUNCU BUTON 
        [HttpPost("rezervasyon-arsivle")]
        public IActionResult RezervasyonArsivle()
        {
            var bitmisIslemler = _context.Rezervasyonlar.Where(x => x.OnayDurumu != "Bekliyor" && x.OnayDurumu != "Onaylandı").ToList();
            if (!bitmisIslemler.Any()) return Ok(new { mesaj = "Arşivlenecek tamamlanmış işlem yok." });

            foreach (var islem in bitmisIslemler)
            {
                var log = new RezervasyonLog
                {
                    EskiRezervasyonID = islem.RezervasyonID,
                    KullaniciEmail = islem.KullaniciEmail,
                    KitapID = islem.KitapID,
                    TalepTarihi = islem.TalepTarihi,
                    OnayTarihi = islem.OnayTarihi,
                    OnayDurumu = islem.OnayDurumu,
                    ArsivlenmeTarihi = DateTime.Now
                };
                _context.RezervasyonLoglari.Add(log);
                _context.Rezervasyonlar.Remove(islem);
            }
            _context.SaveChanges();

            // Loglama Kısmı
            string adminEmail = HttpContext.Session.GetString("UserSession") ?? "Admin";
            var (adminAd, adminSoyad) = KimYapiyor(adminEmail);

            Loglayici.Kaydet(_context, adminEmail, adminAd, adminSoyad, "Arşivleme", "Rezervasyon kısmındaki işlemler Loglandı");
            

            return Ok(new { mesaj = $"{bitmisIslemler.Count} adet geçmiş rezervasyon arşivlendi! 📦" });
        }
    }
}