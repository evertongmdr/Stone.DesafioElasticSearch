namespace Stone.Common.Core.DTOs.Support
{
    public class PagedResult<T> where T : class
    {
        public IReadOnlyList<T> Items { get; set; }
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public int TamanhoPagina { get; set; }
        public int QuantidadeTotal { get; set; }
        public bool TemAnterior => PaginaAtual > 1;
        public bool TemProxima => PaginaAtual < TotalPaginas;

        public PagedResult() { }

        public PagedResult(List<T> items, int quantidadeTotal, int paginaAtual, int tamanhoPagina)
        {


            QuantidadeTotal = quantidadeTotal;
            PaginaAtual = paginaAtual;
            TamanhoPagina = tamanhoPagina;
            TotalPaginas = (int)Math.Ceiling(quantidadeTotal / (double)tamanhoPagina);
            Items = items;
        }
    }
}
