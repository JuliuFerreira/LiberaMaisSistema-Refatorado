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

        public string Nome { get; set; }

        public string? Url { get; set; }
                
        public List<PromotoraBanco>? PromotoraBancos { get; set; }

    }
}
