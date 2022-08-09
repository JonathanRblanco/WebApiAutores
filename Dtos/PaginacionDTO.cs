namespace WebApiAutores.Dtos
{
    public class PaginacionDTO
    {
        public int Pagina { get; set; }
        private int recordsPorPagina { get; set; } = 10;
        private readonly int cantidadMaximaRecordsPorPagina = 50;
        public int RecordsPorPagina
        {
            get
            {
                return recordsPorPagina;
            }
            set
            {
                recordsPorPagina = (value > cantidadMaximaRecordsPorPagina) ? cantidadMaximaRecordsPorPagina : value;
            }
        }
    }
}
