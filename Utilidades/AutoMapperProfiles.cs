using AutoMapper;
using WebApiAutores.Dtos;
using WebApiAutores.Entidades;

namespace WebApiAutores.Utilidades
{
    public class AutoMapperProfiles:Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AutorCreacionDTO, Autor>();
            CreateMap<Autor, AutorDTO>();
            CreateMap<Autor, AutorDTOConLibros>().ForMember(x=>x.Libros,opciones=>opciones.MapFrom(MapAutorDTOLibros));
            CreateMap<LibroCreacionDTO, Libro>().ForMember(libro=>libro.AutoresLibros,opciones=>opciones.MapFrom(MapAutoresLibros));
            CreateMap<Libro, LibroDTO>();
            CreateMap<Libro, LibroDTOConAutores>().ForMember(x=>x.Autores,opciones=>opciones.MapFrom(MapLibroDTOAutores));
            CreateMap<ComentarioCreacionDTO, Comentario>();
            CreateMap<Comentario, ComentarioDTO>();
            CreateMap<LibroPatchDTO, Libro>().ReverseMap();
        }

        private List<LibroDTO> MapAutorDTOLibros(Autor arg,AutorDTO autor)
        {
            var resultado = new List<LibroDTO>();
            if (arg.AutoresLibros == null)
            {
                return resultado;
            }
            foreach (var item in arg.AutoresLibros)
            {
                resultado.Add(new LibroDTO()
                {
                    Id = item.LibroId,
                    Titulo = item.Libro.Titulo
                });
            }
            return resultado;
        }

        private List<AutorDTO> MapLibroDTOAutores(Libro arg,LibroDTO libro)
        {
            var resultado = new List<AutorDTO>();
            if (arg.AutoresLibros == null)
            {
                return resultado;
            }
            foreach (var item in arg.AutoresLibros)
            {
                resultado.Add(new AutorDTO()
                {
                    Id = item.AutorId,
                    Nombre = item.Autor.Nombre
                });
            }
            return resultado;
        }

        private List<AutorLibro> MapAutoresLibros(LibroCreacionDTO libroCreacionDTO,Libro libro)
        {
            var resultado = new List<AutorLibro>();
            if (libroCreacionDTO.AutoresIds == null)
            {
                return resultado;
            }
            foreach (var item in libroCreacionDTO.AutoresIds)
            {
                resultado.Add(new AutorLibro() { AutorId = item });
            }
            return resultado;
        }
    }
}
