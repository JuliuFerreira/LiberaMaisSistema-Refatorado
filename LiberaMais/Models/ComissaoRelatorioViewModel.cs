using LiberaMais.Models.Enums;

namespace LiberaMais.Models
{
    public class ComissaoRelatorioViewModel
    {
        public int VendaId { get; set; }

        public string ClienteNome { get; set; }

        public string ClienteCpf { get; set; }

        public string PromotoraNome { get; set; }

        public string BancoNome { get; set; }

        public string UsuarioNome { get; set; }

        public decimal? ValorComissao { get; set; }

        public DateTime? DataPgtoContrato { get; set; }

        public DateTime? DataPgtoComissao { get; set; }

        public StatusEnum? StatusContrato { get; set; }

        public Venda? Venda { get; set; }

        public bool IsComissaoPaga => DataPgtoComissao.HasValue;

        public string StatusFinanceiro
        {
            get
            {
                // Se por algum motivo a propriedade Venda for nula, evita que o sistema quebre
                if (Venda == null) return "Dados Incompletos";

                if (Venda.DataPgtoComissao.HasValue) return "Paga";

                if (!Venda.DataPgtoContrato.HasValue) return "Aguardando Contrato";

                var dataLimite = Venda.DataPgtoContrato.Value.AddDays(5);
                if (DateTime.Today > dataLimite)
                {
                    return "Em Atraso";
                }

                return "Aguardando Recebimento";
            }
        }
        public int DiasAtrasoOuPrazo
        {
            get
            {
                if (IsComissaoPaga || !DataPgtoContrato.HasValue) return 0;

                var dataLimite = DataPgtoContrato.Value.AddDays(5);
                return (DateTime.Today - dataLimite).Days;
            }
        }
    }
}
