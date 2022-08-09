namespace WebApiAutores.Dtos
{
    public class DatoHATEOAS
    {
        public DatoHATEOAS(string enlace,string metodo,string descripcion)
        {
            Enlace = enlace;
            Metodo = metodo;
            Descripcion = descripcion;
        }
        public string Enlace { get; set; }
        public string Metodo { get; set; }
        public string Descripcion { get; set; }
    }
}
