using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OgrenciBilgiSistemiProje.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context) // Eğer session'da kullanıcı adı yoksa login sayfasına yönlendirir
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("username"))) // Eğer session'da kullanıcı adı yoksa
            {
                context.Result = RedirectToAction("Login", "Account"); // Login sayfasına yönlendirir
            }
            base.OnActionExecuting(context); // Base sınıfın OnActionExecuting metodunu çalıştırır
        }
    }
}
