using LiberaMais.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LiberaMais.Models
{
    public class Financa
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Selecione o mês.")]
        public int Mes {  get; set; }

        public MesEnum Mesdescription => (MesEnum)this.Mes;

        [Required(ErrorMessage = "Selecione o ano.")]
        public int Ano { get; set; }

        //public AnoEnum Anodescription => (AnoEnum)this.Ano;

        public List<Receita>? Receitas { get; set; }
        public List<Despesa>? Despesas { get; set; }

        [NotMapped]
        public decimal TotalReceitas { get; set; }

        [NotMapped]
        public decimal TotalDespesas { get; set; }

        [NotMapped]
        public decimal SaldoTotal => TotalReceitas - TotalDespesas;

    }
}
