# Vera Platformu 

Vera Platformu, B2B işletmeler ve serbest çalışanlar (freelancer) arasındaki iş süreçlerini, sözleşmeleri ve proje takibini daha düzenli ve şeffaf hale getirmek için geliştirdiğimiz bir proje yönetim ekosistemidir.

## Kullanılan Teknolojiler

Bu projeyi geliştirirken güncel ve performanslı bir altyapı kurmaya özen gösterdik:

* .NET 10.0
* Entity Framework Core
* SQLite (Veritabanı)

## Projeyi Bilgisayarınızda Çalıştırma

Projeyi yerel ortamınızda ayağa kaldırmak oldukça basittir. Veritabanı olarak SQLite kullandığımız için herhangi bir dış veritabanı sunucusu veya Docker kurulumu yapmanıza gerek yoktur. Veritabanı dosyası, ilk kurulum adımında projenin içine otomatik olarak oluşturulacaktır.

Aşağıdaki adımları sırayla terminalinizde çalıştırarak projeyi başlatabilirsiniz:

1. Gerekli bağımlılıkları ve paketleri indirin:
```bash
dotnet restore
Veritabanını oluşturmak ve tabloları hazır hale getirmek için migration işlemlerini uygulayın:

Bash
dotnet ef database update
Uygulamayı çalıştırın:

Bash
dotnet run
Eğer veritabanı tablolarında veya migration süreçlerinde bir sorun yaşarsanız, projenin içindeki Migrations klasörünü kontrol edebilir veya mevcut veritabanı dosyasını silip database update komutunu yeniden çalıştırabilirsiniz.