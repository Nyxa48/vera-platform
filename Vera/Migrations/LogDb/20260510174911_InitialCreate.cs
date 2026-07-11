using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Vera.Migrations.LogDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    Details = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SenderId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceiverId = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Logs",
                columns: new[] { "Id", "Action", "Details", "Timestamp", "UserId" },
                values: new object[,]
                {
                    { 1, "Yeni Teklif Kabul Edildi", "Kurumsal ERP Sistemi projesi için teklifiniz Zirve Teknoloji tarafından onaylandı.", new DateTime(2026, 5, 9, 14, 0, 0, 0, DateTimeKind.Unspecified), 1 },
                    { 2, "Hesaba Geçti", "₺12.500 tutarındaki hakedişiniz bakiyenize başarıyla eklendi.", new DateTime(2026, 5, 8, 10, 0, 0, 0, DateTimeKind.Unspecified), 1 },
                    { 3, "Sistem Güncellemesi", "Vera v2.0 yayında! Yeni özellikleri hemen keşfedin.", new DateTime(2026, 5, 6, 9, 0, 0, 0, DateTimeKind.Unspecified), 1 },
                    { 4, "Yeni Proje Eşleşmesi", "Yeteneklerinize uygun 'Mobil Uygulama Tasarımı' projesi yayınlandı.", new DateTime(2026, 5, 9, 11, 0, 0, 0, DateTimeKind.Unspecified), 2 },
                    { 5, "Yeni İş İlanı Yayınlandı", "React Native Kurye Uygulaması projesi yayına alındı.", new DateTime(2026, 5, 4, 16, 0, 0, 0, DateTimeKind.Unspecified), 6 },
                    { 6, "Ödeme Yapıldı", "Figma UI/UX Revizesi projesi için freelancer'a ₺5.000 ödeme yapıldı.", new DateTime(2026, 5, 7, 12, 30, 0, 0, DateTimeKind.Unspecified), 5 }
                });

            migrationBuilder.InsertData(
                table: "Messages",
                columns: new[] { "Id", "Content", "IsRead", "ReceiverId", "SenderId", "SentAt" },
                values: new object[,]
                {
                    { 1, "Emin Bey selamlar, ERP projesi için yarın Zoom üzerinden bir toplantı yapabilir miyiz?", false, 1, 4, new DateTime(2026, 5, 9, 13, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "Harika olur, yarın saat 14:00 benim için çok uygun. Linki iletebilirsiniz.", true, 4, 1, new DateTime(2026, 5, 9, 14, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, "Ayşe Hanım, renk paleti harika olmuş! Alt sayfa tasarımlarına da başlayabiliriz.", true, 2, 5, new DateTime(2026, 5, 8, 16, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, "API dokümantasyonunu inceledik, çok temiz bir iş çıkarmışsınız. Sözleşme için hukuk departmanımız size dönüş yapacak.", false, 1, 6, new DateTime(2026, 5, 9, 16, 15, 0, 0, DateTimeKind.Unspecified) },
                    { 5, "Mehmet Bey, backend API'yi ne zaman teslim edebilirsiniz?", true, 3, 4, new DateTime(2026, 5, 8, 9, 30, 0, 0, DateTimeKind.Unspecified) },
                    { 6, "Perşembe gününe kadar ilk sürümü hazırlayabilirim.", true, 4, 3, new DateTime(2026, 5, 8, 10, 15, 0, 0, DateTimeKind.Unspecified) },
                    { 7, "Zeynep Hanım, Flutter uygulama mockup'larını gördük, harika görünüyor!", false, 7, 5, new DateTime(2026, 5, 7, 14, 0, 0, 0, DateTimeKind.Unspecified) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "Messages");
        }
    }
}
