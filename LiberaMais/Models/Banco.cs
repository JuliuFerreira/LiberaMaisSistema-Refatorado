using LiberaMais.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace LiberaMais.Models
{

    [Serializable]
    public class Banco
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]        
        public int Id { get; set; }

        [Required(ErrorMessage ="O nome é obrigatório")]
        public string Nome { get; set; }

        [RegularExpression(@"^(https?:\/\/)?(www\.)[a-zA-Z0-9-]+(\.[a-zA-Z]{2,})+(\.[a-zA-Z]{2,})?$",
        ErrorMessage = "Por favor, insira uma URL válida (ex: ://site.com ou ://site.com.br)")]
        public string? Url { get; set; }
                
        public List<PromotoraBanco>? PromotoraBancos { get; set; }

    }
}
