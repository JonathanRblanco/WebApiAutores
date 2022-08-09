using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApiAutores.Dtos;
using WebApiAutores.Servicios;

namespace WebApiAutores.Filtros
{
    public class HATEOASAutorFilterAttribute : HATEOASFiltroAttribute
    {
        private readonly GeneradorEnlaces generadorEnlaces;

        public HATEOASAutorFilterAttribute(GeneradorEnlaces generadorEnlaces)
        {
            this.generadorEnlaces = generadorEnlaces;
        }
        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var debeIncluir = DebeIncluirHATEOAS(context);
            if (!debeIncluir)
            {
                await next();
                return;
            }
            var resultado = context.Result as ObjectResult;
            var autorDto = resultado.Value as AutorDTO;
            if (autorDto == null)
            {
                var autores = resultado.Value as List<AutorDTO> ?? throw new ArgumentException("Se esperaba una instancia de AutorDTO o List<AutorDTO>");
                autores.ForEach(async a => await generadorEnlaces.GenerarEnlaces(a));
                resultado.Value = autores;
            }
            else
            {
                await generadorEnlaces.GenerarEnlaces(autorDto);
            }
            await next();
        }
    }
}
