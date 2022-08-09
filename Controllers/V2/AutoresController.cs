using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Dtos;
using WebApiAutores.Entidades;
using WebApiAutores.Filtros;

namespace WebApiAutores.Controllers.V2
{
    [ApiController]
    [Route("api/v2/autores")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IAuthorizationService authorizationService;

        public IMapper Mapper { get; }

        public AutoresController(ApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService)
        {
            this.context = context;
            Mapper = mapper;
            this.authorizationService = authorizationService;
        }
        [HttpGet(Name = "obtenerAutoresV2")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        public async Task<ActionResult<List<AutorDTO>>> Get()
        {
            var autores = await context.Autores.ToListAsync();
            autores.ForEach(x => x.Nombre = x.Nombre.ToUpper());
            return Mapper.Map<List<AutorDTO>>(autores);
        }
        [HttpGet("{id:int}", Name = "obtenerAutorV2")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        public async Task<ActionResult<AutorDTOConLibros>> Get(int id)
        {
            var autor = await context.Autores.Include(x => x.AutoresLibros).ThenInclude(x => x.Libro).ThenInclude(x => x.Comentarios).FirstOrDefaultAsync(a => a.Id == id);
            if (autor == null)
            {
                return NotFound();
            }
            var dto = Mapper.Map<AutorDTOConLibros>(autor);

            return dto;
        }

        [HttpGet("{nombre}", Name = "obtenerAutorPorNombreV2")]
        public async Task<ActionResult<List<AutorDTO>>> GetPorNombre([FromRoute] string nombre)
        {
            var autores = await context.Autores.Where(x => x.Nombre.Contains(nombre)).ToListAsync();

            return Mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpPost(Name = "crearAutorV2")]
        public async Task<ActionResult> Post([FromBody] AutorCreacionDTO autor)
        {
            var existeAutor = await context.Autores.AnyAsync(x => x.Nombre == autor.Nombre);
            if (existeAutor)
            {
                return BadRequest($"Ya existe un autor con el nombre {autor.Nombre}");
            }
            var autorNuevo = Mapper.Map<Autor>(autor);
            context.Add(autorNuevo);
            await context.SaveChangesAsync();
            var autorDto = Mapper.Map<AutorDTO>(autorNuevo);
            return CreatedAtRoute("obtenerAutorV2", new { id = autorNuevo.Id }, autorDto);
        }
        [HttpPut("{id:int}", Name = "actualizarAutorV2")]//api/autores/x
        public async Task<ActionResult> Put(AutorCreacionDTO autorCreacion, int id)
        {
            var existe = await context.Autores.AnyAsync(a => a.Id == id);
            if (!existe)
            {
                return NotFound();
            }
            var autor = Mapper.Map<Autor>(autorCreacion);
            autor.Id = id;
            context.Update(autor);
            await context.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("{id:int}", Name = "eliminarAutorV2")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Autores.AnyAsync(a => a.Id == id);
            if (!existe)
            {
                return NotFound();
            }
            context.Remove(new Autor() { Id = id });
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
