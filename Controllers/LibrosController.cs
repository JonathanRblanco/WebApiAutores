using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Dtos;
using WebApiAutores.Entidades;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiAutores.Controllers
{
    [Route("api/libros")]
    [ApiController]
    public class LibrosController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public IMapper Mapper { get; }

        public LibrosController(ApplicationDbContext context,IMapper mapper)
        {
            this.context = context;
            Mapper = mapper;
        }
        // GET: api/<LibrosController>
        [HttpGet("{id:int}",Name ="obtenerLibro")]
        public async Task<ActionResult<LibroDTOConAutores>> Get(int id)
        {
            var libro = await context.Libros.Include(x=>x.AutoresLibros).ThenInclude(x=>x.Autor).FirstOrDefaultAsync(x => x.Id == id);
            if (libro == null)
            {
                return NotFound();
            }
            libro.AutoresLibros = libro.AutoresLibros.OrderBy(x => x.Orden).ToList();
            return Mapper.Map<LibroDTOConAutores>(libro);
        }
        [HttpGet]
        public async Task<List<LibroDTO>> Get()
        {
            return Mapper.Map<List<LibroDTO>>(await context.Libros.Include(x => x.AutoresLibros).ThenInclude(x => x.Autor).Include(x=>x.Comentarios).ToListAsync());
        }

        
        [HttpPost]
        public async Task<ActionResult> Post(LibroCreacionDTO libro)
        {
            if (libro.AutoresIds == null)
            {
                return BadRequest("No se puede crear un libro sin autores");
            }
            var autoresIds = await context.Autores.Where(x => libro.AutoresIds.Contains(x.Id)).Select(x=>x.Id).ToListAsync();
            if (libro.AutoresIds.Count != autoresIds.Count)
            {
                return BadRequest("Algunos autores no existen");
            }
            var nuevoLibro = Mapper.Map<Libro>(libro);
            AsignarOrdenAutores(nuevoLibro);
            context.Add(nuevoLibro);
            await context.SaveChangesAsync();
            var libroDTO=Mapper.Map<LibroDTO>(libro);
            return CreatedAtRoute("obtenerLibro", new { id = nuevoLibro.Id,},libroDTO);
        }

        // PUT api/<LibrosController>/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, LibroCreacionDTO libroCreacion)
        {
            var libroDB = await context.Libros.Include(x=>x.AutoresLibros).FirstOrDefaultAsync(x => x.Id == id);
            if (libroDB == null)
            {
                return NotFound();
            }
            libroDB = Mapper.Map(libroCreacion, libroDB);
            AsignarOrdenAutores(libroDB);
            await context.SaveChangesAsync();
            return NoContent();
        }
        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch(int id,JsonPatchDocument<LibroPatchDTO> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }
            var libroDB = await context.Libros.FirstOrDefaultAsync(x => x.Id == id);
            if (libroDB == null)
            {
                return NotFound();
            }
            var libroPatch = Mapper.Map<LibroPatchDTO>(libroDB);
            patchDocument.ApplyTo(libroPatch, ModelState);
            Mapper.Map(libroPatch, libroDB);
            var esValido = TryValidateModel(libroPatch);
            if (!esValido)
            {
                return BadRequest(ModelState);
            }
            
            await context.SaveChangesAsync();
            return NoContent();

        }
        // DELETE api/<LibrosController>/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Libros.AnyAsync(a => a.Id == id);
            if (!existe)
            {
                return NotFound();
            }
            context.Remove(new Libro() { Id = id });
            await context.SaveChangesAsync();
            return NoContent();
        }
        private void AsignarOrdenAutores(Libro libro)
        {
            if (libro.AutoresLibros != null)
            {
                for (int i = 0; i < libro.AutoresLibros.Count; i++)
                {
                    libro.AutoresLibros[i].Orden = i;
                }
            }
        }
    }
}
