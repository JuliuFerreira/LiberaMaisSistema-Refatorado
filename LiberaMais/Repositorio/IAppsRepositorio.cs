using LiberaMais.Models;
using Microsoft.Identity.Client;

namespace LiberaMais.Repositorio
{
    public interface IAppsRepositorio
    {
        public List<App> ListarPorPeriodo(int mes, int ano);
    }
}
