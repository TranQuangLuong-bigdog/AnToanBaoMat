using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AnToanBaoMat.Services
{
    public class RoleAuthorize : ActionFilterAttribute
    {
        private readonly string _role;

        public RoleAuthorize(string role)
        {
            _role = role;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role))
            {
                context.Result = new RedirectToActionResult(
                    "Login",
                    "Account",
                    null);

                return;
            }

            if (role != _role)
            {
                context.Result = new ContentResult
                {
                    Content = "Bạn không có quyền truy cập."
                };

                return;
            }

            base.OnActionExecuting(context);
        }
    }
}