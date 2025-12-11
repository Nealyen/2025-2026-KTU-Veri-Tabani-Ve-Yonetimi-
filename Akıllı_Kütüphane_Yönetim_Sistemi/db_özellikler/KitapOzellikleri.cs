using Akıllı_Kütüphane_Yönetim_Sistemi.Data;
using Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif;

namespace Akıllı_Kütüphane_Yönetim_Sistemi.db_özellikler
{
    public class KitapOzellikleri
    {
        private readonly ApplicationDbContext _context;

        
        public KitapOzellikleri(ApplicationDbContext context)
        {
            _context = context; //burası dışarıdan gelen context i alır ve sınıf içinde kullanır (this.context = context) misali
        }
        // dışarıdan hazır gelen db context i kullanmak için olusturulan constructor

        public List<Kitap> TumKitaplariGetir()
        {
            return _context.Kitaplar.ToList();
        }

        public void KitapEkle(Kitap yeniKitap)
        {
            _context.Kitaplar.Add(yeniKitap);
            _context.SaveChanges();
        }

        public void KitapSil(int silinecekId)
        {
            var silinecekKitap = _context.Kitaplar.Find(silinecekId);
            if (silinecekKitap != null)
            {
                _context.Kitaplar.Remove(silinecekKitap);
                _context.SaveChanges();
            }
        }

        public Kitap TekKitapGetir(int id)
        {
            return _context.Kitaplar.Find(id);
        }

        public void KitapGuncelle(Kitap guncellenecekKitap)
        {
            _context.Kitaplar.Update(guncellenecekKitap);
            _context.SaveChanges();
        }
        
        public void GecikmeVeCezaKontrolu()
        {
            
            var aktifOduncIslemler = _context.OduncIslemler
                                             .Where(x => x.TeslimTarihi == null)
                                             .ToList(); 
            //İADE Edilmemiş verileri bulmak içib teslim edilmemiş kitapları bul

            foreach (var islem in aktifOduncIslemler)
            {
                if (islem.AlisTarihi != null)
                {
                    
                    DateTime sonTeslim = islem.AlisTarihi.Value.AddDays(14); //son teslim tarihi olarak 14 gün seçtim

                    
                    islem.SonTeslimTarihi = sonTeslim;  //DB kısmındaki son teslim tarihini güncelle

                    
                    if (DateTime.Now > sonTeslim)   // bugun son teslim tarihini geçtiyse
                    {
                        
                        TimeSpan fark = DateTime.Now - sonTeslim;   //Gün hesaplaması 
                        int gecikenGun = (int)fark.TotalDays;

                        decimal cezaTutari = gecikenGun * 100;      //Gün basına 100 tl ceza tutarı ekle

                        var mevcutCeza = _context.Cezalar.FirstOrDefault(c => c.IslemID == islem.IslemID);
                         
                        if (mevcutCeza != null)  // Zaten ceza kaydı varsa tutarı güncelle (Her gün ekleniyor)
                        {
                           
                            mevcutCeza.GecikmeGunSayisi = gecikenGun;
                            mevcutCeza.CezaTutari = cezaTutari;
                            _context.Cezalar.Update(mevcutCeza);
                        }
                        else
                        {
                            
                            var yeniCeza = new Ceza  //İlk defa ceza veriliyorsa yeni kayıt aç
                            {
                                IslemID = islem.IslemID,
                                KullaniciEmail = islem.KullaniciEmail,
                                GecikmeGunSayisi = gecikenGun,
                                CezaTutari = cezaTutari,
                                OdendiMi = false 
                            };
                            _context.Cezalar.Add(yeniCeza);
                        }
                    }
                }
            }
            _context.SaveChanges();
        }
    }
}