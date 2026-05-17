using LiberaMais.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LiberaMais.Models
{
    [Serializable]
    public class Cliente
    {
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long IdCliente { get; set; }

        [ForeignKey("Usuarios")]
        public int UsuarioId { get; set; }

        public string? UsuarioNome { get; set; }

        public UsuarioModel? Usuario { get; set; }

        [Remote(action: "VerificarCpfUnico", controller: "Clientes", ErrorMessage = "Este CPF já está em uso.")]
        [Required(ErrorMessage = "Por favor, insira o CPF")]
        public string Cpf { get; set; }

        [Required(ErrorMessage = "Por favor, digite o Nome")]
        public string Nome {  get; set; }

        public string? SenhaInss {  get; set; }

        [Required(ErrorMessage = "Por favor, digite a data de nascimento")]
        public DateTime DataNascimento {  get; set; }

        [NotMapped]
        public int Idade
        {
            get
            {
                return Utils.Utils.CalcularIdade(DataNascimento);
            }
        }

        [NotMapped]
        public bool IsAniversario { get; set; }
        public string? Cep { get; set; }

        public string? Logradouro { get; set; }

        public int? Ncasa { get; set; }

        public string? Complemento { get; set; }

        public string? Bairro { get; set; }

        public string? Cidade { get; set; }

        public string? Estado { get; set; }

        [Phone(ErrorMessage ="O número informado não é válido")]
        [Required(ErrorMessage ="Informe um número de telefone")]
        [RegularExpression(@"^\(?\d{2}\)?[\s-]?\d{5}-?\d{4}$", ErrorMessage = "Por favor, insira um número de telefone válido.")]
        public string Tel1 { get; set; }

        [Phone(ErrorMessage = "O número informado não é válido")]
        public string? Tel2 { get; set; }

        [EmailAddress(ErrorMessage = "Digite um email válido!")]
        public string? Email { get; set; }

        public string? observacoes { get; set; }       

        

    }
}
