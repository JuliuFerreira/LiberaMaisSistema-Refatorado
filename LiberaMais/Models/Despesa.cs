using LiberaMais.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

//namespace LiberaMais.Models
//{
//    [Serializable]
//    public class Despesa
//    {
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public int Id { get; set; }

//        [ForeignKey("Financa")]
//        public int FinancaId { get; set; }

//        [Required(ErrorMessage = "Digite a data da despesa!")]
//        public DateTime DataDespesa { get; set; }

//        [Required(ErrorMessage = "Digite a descrição da receita!")]
//        public string Descricao { get; set; }

//        [Required(ErrorMessage = "Selecione o usuário!")]
//        public int Usuario { get; set; }
//        public UsuarioEnum UsuarioDescription => (UsuarioEnum)this.Usuario;

//        [Required(ErrorMessage = "Digite o valor recebido!")]
//        public decimal ValorDespesa { get; set; }

//        public virtual Financa? Financa { get; set; }

//    }
//}
