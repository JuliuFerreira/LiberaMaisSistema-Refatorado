using LiberaMais.Models.Enums;

namespace LiberaMais.Models
{
    public class Agendamento
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }

        public int UsuarioId { get; set; }

        public DateTime DataCadastro { get; set; }
        
        public DateTime DataAgendamento { get; set; }

        public TipoAgendamentoEnum TipoAgendamento { get; set; }

        public string? Informacoes { get; set; }

        public Cliente? Cliente { get; set; }

        public UsuarioModel? Usuario { get; set; }
    }
}
