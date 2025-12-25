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

        [HttpPost("olustur")] //TALEP OLUSTURMA KISMI
        public IActionResult TalepOlustur([FromBody] Rezervasyon yeniTalep)
        {
           // Kullanıcının elinde teslim edilmemiş (Onaylandı) VEYA henüz onay bekleyen (Bekliyor) kitap var mı kontrol etmek için yapılan ayarlama
            // Kullanıcıya anlık max sadece 1 kitap hakkı tanıyoruz.
            var aktifTalep = _context.Rezervasyonlar
                .FirstOrDefault(r => r.KullaniciEmail == yeniTalep.KullaniciEmail
                                  && (r.OnayDurumu == "Onaylandı" || r.OnayDurumu == "Bekliyor"));

            if (aktifTalep != null)
            {
                string hataMesaji = aktifTalep.OnayDurumu == "Bekliyor"
                    ? "Zaten onay bekleyen bir talebiniz var! Önce onu iptal edin."
                    : "Elinizde zaten teslim etmediğiniz bir kitap var! Önce onu iade edin.";

                return BadRequest(new { mesaj = hataMesaji });
            }


            var kitap = _context.Kitaplar.Find(yeniTalep.KitapID);
            if (kitap == null) return NotFound(new { mesaj = "Kitap bulunamadı." });

            if (kitap.KalanKitap <= 0) return BadRequest(new { mesaj = "Stokta kitap kalmadı!" });

            kitap.KalanKitap -= 1; // Stoktan düşüyoruz

            yeniTalep.TalepTarihi = DateTime.Now;
            yeniTalep.OnayDurumu = "Bekliyor";
            _context.Rezervasyonlar.Add(yeniTalep);

            _context.SaveChanges();
            return Ok(new { mesaj = "Talep alındı! Admin onayı bekleniyor." });
        }

        //REZERVASYONLARI LİSTELE , Admin olan herkesi görür , Kullanıcılar kendilerininkini
        [HttpGet("listele")]
        public IActionResult Listele([FromQuery] string email, [FromQuery] bool isAdmin)
        {
            if (isAdmin)
            {
                EskiKayitlariArsivle();
            }

            if (isAdmin)
            {
                var liste = _context.Rezervasyonlar
                    .ToList() // Önce veriyi listede tutuyoruz
                    .OrderBy(x => x.OnayDurumu == "Bekliyor" ? 0 : 1) 
                    .ThenByDescending(x => x.TalepTarihi) // tarihe göre sıralama
                    .Select(r => new {
                        r.RezervasyonID,
                        r.KullaniciEmail,
                        r.TalepTarihi,
                        r.OnayDurumu,
                        // Kitap adı null olmasın diye kontrol edilir
                        KitapAdi = _context.Kitaplar.FirstOrDefault(k => k.KitapID == r.KitapID)?.KitapAdi ?? "Silinmiş Kitap",
                        r.KitapID
                    }).ToList();

                return Ok(liste);
            }
            else
            {
                // Normal kullanıcı sadece kendi rezervasyonunu görür
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

        private void EskiKayitlariArsivle()
        {
            var silinecekTarih = DateTime.Now.AddDays(-3);

            // Hem reddedilenleri, hem de iade edilenleri bulma kısmı
            var eskiKayitlar = _context.Rezervasyonlar
                .Where(x => (x.OnayDurumu == "Reddedildi" || x.OnayDurumu == "İade Edildi")
                            && x.OnayTarihi < silinecekTarih)
                .ToList();

            if (eskiKayitlar.Count > 0)
            {
                foreach (var eski in eskiKayitlar)
                {
                    var log = new RezervasyonLog
                    {
                        EskiRezervasyonID = eski.RezervasyonID,
                        KullaniciEmail = eski.KullaniciEmail,
                        KitapID = eski.KitapID,
                        TalepTarihi = eski.TalepTarihi,
                        OnayTarihi = eski.OnayTarihi,
                        OnayDurumu = eski.OnayDurumu, // "İade Edildi" olarak kaydedilecek
                        ArsivlenmeTarihi = DateTime.Now
                    };
                    _context.RezervasyonLoglari.Add(log);
                    _context.Rezervasyonlar.Remove(eski);
                }
                _context.SaveChanges();
            }
        }

        // onaylama kısmı sadece Admin hesapları yapabilir
        [HttpPost("onayla/{id}")]
        public IActionResult Onayla(int id)
        {
            var talep = _context.Rezervasyonlar.Find(id);
            if (talep == null) return NotFound("Talep bulunamadı.");

            if (talep.OnayDurumu != "Bekliyor") return BadRequest("Bu talep zaten işlem görmüş.");

            // aktif durumu değiştirme
            talep.OnayDurumu = "Onaylandı";
            talep.OnayTarihi = DateTime.Now;

            //  Ödünç Tablosuna Eklenme kısmı onaylanırsa yapılan işlem
            var yeniOdunc = new OduncIslem
            {
                KitapID = talep.KitapID,
                KullaniciEmail = talep.KullaniciEmail,
                AlisTarihi = DateTime.Now,
                SonTeslimTarihi = DateTime.Now.AddDays(14) // Süre 14 gün
            };
            _context.OduncIslemler.Add(yeniOdunc);

            //   "Dışarıdaki Kitap" sayısını artır , kitap alındı
            // (Stoku zaten talep kısmında geçici olarak düşmüştük
            var kitap = _context.Kitaplar.Find(talep.KitapID);
            if (kitap != null) kitap.AlinanKitapSayisi += 1;

            _context.SaveChanges();
            return Ok(new { mesaj = "Rezervasyon onaylandı ve kitap ödünç verildi! ✅" });
        }

        // Reddetme kısmı sadece Admin
        [HttpPost("reddet/{id}")]
        public IActionResult Reddet(int id)
        {
            var talep = _context.Rezervasyonlar.Find(id);
            if (talep == null) return NotFound("Talep bulunamadı.");

            if (talep.OnayDurumu != "Bekliyor") return BadRequest("Zaten işlem yapılmış.");

            talep.OnayDurumu = "Reddedildi";

            // Depoya geri iade ettik , geçici kısımdan ilk bastaki kısma geri döndü
            var kitap = _context.Kitaplar.Find(talep.KitapID);
            if (kitap != null)
            {
                kitap.KalanKitap += 1;
            }

            _context.SaveChanges();
            return Ok(new { mesaj = "Talep reddedildi, kitap stoğa geri döndü. ❌" });
        }
        [HttpGet("odunc-listele")]
        public IActionResult OduncListele(string email, bool isAdmin)
        {
            // Tabloları birleştir
            var sorgu = from o in _context.OduncIslemler
                        join k in _context.Kitaplar on o.KitapID equals k.KitapID
                        select new { o, k };

            // Admin DEĞİLSE sadece kendi verisini görür
            // Admin ise herkes listelenir.
            if (!isAdmin)
            {
                sorgu = sorgu.Where(x => x.o.KullaniciEmail == email);
            }

            // Veriyi Hazırla ve Gönder
            var sonucListesi = sorgu
                .OrderByDescending(x => x.o.AlisTarihi)
                .Select(x => new
                {
                    x.o.IslemID,
                    // Admin kimin aldığını bilsin diye Email'i de gösteriyoruz
                    x.o.KullaniciEmail,
                    KitapAdi = x.k.KitapAdi,
                    AlisTarihi = x.o.AlisTarihi,
                    SonTeslimTarihi = x.o.SonTeslimTarihi,
                    TeslimTarihi = x.o.TeslimTarihi,
                    x.o.ParaCezasi
                })
                .ToList();

            return Ok(sonucListesi);
        }

        [HttpPost("iade-et/{id}")]
        public IActionResult IadeEt(int id)
        {
            var kayit = _context.OduncIslemler.Find(id);

            if (kayit == null) return NotFound(new { mesaj = $"Kayıt bulunamadı! Gönderilen ID: {id}" });

            kayit.TeslimTarihi = DateTime.Now;

            // Ceza Hesaplama Kısmı
            if (kayit.TeslimTarihi > kayit.SonTeslimTarihi)
            {
                TimeSpan fark = kayit.TeslimTarihi.Value - kayit.SonTeslimTarihi.Value; 
                int gecikenGun = (int)Math.Ceiling(fark.TotalDays);

                kayit.ParaCezasi = gecikenGun > 0 ? gecikenGun * 100 : 0;
            }
            else
            {
                kayit.ParaCezasi = 0;
            }

            // Stok Güncelleme Kısmı
            var kitap = _context.Kitaplar.Find(kayit.KitapID);
            if (kitap != null)
            {
                kitap.KalanKitap += 1;
                if (kitap.AlinanKitapSayisi > 0) kitap.AlinanKitapSayisi -= 1;
            }

            _context.SaveChanges();

            return Ok(new
            {
                mesaj = "İade başarıyla alındı.",
                ceza = kayit.ParaCezasi,
                teslimTarihi = kayit.TeslimTarihi
            });
        }

        // KUllanıcının kendisinin iptal ettiği kısım
        [HttpDelete("iptal/{id}")]
        public IActionResult TalepIptal(int id)
        {
            var talep = _context.Rezervasyonlar.Find(id);
            if (talep == null) return NotFound(new { mesaj = "Talep bulunamadı." });

            // Sadece "Bekliyor" olanlar iptal edilebilir. onaylanmıs veya reddedilmiş kitaplara işlem yapılamaz 
            if (talep.OnayDurumu != "Bekliyor")
            {
                return BadRequest(new { mesaj = "Sadece 'Bekliyor' durumundaki talepler iptal edilebilir." });
            }

            // Kitabın depoya geri eklenme kısmı
            var kitap = _context.Kitaplar.Find(talep.KitapID);
            if (kitap != null)
            {
                kitap.KalanKitap += 1;
            }

            //Veri işlemi iptal oldugundan log a kaydolmasın diye veri silinir
            _context.Rezervasyonlar.Remove(talep);

            _context.SaveChanges();
            return Ok(new { mesaj = "Talep iptal edildi ve silindi. 🗑️" });
        }

        [HttpPost("odunc-arsivle")]
        public IActionResult OduncArsivle()
        {
            // Sadece teslim edilmiş iade tarihleri gelmiş veya geçmiş işlemleri bul
            var bitmisIslemler = _context.OduncIslemler
                                    .Where(x => x.TeslimTarihi != null)
                                    .ToList();

            if (!bitmisIslemler.Any())
            {
                return Ok(new { mesaj = "Arşivlenecek tamamlanmış işlem bulunamadı." });
            }

            // Bunları ilgili Log tablosuna taşı
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

                _context.OduncLoglari.Add(log);      // Loga ekle
                _context.OduncIslemler.Remove(islem); // Listeden sil
            }

            _context.SaveChanges();
            return Ok(new { mesaj = $"{bitmisIslemler.Count} adet işlem başarıyla arşivlendi ve listeden temizlendi! 📦" });
        }
        [HttpPost("rezervasyon-arsivle")]
        public IActionResult RezervasyonArsivle()
        {
            //  Durumu 'Bekliyor' olmayan kayıtları bul
            var bitmisIslemler = _context.Rezervasyonlar
                                    .Where(x => x.OnayDurumu != "Bekliyor")
                                    .ToList();

            if (!bitmisIslemler.Any())
            {
                return Ok(new { mesaj = "Arşivlenecek tamamlanmış işlem yok." });
            }

            // Bunları ilgili Log tablosuna taşı
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

                _context.RezervasyonLoglari.Add(log);  // Loga ekle
                _context.Rezervasyonlar.Remove(islem); // Ana tablodan sil
            }

            _context.SaveChanges();
            return Ok(new { mesaj = $"{bitmisIslemler.Count} adet geçmiş rezervasyon arşivlendi! 📦" });
        }
    }
}