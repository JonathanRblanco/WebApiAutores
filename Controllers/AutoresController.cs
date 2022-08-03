using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Dtos;
using WebApiAutores.Entidades;
using WebApiAutores.Filtros;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/autores")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,Policy = "EsAdmin")]
    public class AutoresController:ControllerBase
    {
        private readonly ApplicationDbContext context;

        public IMapper Mapper { get; }

        public AutoresController(ApplicationDbContext context,IMapper mapper)
        {
            this.context = context;
            Mapper = mapper;
        }
        [HttpGet("{id:int}",Name ="obtenerAutor")]
        public async Task<ActionResult<AutorDTOConLibros>> Get(int id)
        {
            var autor = await context.Autores.Include(x=>x.AutoresLibros).ThenInclude(x=>x.Libro).ThenInclude(x=>x.Comentarios).FirstOrDefaultAsync(a => a.Id == id);
            if (autor == null)
            {
                return NotFound();
            }
            return Mapper.Map<AutorDTOConLibros>(autor);
        }
        [HttpGet("{nombre}")]
        public async Task<ActionResult<List<AutorDTO>>> Get([FromRoute] string nombre)
        {
            var autores = await context.Autores.Where(x => x.Nombre.Contains(nombre)).ToListAsync();

            return Mapper.Map<List<AutorDTO>>(autores);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<AutorDTO>>> Get()
        {
            var autores= await context.Autores.ToListAsync();
            return Mapper.Map<List<AutorDTO>>(autores);
        }
        [HttpPost]
        public async Task<ActionResult> Post([FromBody]AutorCreacionDTO autor)
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
            return CreatedAtRoute("obtenerAutor", new {id=autorNuevo.Id},autorDto);
        }
        [HttpPut("{id:int}")]//api/autores/x
        public async Task<ActionResult> Put(AutorCreacionDTO autorCreacion,int id)
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
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Autores.AnyAsync(a => a.Id == id);
            if (!existe)
            {
                return NotFound();
            }
            context.Remove(new Autor() { Id=id});
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
