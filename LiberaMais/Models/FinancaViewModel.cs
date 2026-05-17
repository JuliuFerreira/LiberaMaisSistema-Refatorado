namespace LiberaMais.Models
{
    public class FinancaViewModel
    {
        public IEnumerable<Financa> Financas { get; set; }
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }
        public decimal SaldoTotal { get; set; }
    }
}
