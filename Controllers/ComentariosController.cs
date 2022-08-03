using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Dtos;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/libros/{libroId:int}/comentarios")]
    public class ComentariosController:ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper Mapper;
        private readonly UserManager<IdentityUser> userManager;

        public ComentariosController(ApplicationDbContext context,IMapper mapper,UserManager<IdentityUser> userManager)
        {
            this.context = context;
            Mapper = mapper;
            this.userManager = userManager;
        }
        [HttpGet("{id:int}",Name ="obtenerComentario")]
        public async Task<ActionResult<ComentarioDTO>> GetPorId(int id)
        {
            var comentario = await context.Comentarios.FirstOrDefaultAsync(x => x.Id == id);
            if (comentario == null)
            {
                return NotFound();
            }
            return Mapper.Map<ComentarioDTO>(comentario);
        }
        [HttpGet]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int libroId)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!existeLibro)
            {
                return NotFound();
            }
            var comentarios = await context.Comentarios.Where(x => x.LibroId == libroId).ToListAsync();
            return Mapper.Map<List<ComentarioDTO>>(comentarios);
        }
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post(int libroId,ComentarioCreacionDTO comentario)
        {
            var emailClaim = HttpContext.User.Claims.Where(c=>c.Type=="email").FirstOrDefault();
            var email = emailClaim.Value;
            var usuario = await userManager.FindByEmailAsync(email);
            var usuarioId = usuario.Id;
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!existeLibro)
            {
                return NotFound();
            }
            var comentarioNuevo = Mapper.Map<Comentario>(comentario);
            comentarioNuevo.LibroId = libroId;
            comentarioNuevo.UsuarioId = usuarioId;
            context.Add(comentarioNuevo);
            await context.SaveChangesAsync();
            var comentarioDto = Mapper.Map<ComentarioDTO>(comentarioNuevo);
            return CreatedAtRoute("obtenerComentario", new { id = comentarioNuevo.Id,libroId=libroId }, comentarioDto);
        }
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int libroId,int id,ComentarioCreacionDTO comentarioCreacion)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!existeLibro)
            {
                return NotFound();
            }
            var existeComentario = await context.Comentarios.AnyAsync(x => x.Id == id);
            if (!existeComentario)
            {
                return NotFound();
            }
            var comentario = Mapper.Map<Comentario>(comentarioCreacion);
            comentario.Id = id;
            comentario.LibroId = libroId;
            context.Update(comentario);
            await context.SaveChangesAsync();
            return NoContent();
        }

    }
}
