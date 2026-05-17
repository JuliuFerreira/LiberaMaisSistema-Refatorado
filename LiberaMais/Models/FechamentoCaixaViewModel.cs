using LiberaMais.Models.Enums;

namespace LiberaMais.Models
{
    public class FechamentoCaixaViewModel
    {

        public int Mes { get; set; }
        public MesEnum Mesdescription => (MesEnum)this.Mes;
        public int Ano { get; set; }
        public decimal ReceitaContaJulio { get; set; }
        public decimal DespesaContaJulio { get; set; }
        public decimal ReceitaContaRafael { get; set; }
        public decimal DespesaContaRafael { get; set; }

        // Certifique-se de inicializar as propriedades calculadas corretamente
        public decimal SaldoContaJulio => ReceitaContaJulio - DespesaContaJulio;
        public decimal SaldoContaRafael => ReceitaContaRafael - DespesaContaRafael;

        public decimal ReceitaTotal => ReceitaContaJulio + ReceitaContaRafael;

        public decimal DespesaTotal => DespesaContaJulio + DespesaContaRafael;

        public decimal SaldoTotal => SaldoContaJulio + SaldoContaRafael;

        public decimal Salario => SaldoTotal / 2;
                
        public string DiferencaSaldoParaIgualarSalario { get; set; }

    }    
}
