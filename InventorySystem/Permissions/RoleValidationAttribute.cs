using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace InventorySystem.Permissions
{
    public class RoleValidationAttribute: ActionFilterAttribute
    {
        private readonly int[] _allowedRoles;

        public RoleValidationAttribute(params int[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;

            // Obtener el rol del usuario desde la sesión
            var userRole = session.GetInt32("IdRol");

            if (userRole == null || !_allowedRoles.Contains(userRole.Value))
            {
                // Redirigir al usuario si no tiene acceso
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
            }

            base.OnActionExecuting(context);
        }
    }
}
