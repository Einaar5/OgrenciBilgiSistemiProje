using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OgrenciBilgiSistemiProje.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var username = HttpContext.Session.GetString("username");
            var role = HttpContext.Session.GetString("role");

            if (string.IsNullOrEmpty(username))
            {
                context.Result = RedirectToAction("Login", "Account");
            }
            else
            {
                // Controller adını al
                var controllerName = context.Controller.GetType().Name;

                // Rol kontrolü yap
                if (controllerName == "AdminController" && role != "Admin")
                {
                    context.Result = RedirectToAction("Login", "Account");
                }
                else if (controllerName == "StudentController" && role != "Student")
                {
                    context.Result = RedirectToAction("StuTeaLog", "Account");
                }
                else if (controllerName == "TeacherController" && role != "Teacher")
                {
                    context.Result = RedirectToAction("StuTeaLog", "Account");
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
