using LiberaMais.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LiberaMais.Models
{
    public class TaxaCoeficiente
    {
        public int Id { get; set; }

        [Required]
        public string Taxa { get; set; }

        [Required]
        [Display(Name = "Coeficiente")]
        [Column(TypeName = "decimal(18, 8)")]
        public decimal Coeficiente { get; set; }

        [Required]
        [Display(Name = "Operação")]
        public OperacaoEnum Operacao { get; set; }


        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;
    }
}