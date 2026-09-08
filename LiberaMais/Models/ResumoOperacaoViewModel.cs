namespace LiberaMais.Models
{
    public class ResumoOperacaoViewModel
    {
        public string NomeOperacao { get; set; }

        public int Quantidade { get; set; }

        public decimal TotalValorContrato { get; set; }

        public decimal TotalSaldoDevedor { get; set; }

        public List<ResumoStatusViewModel> Status { get; set; }
            = new List<ResumoStatusViewModel>();
    }
}