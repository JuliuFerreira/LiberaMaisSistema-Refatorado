using LiberaMais.Models.Enums;

namespace LiberaMais.Models
{
    public class Historico
    {
        public int Id { get; set; }

        public int AcionamentoId { get; set; }

        public int UsuarioId { get; set; }

        public DateTime Data { get; set; }

        public string Telefone { get; set; }

        public StatusHistoricoEnum StatusEnum { get; set; }

        public DateTime? DataAgendamento { get; set; }

        public string? RegistroHistorico { get; set; }

        public Acionamento? Acionamento { get; set; }

        public UsuarioModel? Usuario { get; set; }
    }
}
