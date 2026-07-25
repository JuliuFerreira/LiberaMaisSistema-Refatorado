using LiberaMais.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LiberaMais.Models
{
    public class Financa
    {
        [Key]
        public int Id { get; set; }

        public string Descricao { get; set; }

        public int? PromotoraId { get; set; }

        public Promotora? Promotora { get; set; }

        public decimal Valor { get; set; }

        public DateTime Data { get; set; }

        public TipoFinanca Tipo { get; set; }

        public int Mes { get; set; }

        public int Ano { get; set; }

        public ContaSocio? ContaSocio { get; set; }

        public int? UsuarioId { get; set; }

        public UsuarioModel? Usuario { get; set; }





    }

}