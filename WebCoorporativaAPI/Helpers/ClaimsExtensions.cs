using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace WebCoorporativaAPI.Helpers
{
    public static class ClaimsExtensions
    {
        public static bool TienePermiso(this ClaimsPrincipal user, string permiso)
        {
            if (user == null) return false;

            // Si es admin tiene todos los permisos
            //if (user.Claims.Any(c => c.Type == "esAdmin" && c.Value == "true"))
            //    return true;
            if (user.Claims.Any(c => c.Type == "esAdmin" && c.Value.Equals("true", StringComparison.OrdinalIgnoreCase)))
                return true;

            return user.Claims
                .Where(c => c.Type == "permiso")
                .Any(c => c.Value == permiso);
        }

        public static int? GetPerfilId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst("perfilId");

            if (claim == null)
                return null;

            return int.TryParse(claim.Value, out int perfilId)
                ? perfilId
                : null;

        }
    }
}
