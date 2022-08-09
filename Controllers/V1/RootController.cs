using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiAutores.Dtos;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]
    [Route("api/v1")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RootController : ControllerBase
    {
        private readonly IAuthorizationService authorizationService;

        public RootController(IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
        }
        [HttpGet(Name = "obtenerRoot")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DatoHATEOAS>>> Get()
        {
            var esAdmin = await authorizationService.AuthorizeAsync(User, "esAdmin");
            var datos = new List<DatoHATEOAS>();
            datos.Add(new DatoHATEOAS(enlace: Url.Link("obtenerRoot", new { }), metodo: "GET", descripcion: "self"));
            datos.Add(new DatoHATEOAS(enlace: Url.Link("obtenerAutores", new { }), metodo: "GET", descripcion: "autores"));
            if (esAdmin.Succeeded)
            {
                datos.Add(new DatoHATEOAS(enlace: Url.Link("crearAutor", new { }), metodo: "POST", descripcion: "autor-crear"));
                datos.Add(new DatoHATEOAS(enlace: Url.Link("crearLibro", new { }), metodo: "POST", descripcion: "libro-crear"));
            }
            return datos;
        }
    }
}
