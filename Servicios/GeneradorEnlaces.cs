using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using WebApiAutores.Dtos;

namespace WebApiAutores.Servicios
{
    public class GeneradorEnlaces
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IActionContextAccessor actionContextAccessor;

        public GeneradorEnlaces(IAuthorizationService authorizationService,IHttpContextAccessor httpContextAccessor,IActionContextAccessor actionContextAccessor)
        {
            this.authorizationService = authorizationService;
            this.httpContextAccessor = httpContextAccessor;
            this.actionContextAccessor = actionContextAccessor;
        }
        private IUrlHelper ConstruirURLHelper()
        {
            var factoria = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
            return factoria.GetUrlHelper(actionContextAccessor.ActionContext);
        }
        private async Task<bool> EsAdmin()
        {
            var httpContext = httpContextAccessor.HttpContext;
            var resultado = await authorizationService.AuthorizeAsync(httpContext.User, "esAdmin");
            return resultado.Succeeded;
        }
        public async Task GenerarEnlaces(AutorDTO autorDTO)
        {
            var esAdmin = await EsAdmin();
            var Url = ConstruirURLHelper();
            autorDTO.Enlaces.Add(new DatoHATEOAS(Url.Link("obtenerAutor", new { id = autorDTO.Id }), "GET", "self"));
            if (esAdmin)
            {
                autorDTO.Enlaces.Add(new DatoHATEOAS(Url.Link("actualizarAutor", new { id = autorDTO.Id }), "PUT", "autor-actualizar"));
                autorDTO.Enlaces.Add(new DatoHATEOAS(Url.Link("eliminarAutor", new { id = autorDTO.Id }), "DELETE", "autor-eliminar"));
            }
        }
    }
}
