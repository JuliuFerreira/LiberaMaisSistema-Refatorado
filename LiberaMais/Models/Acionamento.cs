namespace LiberaMais.Models
{
    public class Acionamento
    {
        public int Id { get; set; }

        public DateTime? DataCadastro { get; set; }

        public int UsuarioId { get; set; }

        public string Nome { get; set; }

        public string Cpf { get; set; }

        public string? UltimoStatus { get; set; }

        public UsuarioModel? Usuario { get; set; }

        public ICollection<Historico> Historicos { get; set; } = new List<Historico>();

    }
}
