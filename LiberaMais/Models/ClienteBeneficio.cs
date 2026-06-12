namespace LiberaMais.Models
{
    [Serializable]
    public class ClienteBeneficio
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }

        public int BeneficioId { get; set; }

        public string? NumeroBeneficio { get; set; }

        public string? SenhaOrgao { get; set; }

        public Cliente? Cliente { get; set; }

        public Beneficio? Beneficio {  set; get; }

    }
}
