using System.ComponentModel.DataAnnotations;
using LiberaMais.Models.Enums;

namespace LiberaMais.Models
{
    [Serializable]
    public class Venda
    {
        public int Id { get; set; }
        public DateTime? DataCadastro { get; set; } = DateTime.Now;
        public int UsuarioId { get; set; }
        public UsuarioModel? Usuario { get; set; } 
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }
        public int ClienteBeneficioId { get; set; }
        public ClienteBeneficio? ClienteBeneficio { get; set; }
        public string? Observacao { get; set; }
        public int PromotoraId { get; set; }
        public Promotora? Promotora { get; set; }
        public int BancoId { get; set; }
        public Banco? Banco { get; set; }
        public OperacaoEnum Operacao { get; set; } 
        public StatusEnum StatusContrato { get; set; } 

        //Campos contrato normal
        public decimal? ValorParcela { get; set; }
        public decimal? ValorContrato { get; set; }
        public int? NumeroDeParcelas { get; set; }

        //Campos portabilidade
        public string? BancoComprado { get; set; }
        public string? NumeroContrato { get; set; }
        public decimal? ValorParcelaPort { get; set; }
        public decimal? SaldoDevedor { get; set; }
        public int? ParcelasRestantes { get; set; }

        public decimal? ValorComissao { get; set; }

        public DateTime? DataPgtoComissao { get; set; }
        public DateTime? DataPgtoContrato { get; set; }

        public decimal? ValorRepasse { get; set; }

        public DateTime? DataRepasse { get; set; }

        public StatusRepasseComissaoEnum? RepasseComissao { get; set; }

        public string? ObservacaoRepasse { get; set; }

    }
}