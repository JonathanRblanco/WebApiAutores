using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Dtos;
using WebApiAutores.Entidades;
using WebApiAutores.Filtros;
using WebApiAutores.Utilidades;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]
    [Route("api/v1/autores")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public IMapper Mapper { get; }

        public AutoresController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            Mapper = mapper;
        }
        [HttpGet(Name = "obtenerAutores")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        public async Task<ActionResult<List<AutorDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryable = context.Autores.AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);
            var autores = await queryable.OrderBy(x=>x.Nombre).Paginar(paginacionDTO).ToListAsync();
            return Mapper.Map<List<AutorDTO>>(autores);
        }
        [HttpGet("{id:int}", Name = "obtenerAutor")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        [ProducesResponseType(404)]
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

        [HttpGet("{nombre}", Name = "obtenerAutorPorNombre")]
        public async Task<ActionResult<List<AutorDTO>>> GetPorNombre([FromRoute] string nombre)
        {
            var autores = await context.Autores.Where(x => x.Nombre.Contains(nombre)).ToListAsync();

            return Mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpPost(Name = "crearAutor")]
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
            return CreatedAtRoute("obtenerAutor", new { id = autorNuevo.Id }, autorDto);
        }
        [HttpPut("{id:int}", Name = "actualizarAutor")]//api/autores/x
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
        /// <summary>
        /// Borra un autor
        /// </summary>
        /// <param name="id">Id del autor a borrar</param>
        /// <returns></returns>
        [HttpDelete("{id:int}", Name = "eliminarAutor")]
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
