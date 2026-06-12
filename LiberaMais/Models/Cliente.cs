using AspNetCoreGeneratedDocument;
using LiberaMais.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace LiberaMais.Models
{
    [Serializable]
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O CPF é obrigatório")]
        [StringLength(14)]
        public string Cpf { get; set; }

        [Required(ErrorMessage = "O Nome é obrigatório")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "A data de nascimento é obrigatória")]
        public DateTime? DataNascimento { get; set; }

        [Required(ErrorMessage = "O telefone é obrigatório")]
        public string Fone { get; set; }

        [EmailAddress(ErrorMessage = "Digite um email válido")]
        public string? Email { get; set; }

        public string? Observacoes { get; set; }

        public Endereco? Endereco { get; set; }

        public int UsuarioId { get; set; }

        public UsuarioModel? Usuario { get; set; }

        [ValidateNever]
        public virtual List<ClienteBeneficio> ClienteBeneficios { get; set; }
        //public string ValidarCpf (string Cpf)
        //{
        //    if (string.IsNullOrWhiteSpace(Cpf))
        //    {
        //        return "Cpf inválido";
        //    }

        //    return string.Empty;
        //}

    }
}
