using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LiberaMais.Models.Enums;
using System.Diagnostics.CodeAnalysis;

namespace LiberaMais.Models
{

    [Serializable]
    public class Promotora
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Nome { get; set; }

        public string? Url { get; set; }

        public string Login { get; set; }
        
        public string Senha { get; set; }

        public List<PromotoraBanco>? PromotoraBancos { get; set; }


    }
}
