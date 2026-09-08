namespace LiberaMais.Models
{
    public class RegraBanco
    {
        public int Id { get; set; }

        public int PromotoraBancoId { get; set; }

        public string? RegrasValores { get; set; }

        public string? RegraIdade { get; set; }

        public string? BancoNaoPortado { get; set; }

        public string? BancoComRegra { get; set; }

        public string? BancoRegraGeral { get; set; }

        public DateTime DataAtualizacao { get; set; }

        public PromotoraBanco? PromotoraBanco { get; set; }
    }
}
