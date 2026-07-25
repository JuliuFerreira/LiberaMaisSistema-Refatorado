//using LiberaMais.Models.Enums;
//using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace LiberaMais.Models
//{
//    [Serializable]
//    public class Receita
//    {
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public int Id { get; set; }

//        [ForeignKey("Financa")]
//        public int FinancaId { get; set; }

//        [ValidateNever]
//        public virtual Financa? Financa { get; set; }


//        [Required(ErrorMessage = "Digite a data da Receita!")]
//        public DateTime DataReceita { get; set; }


//        [Required(ErrorMessage ="Digite a descrição da receita!")]
//        public string Descricao { get; set; }


//        [Required(ErrorMessage = "Selecione o usuário!")]
//        public int Usuario { get; set; }
        
//        public UsuarioEnum UsuarioDescription => (UsuarioEnum)this.Usuario;

//        [Required(ErrorMessage ="Digite o valor recebido!")]
//        public decimal ValorRecebido { get; set; }


//        public int PromotoraId { get; set; }

//        [ValidateNever]
//        public virtual Promotora? Promotora { get; set; }
//    }
//}
