namespace Edunext.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class LoginFilter : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;
        var role = session.GetInt32("Role");

        // Kiểm tra nếu Role không tồn tại (chưa đăng nhập)
        if (role == null)
        {
            context.Result = new RedirectToActionResult("Login", "User", null);
        }
    }
}

