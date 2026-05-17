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
        public string Login { get; set; }

        public string Senha { get; set; }

        public int PromotoraId { get; set; }

        public Promotora? Promotora { get; set; }

        public int BancoId { get; set; }

        public Banco? Banco { get; set; }

    }
}
