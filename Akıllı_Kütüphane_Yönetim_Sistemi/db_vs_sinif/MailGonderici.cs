using System.Net;
using System.Net.Mail;

namespace Akıllı_Kütüphane_Yönetim_Sistemi
{
    public static class MailGonderici
    {
        // GÖNDERİCİ AYARLARI 
        private static string GonderenMail = "nealyen1441@gmail.com";
        private static string GonderenSifre = "ncycjknjkjympxfo"; 

        public static void Gonder(string aliciEmail, string konu, string icerik)
        {
            try
            {
                // Postane (SMTP) Ayarları
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.EnableSsl = true; // Güvenli bağlantı için ayar 
                smtp.Credentials = new NetworkCredential(GonderenMail, GonderenSifre);

                // MAİL HAZIRLIK KISMI
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(GonderenMail, "Nealyen'in Kütüphanesi"); 
                mail.To.Add(aliciEmail);
                mail.Subject = konu;
                mail.Body = icerik;
                mail.IsBodyHtml = true; // HTML tasarımı desteklemesi için

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                // Mail gönderirken hata olursa program çökmesin, sadece konsola yazsın.
                Console.WriteLine("Mail Gönderme Hatası: " + ex.Message);
            }
        }
    }
}