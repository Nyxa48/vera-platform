using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Vera.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Budget = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsHourly = table.Column<bool>(type: "INTEGER", nullable: false),
                    EmployerId = table.Column<int>(type: "INTEGER", nullable: false),
                    FreelancerId = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectStages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    WalletBalance = table.Column<decimal>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    About = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Hours = table.Column<double>(type: "REAL", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkLogs_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Id", "Budget", "CreatedAt", "Description", "EmployerId", "FreelancerId", "IsHourly", "Status", "Title" },
                values: new object[,]
                {
                    { 1, 85000m, new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Şirketimiz için özelleştirilmiş ERP yazılımı geliştirilmesi. Muhasebe, stok yönetimi ve insan kaynakları modülleri olacak.", 4, null, false, "Açık", "Kurumsal ERP Sistemi" },
                    { 2, 450m, new DateTime(2026, 5, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Satış verilerinin analizi, görselleştirilmesi ve makine öğrenmesi ile tahminleme modeli oluşturulması.", 4, null, true, "Açık", "Python ile Veri Analizi" },
                    { 3, 300m, new DateTime(2026, 4, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kurumsal sitemizin Google sıralamasını yükseltme. Teknik SEO, içerik stratejisi ve backlink çalışması yapılacak.", 4, null, true, "Açık", "SEO Optimizasyonu" },
                    { 4, 12000m, new DateTime(2026, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Eğitim amaçlı platform oyunu. Fizik motorları, karakter animasyonları ve level design dahil.", 4, null, false, "Açık", "Unity 2D Oyun Geliştirme" },
                    { 5, 550m, new DateTime(2026, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Yavaş sorguların iyileştirilmesi, index stratejisi ve veritabanı normalizasyonu.", 4, null, true, "Açık", "PostgreSQL DB Optimizasyonu" },
                    { 6, 750m, new DateTime(2026, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Müşteri hizmetleri için OpenAI API kullanarak akıllı chatbot geliştirilmesi ve web sitesine entegrasyonu.", 4, null, true, "Açık", "AI Chatbot Entegrasyonu" },
                    { 7, 3000m, new DateTime(2026, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Firma tanıtım videoları için profesyonel hareketli logo animasyonu tasarlanması.", 4, null, false, "Açık", "Logo Animasyonu (After Effects)" },
                    { 8, 2500m, new DateTime(2026, 5, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Yeni e-ticaret markamız için minimalist, modern ve akılda kalıcı logo tasarımı.", 5, null, false, "Açık", "E-Ticaret Logo Tasarımı" },
                    { 9, 150m, new DateTime(2026, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Teknoloji ve yazılım üzerine haftalık 5 SEO uyumlu blog içeriği üretilmesi.", 5, null, true, "Açık", "Blog İçerik Yazarlığı" },
                    { 10, 5000m, new DateTime(2026, 4, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ana sayfa, ürün detay ve sepet sayfalarının modern trendlere göre yeniden tasarlanması.", 5, null, false, "Açık", "Figma UI/UX Revizesi" },
                    { 11, 200m, new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "15 dakikalık vlog içerikleri için profesyonel kurgu, renk düzeltme ve efektler.", 5, null, true, "Açık", "Video Kurgu (YouTube)" },
                    { 12, 18000m, new DateTime(2026, 4, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "API entegreli sosyal medya yönetim paneli. Instagram, Twitter ve TikTok desteği olacak.", 5, null, false, "Açık", "SMM Panel Yazılımı" },
                    { 13, 1500m, new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "30 adet özel vektörel ikon. Hem iOS hem Android uyumlu SVG formatında.", 5, null, false, "Açık", "Mobil Tasarım İkon Seti" },
                    { 14, 120m, new DateTime(2026, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Teknik yazılım dokümantasyonu çevirisi. 50 sayfa, terminolojiye hakim biri aranıyor.", 5, null, true, "Açık", "İngilizce-Türkçe Çeviri" },
                    { 15, 35000m, new DateTime(2026, 5, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Gerçek zamanlı teslimat takibi, harita entegrasyonu ve push notification desteği olan mobil uygulama.", 6, null, false, "Açık", "React Native Kurye Uygulaması" },
                    { 16, 600m, new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mevcut monolitik API'lerin mikroservis mimarisine geçirilmesi ve Docker container'larına taşınması.", 6, null, true, "Açık", "Dockerize Microservices" },
                    { 17, 3500m, new DateTime(2026, 4, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Liquid kodları ile özel bölümler ekleme, ürün filtreleme ve mega menü geliştirme.", 6, null, false, "Açık", "Shopify Tema Düzenleme" },
                    { 18, 20000m, new DateTime(2026, 4, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Web sitesi ve API'ler için kapsamlı sızma testi, güvenlik açığı taraması ve detaylı raporlama.", 6, null, false, "Açık", "Cyber Security Audit" },
                    { 19, 4000m, new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Özel tema geliştirme, WooCommerce entegrasyonu ve çok dilli destek.", 6, null, false, "Açık", "WordPress Kurumsal Site" },
                    { 20, 400m, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mevcut Flutter projesindeki performans sorunları ve bug'ların temizlenmesi.", 6, null, true, "Açık", "Flutter Hata Giderme" },
                    { 21, 6000m, new DateTime(2026, 3, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ubuntu sunucularda güvenlik sıkılaştırma, firewall konfigürasyonu ve SSH güvenliği.", 6, null, false, "Açık", "Linux Server Hardening" },
                    { 22, 500m, new DateTime(2026, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ürün arama altyapısı için Elasticsearch cluster kurulumu ve veri indeksleme.", 4, null, true, "Açık", "Elasticsearch Kurulumu" },
                    { 23, 8000m, new DateTime(2026, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Satış ve finans verilerinin Power BI ile görselleştirilmesi ve otomatik raporlama.", 5, null, false, "Açık", "Power BI Dashboard" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "About", "CreatedAt", "Email", "FullName", "Password", "Role", "Title", "WalletBalance" },
                values: new object[,]
                {
                    { 1, "10 yıllık deneyimli backend geliştirici. ASP.NET Core, Entity Framework, SQL Server ve mikroservis mimarileri konusunda uzmanım. Temiz kod yazarım, deadline'lara uyarım.", new DateTime(2025, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "emin@vera.com", "Emin Sancaklı", "AQAAAAIAAYagAAAAEFIGkRyQ1hytcCmuXF/MnvwHMBjUfpOcGMWjpKXQfau2Cwc/0jvWwqzzmVDF2FVWKg==", "Freelancer", "Senior .NET Developer", 45000m },
                    { 2, "Figma, Adobe XD ve Illustrator ile kullanıcı deneyimi odaklı tasarımlar yapıyorum. Minimalist ve modern arayüzler benim tutkum.", new DateTime(2025, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "ayse@vera.com", "Ayşe Demir", "AQAAAAIAAYagAAAAEJa+N/WKPy4ij6yO9KyxUaJduuWDQOb9qiB/XpaSOdT+ofSal0HcQa69UtzMKTn/Fw==", "Freelancer", "UI/UX & Marka Tasarımcısı", 28000m },
                    { 3, "React, Next.js, Node.js ve MongoDB ile modern web uygulamaları geliştiriyorum. 7 yıllık sektör deneyimim var.", new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "mehmet@vera.com", "Mehmet Kaya", "AQAAAAIAAYagAAAAEKSF/ZTgpU0Y00W/gJr1KK5SHNpsHA5EepCXB6XeEHi/tf70xqx78GWNH2UHtF6K5g==", "Freelancer", "Full Stack Developer (React & Node.js)", 62000m },
                    { 4, "Kurumsal yazılım çözümleri ve dijital dönüşüm hizmetleri sunuyoruz.", new DateTime(2024, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "info@zirve.com", "Zirve Teknoloji A.Ş.", "AQAAAAIAAYagAAAAEI0IMmlo7rrNW94vWsqAEEURRjCD2GjMrLXnKmlg3+ugXJORaXWfwu6ST9fUEl+Uaw==", "Employer", "Yazılım Ajansı", 100000m },
                    { 5, "Yaratıcı tasarım, sosyal medya yönetimi ve marka stratejisi alanında lider ajans.", new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "info@pixel.com", "Pixel Medya", "AQAAAAIAAYagAAAAEL0BkZ8Z+sPlZQUOSq/l8V9w5HNyCY/HKx/1P7dC6kX+b7Vf8Zmlfad+jX718Cuz/Q==", "Employer", "Dijital Ajans", 80000m },
                    { 6, "Türkiye genelinde lojistik ve dağıtım hizmetleri sunan köklü bir firma.", new DateTime(2024, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "info@global.com", "Global Lojistik", "AQAAAAIAAYagAAAAEKIwG+TblxK5YP22eEYsQpt4x2fjXPC9FO5y+1pBsyvxxnXSIGT2hLibHyj3iLRqLA==", "Employer", "Lojistik & Dağıtım", 250000m },
                    { 7, "Flutter ve Dart ile cross-platform mobil uygulamalar geliştiriyorum. Firebase, REST API entegrasyonları ve state management konularında deneyimliyim.", new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "zeynep@vera.com", "Zeynep Yıldız", "AQAAAAIAAYagAAAAEFA8yFaIjx/WPUknQEoFqLDXfBWwwOrltr4uJpFDrdSqMMVXrhPkJFniHuEeE7nmQQ==", "Freelancer", "Mobil Uygulama Geliştirici (Flutter)", 15500m },
                    { 8, "AWS, Azure, Docker, Kubernetes ve CI/CD pipeline'ları konusunda uzmanım. Altyapı optimizasyonu ve sunucu yönetimi yapıyorum.", new DateTime(2025, 4, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "can@vera.com", "Can Özturk", "AQAAAAIAAYagAAAAED90Jj1V3RIEblseuxAg9tipjxo9tJII3BRBIe3EqsSJg3a18V1JoBMrsFXqN3zekw==", "Freelancer", "DevOps & Cloud Mühendisi", 38000m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkLogs_ProjectId",
                table: "WorkLogs",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectStages");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "WorkLogs");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
