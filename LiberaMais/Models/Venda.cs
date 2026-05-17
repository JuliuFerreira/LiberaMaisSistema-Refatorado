using LiberaMais.Models.Enums;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace LiberaMais.Models
{

    [Serializable]     

    public class Venda
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [ForeignKey("Usuarios")]
        public int UsuarioId { get; set; }

        public string? UsuarioNome { get; set; }

        public UsuarioModel? Usuario { get; set; }

        [Required(ErrorMessage ="A data é obrigatória!")]
        public DateTime Data { get; set; }

        [Required(ErrorMessage ="Escolha o orgão do cliente.")]
        public int Orgao { get; set; }

        public OrgaoEnum OrgaoDescription => (OrgaoEnum)this.Orgao;

        [Required(ErrorMessage ="Selecione um dos benefícios abaixo:")]
        public int Beneficio { get; set; }

        public BeneficioEnum BeneficioDescription => (BeneficioEnum)this.Beneficio;

        [Required(ErrorMessage ="Qual a operação digitou?")]
        public int Operacao { get; set; }

        public OperacaoEnum OperacaoDescription => (OperacaoEnum)this.Operacao;

        [Required(ErrorMessage ="O CPF do cliente é obrigatório!")]
        public string Cpf { get; set; }

        [Required(ErrorMessage ="O nome do cliente é obrigatório!")]
        public string Nome { get; set; }

        [Required(ErrorMessage ="O nome do banco digitado é obrigatório!")]
        public int Banco { get; set; }

        public BancoEnum BancoDescription => (BancoEnum)this.Banco;

        [Required(ErrorMessage ="Digite o nome da promotora que fez a digitação.")]
        public int Promotora { get; set; }

        public PromotoraEnum PromotoraDescription => (PromotoraEnum)this.Promotora;

        public decimal? Parcela { get; set; }

        [Required(ErrorMessage ="Atualize aqui o status da proposta!")]
        public int Status { get; set; }

        public StatusEnum StatusDescription => (StatusEnum)this.Status;

        public DateTime? DataPagamento { get; set; }
        
        public decimal? ValorComissao { get; set; }

        public int? ComissaoStatus { get; set; }

        public ComissaoEnum? ComissaoStatusDescription => (ComissaoEnum?)this.ComissaoStatus;

        public string? Observacoes { get; set; }

        public DateTime? DataComissao { get; set; }

        public string? Observacao2 { get; set; }

    }

    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())[0]
                            .GetCustomAttribute<DisplayAttribute>()?
                            .Name ?? enumValue.ToString();
        }
    }
}
