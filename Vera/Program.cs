using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Vera.Data;

var builder = WebApplication.CreateBuilder(args);

// [VERİTABANI YÖNETİMİ] - Ana veri işlemleri (Kullanıcılar, Projeler vb.) için SQLite bağlamını (CoreDbContext) yapılandırıyoruz.
// SQLite tercih etmemin sebebi geliştirme sürecinde hafif olması ve sunucu gerektirmemesidir.
builder.Services.AddDbContext<Vera.Data.CoreDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("CoreConnection")));

// [MİMARİ & SOLID] - Loglama işlemlerini ayrı bir veritabanı bağlamında (LogDbContext) tutarak
// Single Responsibility (Tek Sorumluluk) prensibine uyuyoruz. Ana veritabanı yorulmuyor.
builder.Services.AddDbContext<Vera.Data.LogDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("LogConnection")));

builder.Services.AddRazorPages();

// [GÜVENLİK & YETKİLENDİRME] - Cookie tabanlı kimlik doğrulama sisteminin ayarları.
// Kullanıcı giriş yapmamışsa yetkisiz sayfalarda otomatik olarak Login sayfasına yönlendirilecek.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login"; // Yetkisiz erişimlerde yönlendirilecek rota
        options.LogoutPath = "/Logout"; // Çıkış yapıldığında session'ın temizlendiği rota
        options.Cookie.Name = "VeraSessionAuth"; // Tarayıcıda tutulacak güvenlik çerezinin adı
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Kullanıcıyı 7 gün boyunca hatırla
    });

var app = builder.Build();

// [HATA YÖNETİMİ] - Canlı ortamda (Production) detaylı hata mesajlarını gizleyip,
// kullanıcıyı standart bir hata sayfasına yönlendiriyoruz (Güvenlik zafiyetini önlemek için).
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseStaticFiles();

// Eğer sayfa bulunamazsa (404) özel tasarladığımız hata sayfasına yönlendir.
app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");

// [GÜVENLİK PİPELINE] - Middleware sırası çok önemlidir. Önce kimlik doğrulanır, sonra yetki kontrolü yapılır.
app.UseAuthentication(); 
app.UseAuthorization();  

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

// [UYGULAMA BAŞLANGICI] - Uygulama ayağa kalkarken veritabanı eksikse otomatik oluşturulmasını sağlıyoruz.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var coreDb = services.GetRequiredService<CoreDbContext>();
    coreDb.Database.EnsureCreated(); // Migration kullanmadan hızlıca tabloları inşa eder
    
    // [EKSTRA ÖZELLİK] - Projenin boş gözükmemesi için dinamik ve rastgele verilerle (Seed Data) dolduruyoruz.
    // Şifreler PasswordHasher ile güvenli bir şekilde oluşturularak ekleniyor.
    DataSeeder.SeedData(coreDb);

    var logDb = services.GetRequiredService<LogDbContext>();
    logDb.Database.EnsureCreated(); // Sistem logları için gerekli tablolar
}

app.Run();
