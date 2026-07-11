using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Vera.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    {
        public string ErrorMessage { get; set; }

        public void OnGet(string customMessage = null, int? statusCode = null)
        {
            // 404 Sayfa Bulunamadı Hatası Geldiyse
            if (statusCode == 404)
            {
                ErrorMessage = "Yanlış bir bağlantıya tıkladınız veya bu sayfa uzayın derinliklerinde kayboldu. (Hata 404: Not Found)";
                return;
            }

            // Diğer çökmeler için eski kodumuz
            var exceptionHandlerPathFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature?.Error != null)
            {
                ErrorMessage = exceptionHandlerPathFeature.Error.Message;
            }
            else if (!string.IsNullOrEmpty(customMessage))
            {
                ErrorMessage = customMessage;
            }
            else
            {
                ErrorMessage = $"Beklenmeyen bir hata oluştu. Hata Kodu: {statusCode}";
            }
        }
    }
}