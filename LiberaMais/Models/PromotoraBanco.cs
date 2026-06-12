using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LiberaMais.Models
{
    [Serializable]
    public class PromotoraBanco
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage ="O usuário é obrigatório")]
        public string Login { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        public string Senha { get; set; }

        public int PromotoraId { get; set; }

        public Promotora? Promotora { get; set; }

        [Required(ErrorMessage = "Selecione o banco")]
        public int? BancoId { get; set; }

        public Banco? Banco { get; set; }

    }
}
