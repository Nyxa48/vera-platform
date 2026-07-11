using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vera.Models;
using System;

namespace Vera.Data
{
    /// <summary>
    /// [VERİTABANI YÖNETİMİ] 
    /// Entity Framework Core kullanılarak oluşturulmuş ana veritabanı bağlamı (Context).
    /// Sistemdeki tüm temel CRUD (Create, Read, Update, Delete) işlemleri bu sınıf üzerinden yürütülür.
    /// </summary>
    public class CoreDbContext : DbContext
    {
        public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options) { }

        // Veritabanı Tablolarımız (DbSet ile EF Core'a bildirilir)
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectStage> ProjectStages { get; set; }
        public DbSet<WorkLog> WorkLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // EF Core'un her build sırasında yeni migration beklemesi veya 
            // dinamik verilere uyarı atmasını engelliyoruz. Konsol temiz kalıyor.
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // [MİMARİ] - Primary Key (Birincil Anahtar) tanımlamaları
            // Her tablonun benzersiz bir kimliği (Id) olması veritabanı düzenini sağlar.
            modelBuilder.Entity<WorkLog>().HasKey(w => w.Id);
            modelBuilder.Entity<Project>().HasKey(p => p.Id);

            // [VERİTABANI YÖNETİMİ] - Tablolar Arası İlişkiler (Foreign Key)
            // Bir proje silindiğinde (Cascade), o projeye ait tüm logların da silinmesini sağlıyoruz.
            // Bu sayede "Gereksiz (Orphan) Veri" oluşumunu önlüyor, DB bütünlüğünü koruyoruz.
            modelBuilder.Entity<WorkLog>()
                .HasOne<Project>()
                .WithMany()
                .HasForeignKey(w => w.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // NOT: Önceki sürümlerdeki hardcoded "HasData" verileri güvenlik prensiplerimiz gereği kaldırılmıştır.
            // Bunun yerine tüm tohumlama (seeding) işlemleri kriptografik şifreleme kullanılarak
            // güvenli bir şekilde 'DataSeeder.cs' içerisinde dinamik olarak yapılmaktadır.
        }
    }
}