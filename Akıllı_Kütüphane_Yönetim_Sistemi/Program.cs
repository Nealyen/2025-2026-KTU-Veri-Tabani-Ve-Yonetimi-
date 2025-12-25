using Microsoft.EntityFrameworkCore;
using Akıllı_Kütüphane_Yönetim_Sistemi.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantısı (Artık appsettings.json'dan okuyor - Güvenli)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Hafıza (Session) Ayarları
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // 20 dk işlem yapmazsa oturum düşer
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3. Controller Servisleri
builder.Services.AddControllersWithViews();

// 4. Swagger (API Testi İçin)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- UYGULAMA AYARLARI ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles(); // HTML, CSS, JS dosyalarını açar

app.UseRouting();

app.UseAuthorization();

app.UseSession(); // Session'ı aktif et

// Varsayılan rota (MVC kalıntısı ama API projelerinde de zararı yok)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();