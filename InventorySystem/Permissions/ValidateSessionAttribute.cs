using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace InventorySystem.Permissions
{
    public class ValidateSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Obtener la sesión actual del contexto
            var session = context.HttpContext.Session;
            // Obtener la ruta actual
            var route = context.HttpContext.Request.Path.Value?.ToLower();
            // Excluir rutas específicas del filtro
            if (route == "/" || route == "/login/index" || route == "/login/register" || route == "/login/validatelogin")
            {
                base.OnActionExecuting(context);
                return;
            }
            // Verificar si hay datos en la sesión
            var userId = session.GetInt32("IdUser");
            Console.WriteLine("IdUser en sesión durante validación: " + userId);
            // Verificar si la sesión contiene el identificador del usuario
            if (userId == null)
            {
                // Redirigir a la página de inicio de sesión si no hay sesión activa
                context.Result = new RedirectToActionResult("Index", "Login", new { alert = "true" });
            }
            base.OnActionExecuting(context);
        }
    }
}