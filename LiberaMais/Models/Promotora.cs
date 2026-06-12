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

        [Required(ErrorMessage ="O nome é obrigatório")]
        public string Nome { get; set; }

        [RegularExpression(@"^(https?:\/\/)?(www\.)[a-zA-Z0-9-]+(\.[a-zA-Z]{2,})+(\.[a-zA-Z]{2,})?$",
        ErrorMessage = "Por favor, insira uma URL válida (ex: ://site.com ou ://site.com.br)")]
        public string? Url { get; set; }

        [Required(ErrorMessage = "O login é obrigatório")]

        public string Login { get; set; }

        [Required(ErrorMessage = "A senha é obrigatório")]

        public string Senha { get; set; }

        public List<PromotoraBanco>? PromotoraBancos { get; set; }

        [Required(ErrorMessage = "Selecione um usuário.")]
        public int? UsuarioId { get; set; }

        public UsuarioModel? Usuario { get; set; }




    }
}
