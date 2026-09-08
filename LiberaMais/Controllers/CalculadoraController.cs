using LiberaMais.Models;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;

namespace LiberaMais.Controllers
{
    public class CalculadoraController : Controller
    {

        private readonly ITaxaCoeficienteRepositorio _taxaCoeficienteRepositorio;

        public CalculadoraController(ITaxaCoeficienteRepositorio taxaCoeficienteRepositorio)
        {
            _taxaCoeficienteRepositorio = taxaCoeficienteRepositorio;
        }

        public IActionResult Index(TaxaCoeficiente taxaCoeficiente)
        {

            ViewBag.TaxaCoeficiente = _taxaCoeficienteRepositorio.ListarTodos().Where(t => t.Ativo).OrderBy(t => t.Taxa).ToList();



            return View();
        }
    }
}
