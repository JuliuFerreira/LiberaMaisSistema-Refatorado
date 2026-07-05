using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace LiberaMais.Models.Enums
{


    public enum OperacaoEnum : int
    {
        NOVO = 1,

        [Display(Name = ("CARTÃO BENEFICIO"))]
        CartaoBeneficio = 2,

        [Display(Name = ("CARTÃO CONSIGNADO"))]
        CartaoConsignado = 3,

        [Display(Name = ("REFINANCIAMENTO"))]
        REFINANCIAMENTO = 4,

        [Display(Name = ("PORTABILIDADE"))]
        PORTABILIDADE = 5,

        [Display(Name = ("REFIN DA PORT"))]
        RefinDaPort = 6,

        [Display(Name = ("SAQUE COMPLEMENTAR"))]
        SaqueComplementar = 7,

        [Display(Name = ("SAQUE ANIVERSÁRIO"))]
        SaqueAniversario = 8,

        AUMENTO = 9,

        [Display(Name = ("REFIN AUTO"))]
        RefinAuto = 10

    }



    public enum OrgaoEnum : int
    {
        [Display(Name = ("INSS"))]
        INSS = 1,

        [Display(Name = ("FGTS"))]
        FGTS = 2,

        [Display(Name = ("SIAPE"))]
        SIAPE = 3,

        [Display(Name = ("ESTADO DE SC"))]
        EstadoDeSc = 4,

        [Display(Name = ("PREFEITURA DE FLORIANÓPOLIS"))]
        PrefeituraDeFlorianopolis = 5,

        [Display(Name = ("PREFEITURA DE SÃO JOSE"))]
        PrefeituraDeSaoJose = 6,

        [Display(Name = ("OUTROS"))]
        OUTROS = 7,

        [Display(Name = ("CRÉDITO PESSOAL"))]
        Pessoal = 8,

        [Display(Name = ("EMPRÉSTIMO CLT"))]
        Clt = 9,

        [Display(Name = ("PREFEITURA DE PALHOÇA"))]
        PrefeituraDePalhoca = 10,

        [Display(Name = ("PREFEITURA DE BIGUAÇU"))]
        PrefeituraDeBiguacu = 11


    }

    public enum BeneficioEnum : int
    {
        [Display(Name = "APOSENTADORIA")]
        APOSENTADORIA = 1,

        [Display(Name = ("PENSÃO"))]
        Pensao = 2,

        [Display(Name = ("BPC-LOAS"))]
        LOAS = 3,

        [Display(Name = ("SERVIDOR ATIVO"))]
        ATIVO = 4,

        [Display(Name = ("FGTS"))]
        FGTS = 5,

        [Display(Name = ("CRÉDITO PESSOAL"))]
        PESSOAL = 6,

        [Display(Name = ("OUTROS"))]
        Auto = 7,

        [Display(Name = ("CRÉDITO DO TRABALHADOR"))]
        Clt = 8
    }

    public enum StatusEnum : int
    {
        [Display(Name = ("DIGITADO"))]
        Digitado = 1,

        [Display(Name = ("ASSINADO"))]
        Assinado = 2,

        [Display(Name = ("CONTRATO PAGO AGUARDA COMISSÃO"))]
        Pago = 4,

        [Display(Name = ("COMISSÃO PAGA"))]
        ComissaoPaga = 5,

        [Display(Name = ("CANCELADO"))]
        Cancelado = 6
    }


    public enum ComissaoEnum : int
    {
        [Display(Name = ("COMISSÃO EM ABERTO"))]
        EmAberto = 1,

        [Display(Name = ("COMISSÃO RECEBIDA"))]
        Pago = 2,

        [Display(Name = ("COMISSÃO ATRASADA"))]
        EmAtraso = 3
    }

    public enum BancoEnum : int
    {
        [Display(Name = ("BANCO ITAÚ"))]
        Itau = 1,

        [Display(Name = ("BANCO DIGIO"))]
        Digio = 2,

        [Display(Name = ("BANCO PAN"))]
        Pan = 3,

        [Display(Name = ("BANCO BRB"))]
        Brb = 4,

        [Display(Name = ("BANCO SAFRA"))]
        Safra = 5,

        [Display(Name = ("BANCO QUERO MAIS"))]
        QueroMais = 6,

        [Display(Name = ("BANCO C6"))]
        C6 = 7,

        [Display(Name = ("BANCO DAYCOVAL"))]
        Daycoval = 8,

        [Display(Name = ("BANCO PB CONSIGNADO (FRONT)"))]
        Parana = 9,

        [Display(Name = ("BANCO OLÉ SANTANDER"))]
        OleSantander = 10,

        [Display(Name = ("BANCO BMG"))]
        Bmg = 11,

        [Display(Name = ("BANCO CREFISA"))]
        Crefisa = 12,

        [Display(Name = ("BANCO FACTA"))]
        Facta = 13,

        [Display(Name = ("BANCO ICRED"))]
        Icred = 14,

        [Display(Name = ("BANCO BANRISUL"))]
        Banrisul = 15,

        [Display(Name = ("BANCO PAGBANK"))]
        Pagbank = 16,

        [Display(Name = ("BANCO MERCANTIL"))]
        Mercantil = 17,

        [Display(Name = ("BANCO INBURSA"))]
        Inbursa = 18,

        [Display(Name = ("BANCO IC DIGITAL"))]
        IcDigital = 19,

        [Display(Name = ("BANCO SANTANDER FVE"))]
        Santander = 20,

        [Display(Name = ("BANCO DAYCOVAL DIGITAL"))]
        DaycovalDigital = 21,

        [Display(Name = ("BANCO MASTER"))]
        Master = 22,

        [Display(Name = ("BANCO PB CONSIGNADO (PORTAL)"))]
        PortalPb = 23,

        [Display(Name = ("BANCO BARI"))]
        Bari = 24,

        [Display(Name = ("BANCO BARI (DIGITAÇÃO)"))]
        BariDig = 25,

        [Display(Name = ("BANCO HAPPY"))]
        BancoHappy = 26,

        [Display(Name = ("CAIXA ECONÔMICA"))]
        CaixaEconomicaFederal = 27,

        [Display(Name = ("PRESENÇA BANK"))]
        PresencaBank = 28,

        [Display(Name = ("BANCO DO BRASIL"))]
        BancoDoBrasil = 29,

        [Display(Name = ("BANCO FOX"))]
        Fox = 30,

        [Display(Name = ("NBC BANK"))]
        Nbc = 31,

        [Display(Name = ("PICPAY"))]
        Picpay = 32,

        [Display(Name = ("MOTIVA"))]
        Motiva = 33,

        [Display(Name = ("FINANTO"))]
        Finanto = 34,

        [Display(Name = ("QUALIBANK"))]
        Quali = 35,

        [Display(Name = ("AMIGOZ"))]
        Amigoz = 36,

        [Display(Name = ("MEU CASHCARD"))]
        Cashcard = 37,

        [Display(Name = ("SENFF"))]
        Senff = 38,

        [Display(Name = ("RED CONSIG BRB"))]
        Redconsig = 39,

        [Display(Name = ("CONSIG 360 BRB"))]
        Consig360 = 40,

        [Display(Name = ("FULL CONSIG"))]
        Fullconsig = 41,

        [Display(Name = ("MEU CREDBANK"))]
        Credbank = 42,

        [Display(Name = ("CRED CAPITAL"))]
        Credcapital = 43,

        [Display(Name = ("CAPITAL CONSIG"))]
        Capitalconsig = 44

    }

    public enum PromotoraEnum : int
    {
        [Display(Name = ("BEVI"))]
        BEVI = 1,

        [Display(Name = ("NOVA PROMOTORA"))]
        NOVA = 2,

        [Display(Name = ("CONECT PROMOTORA"))]
        CONECT = 3,

        [Display(Name = ("GOLD PROMOTORA"))]
        GOLD = 4,

        [Display(Name = ("PONTO AMIGO"))]
        PontoAmigo = 5,

        [Display(Name = ("OUTROS"))]
        Outros = 6,

        [Display(Name = ("GFT PROMOTORA"))]
        Gft = 7,

        [Display(Name = ("UNICA PROMOTORA"))]
        Unica = 8,

        [Display(Name = ("CREDFRANCO"))]
        CredFranco = 9,

        [Display(Name = ("CREDLEVE"))]
        CredLeve = 10,

        [Display(Name = ("AMF PROMOTORA"))]
        Amf = 11,

        [Display(Name = ("TEDDY CONSIG"))]
        Teddy = 12

    }

    public enum UsuarioEnum : int
    {
        [Display(Name = ("JULIO C. FERREIRA"))]
        JULIO = 1,

        [Display(Name = ("RAFAEL DA COSTA"))]
        RAFAEL = 2
    }

    public enum DocumentoEnum : int
    {
        [Display(Name = ("RG COMPLETO"))]
        RgCompleto = 1,

        [Display(Name = ("RG FRENTE"))]
        RgFrente = 2,

        [Display(Name = ("RG VERSO"))]
        RgVerso = 3,

        [Display(Name = ("CNH"))]
        CNH = 4,

        [Display(Name = ("OUTROS"))]
        OUTROS = 5
    }


    public enum PerfilEnum : int
    {
        Admin = 1,
        Padrao = 2
    }

    public enum MesEnum : int
    {
        [Display(Name = ("JANEIRO"))]
        Janeiro = 1,

        [Display(Name = ("FEVEREIRO"))]
        Fevereiro = 2,

        [Display(Name = ("MARÇO"))]
        Março = 3,

        [Display(Name = ("ABRIL"))]
        Abril = 4,

        [Display(Name = ("MAIO"))]
        Maio = 5,

        [Display(Name = ("JUNHO"))]
        Junho = 6,

        [Display(Name = ("JULHO"))]
        Julho = 7,

        [Display(Name = ("AGOSTO"))]
        Agosto = 8,

        [Display(Name = ("SETEMBRO"))]
        Setembro = 9,

        [Display(Name = ("OUTUBRO"))]
        Outubro = 10,

        [Display(Name = ("NOVEMBRO"))]
        Novembro = 11,

        [Display(Name = ("DEZEMBRO"))]
        Dezembro = 12
    }

    public enum AnoEnum : int
    {
        [Display(Name = ("2024"))]
        a2024 = 1,

        [Display(Name = ("2025"))]
        a2025 = 2,

        [Display(Name = ("2026"))]
        a2026 = 3,

        [Display(Name = ("2027"))]
        a2027 = 4,

        [Display(Name = ("2028"))]
        a2028 = 5,

        [Display(Name = ("2029"))]
        a2029 = 6,

        [Display(Name = ("2030"))]
        a2030 = 7

    }

}
