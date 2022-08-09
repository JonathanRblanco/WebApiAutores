namespace WebApiAutores.Dtos
{
    public class ColeccionDeRecursos<T> : Recurso where T : Recurso
    {
        public List<T> Valores { get; set; } = new List<T>();
    }
}
