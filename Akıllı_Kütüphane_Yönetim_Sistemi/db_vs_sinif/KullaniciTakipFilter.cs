using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Controllers;
using Akıllı_Kütüphane_Yönetim_Sistemi.Data;
using Akıllı_Kütüphane_Yönetim_Sistemi.db_vs_sinif;

namespace Akıllı_Kütüphane_Yönetim_Sistemi
{
    public class KullaniciTakipFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                var email = context.HttpContext.Session.GetString("UserSession");

                // Eğer giriş yapılmışsa çalışsın
                if (!string.IsNullOrEmpty(email))
                {
                    // Session'dan Ad ve Soyad bilgisini çek 
                    string ad = context.HttpContext.Session.GetString("UserAd") ?? "Bilinmiyor";
                    string soyad = context.HttpContext.Session.GetString("UserSoyad") ?? "";

                    var dbContext = context.HttpContext.RequestServices.GetService<ApplicationDbContext>();
                    var descriptor = context.ActionDescriptor as ControllerActionDescriptor;

                    if (descriptor != null)
                    {
                        //  SAYFA İSİMLERİNİN TÜRKÇELEŞTİRMESİ
                        string sayfaAdi = "";
                        string controller = descriptor.ControllerName; // Örn: Home
                        string action = descriptor.ActionName;         // Örn: Index

                        if (controller == "Home" && action == "Index") sayfaAdi = "Ana Sayfa";
                        else if (controller == "Home" && action == "KitapListesi") sayfaAdi = "Kitaplar Sayfası";
                        else if (controller == "Rezervasyon") sayfaAdi = "Rezervasyon Sayfası";
                        else if (controller == "Odunc" || action == "OduncListele") sayfaAdi = "Ödünç Sayfası";
                        // Eğer özel bir isim yoksa standart hali yazsın
                        else sayfaAdi = $"{controller} / {action}";

                        // SADECE GÖRÜNTÜLEME İŞLEMLERİNİ "GET" KAYDET (POST işlemleri değil)
                        if (context.HttpContext.Request.Method == "GET")
                        {
                            Loglayici.Kaydet(dbContext, email, ad, soyad, "Gezinti", $"{sayfaAdi} görüntülendi.");
                        }
                    }
                }
            }
            catch { }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}