namespace LiberaMais.Utils
{
    [Serializable]
    public static class Utils
    {
        public static int CalcularIdade(DateTime dataNascimento)
        {
            int idade = DateTime.Now.Year - dataNascimento.Year;
            if (DateTime.Now.DayOfYear < dataNascimento.DayOfYear)
            {
                idade = idade - 1;
            }
            return idade;
        }

        public static List<int> GerarAno()
        {
            var ano = DateTime.Now.Year;

            var listaAno = new List<int>();

            for(int i = 2024; i <= ano; i++)
            {
                listaAno.Add(i);
            }
            return listaAno;
        }


    }
}
