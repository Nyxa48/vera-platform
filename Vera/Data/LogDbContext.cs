using Microsoft.EntityFrameworkCore;
using Vera.Models;
using System;

namespace Vera.Data
{
    public class LogDbContext : DbContext
    {
        public LogDbContext(DbContextOptions<LogDbContext> options) : base(options) { }

        public DbSet<ActivityLog> Logs { get; set; }
        public DbSet<Message> Messages { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // BİLDİRİMLER
            modelBuilder.Entity<ActivityLog>().HasData(
                new ActivityLog { Id = 1, Action = "Yeni Teklif Kabul Edildi", Details = "Kurumsal ERP Sistemi projesi için teklifiniz Zirve Teknoloji tarafından onaylandı.", Timestamp = new DateTime(2026, 5, 9, 14, 0, 0), UserId = 1 },
                new ActivityLog { Id = 2, Action = "Hesaba Geçti", Details = "₺12.500 tutarındaki hakedişiniz bakiyenize başarıyla eklendi.", Timestamp = new DateTime(2026, 5, 8, 10, 0, 0), UserId = 1 },
                new ActivityLog { Id = 3, Action = "Sistem Güncellemesi", Details = "Vera v2.0 yayında! Yeni özellikleri hemen keşfedin.", Timestamp = new DateTime(2026, 5, 6, 9, 0, 0), UserId = 1 },
                new ActivityLog { Id = 4, Action = "Yeni Proje Eşleşmesi", Details = "Yeteneklerinize uygun 'Mobil Uygulama Tasarımı' projesi yayınlandı.", Timestamp = new DateTime(2026, 5, 9, 11, 0, 0), UserId = 2 },
                new ActivityLog { Id = 5, Action = "Yeni İş İlanı Yayınlandı", Details = "React Native Kurye Uygulaması projesi yayına alındı.", Timestamp = new DateTime(2026, 5, 4, 16, 0, 0), UserId = 6 },
                new ActivityLog { Id = 6, Action = "Ödeme Yapıldı", Details = "Figma UI/UX Revizesi projesi için freelancer'a ₺5.000 ödeme yapıldı.", Timestamp = new DateTime(2026, 5, 7, 12, 30, 0), UserId = 5 }
            );

            // MESAJLAŞMALAR
            modelBuilder.Entity<Message>().HasData(
                new Message { Id = 1, SenderId = 4, ReceiverId = 1, Content = "Emin Bey selamlar, ERP projesi için yarın Zoom üzerinden bir toplantı yapabilir miyiz?", SentAt = new DateTime(2026, 5, 9, 13, 0, 0), IsRead = false },
                new Message { Id = 2, SenderId = 1, ReceiverId = 4, Content = "Harika olur, yarın saat 14:00 benim için çok uygun. Linki iletebilirsiniz.", SentAt = new DateTime(2026, 5, 9, 14, 0, 0), IsRead = true },
                new Message { Id = 3, SenderId = 5, ReceiverId = 2, Content = "Ayşe Hanım, renk paleti harika olmuş! Alt sayfa tasarımlarına da başlayabiliriz.", SentAt = new DateTime(2026, 5, 8, 16, 0, 0), IsRead = true },
                new Message { Id = 4, SenderId = 6, ReceiverId = 1, Content = "API dokümantasyonunu inceledik, çok temiz bir iş çıkarmışsınız. Sözleşme için hukuk departmanımız size dönüş yapacak.", SentAt = new DateTime(2026, 5, 9, 16, 15, 0), IsRead = false },
                new Message { Id = 5, SenderId = 4, ReceiverId = 3, Content = "Mehmet Bey, backend API'yi ne zaman teslim edebilirsiniz?", SentAt = new DateTime(2026, 5, 8, 9, 30, 0), IsRead = true },
                new Message { Id = 6, SenderId = 3, ReceiverId = 4, Content = "Perşembe gününe kadar ilk sürümü hazırlayabilirim.", SentAt = new DateTime(2026, 5, 8, 10, 15, 0), IsRead = true },
                new Message { Id = 7, SenderId = 5, ReceiverId = 7, Content = "Zeynep Hanım, Flutter uygulama mockup'larını gördük, harika görünüyor!", SentAt = new DateTime(2026, 5, 7, 14, 0, 0), IsRead = false }
            );
        }
    }
}