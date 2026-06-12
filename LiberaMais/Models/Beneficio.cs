namespace LiberaMais.Models
{
    [Serializable]
    public class Beneficio
    {
        public int Id { get; set; }

        public int Codigo { get; set; }

        public string Descricao { get; set; }

        public int OrgaoId { get; set; }

        public Orgao? Orgaos { get; set; }

    }
}
