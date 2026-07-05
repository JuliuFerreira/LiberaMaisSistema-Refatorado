namespace LiberaMais.Models
{
    public class DashboardVendaViewModel
    {
        public int Digitado { get; set; }

        public int Assinado { get; set; }

        public int Pago { get; set; }

        public int Cancelado { get; set; }

        public int ComissaoPaga { get; set; }

        public bool PossuiComissaoAtrasada { get; internal set; }

        public int ContratosComissaoAtrasada { get; internal set; }

        public List<Venda> ListaContratos { get; set; }


    }
}
