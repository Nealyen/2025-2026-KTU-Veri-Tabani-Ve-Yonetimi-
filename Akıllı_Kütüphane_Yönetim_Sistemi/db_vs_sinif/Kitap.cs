namespace Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif
{
    public class Kitap
    {
        public int Id { get; set; } 
        public string Ad { get; set; } 
        public string Yazar { get; set; } 
        public int YayinYili { get; set; } 
        public string ISBN { get; set; }    //Kitap numarası ISBN
    }
}
