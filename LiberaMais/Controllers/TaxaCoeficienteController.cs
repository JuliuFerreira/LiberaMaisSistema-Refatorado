using LiberaMais.Models;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;

namespace LiberaMais.Controllers
{
    public class TaxaCoeficienteController : Controller
    {

        private readonly ITaxaCoeficienteRepositorio _taxaCoeficienteRepositorio;

        public TaxaCoeficienteController(ITaxaCoeficienteRepositorio taxaCoeficienteRepositorio)
        {
            _taxaCoeficienteRepositorio = taxaCoeficienteRepositorio;
        }

        public IActionResult Index()
        {
            var Taxas = _taxaCoeficienteRepositorio.ListarTodos();

            return View(Taxas);
        }

        public IActionResult Criar()
        {

            var taxaCoeficiente = new TaxaCoeficiente
            {
                Ativo = true
            };

            return View(taxaCoeficiente);
        }

        [HttpPost]
        public IActionResult Criar(TaxaCoeficiente taxaCoeficiente)
        {

            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Erro ao Criar";
                return View();
            }

            _taxaCoeficienteRepositorio.Adicionar(taxaCoeficiente);
            TempData["MensagemSucesso"] = "Taxa Criada com sucesso.";
            return RedirectToAction("Index");

        }

        public IActionResult Editar(int id)
        {
            var taxas = _taxaCoeficienteRepositorio.BuscarPorId(id);

            if (taxas == null)
            {
                TempData["MensagemErro"] = "Não localizado.";
                return View();
            }

            return View(taxas);
        }

        [HttpPost]
        public IActionResult Editar(TaxaCoeficiente taxaCoeficiente)
        {
            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Erro ao realizar a alteração.";
                return View(taxaCoeficiente);
            }

            var taxaCoeficienteDb = _taxaCoeficienteRepositorio.BuscarPorId(taxaCoeficiente.Id);

            if (taxaCoeficienteDb == null)
            {
                TempData["MensagemErro"] = "Taxa não localizada.";
                return RedirectToAction("Index");
            }

            taxaCoeficienteDb.Operacao = taxaCoeficiente.Operacao;
            taxaCoeficienteDb.Taxa = taxaCoeficiente.Taxa;
            taxaCoeficienteDb.Coeficiente = taxaCoeficiente.Coeficiente;
            taxaCoeficienteDb.Ativo = taxaCoeficiente.Ativo;

            _taxaCoeficienteRepositorio.Atualizar(taxaCoeficienteDb);
            TempData["MensagemSucesso"] = "Taxa Editada com sucesso.";
            return RedirectToAction("Index");

        }

        [HttpPost]
        public IActionResult Apagar(int id)
        {
            try
            {
                var taxas = _taxaCoeficienteRepositorio.BuscarPorId(id);

                if(taxas.Ativo == true)
                {
                    TempData["MensagemErro"] = "Não é possível efetuar essa exclusão, taxa ativa.";
                    return RedirectToAction("Index");
                }


                bool Apagado = _taxaCoeficienteRepositorio.Apagar(id);

                if (Apagado)
                {
                    TempData["MensagemSucesso"] = "Taxa Excluida com sucesso.";
                    return RedirectToAction("Index");
                }

                else
                {
                    TempData["MensagemErro"] = "Não foi possível fazer a exclusão.";
                    return RedirectToAction("Index");
                }
            }

            catch (Exception ex) 
            {
                TempData["MensagemErro"] = "Ops, não foi possível excluir essa taxa!";

                return RedirectToAction("Index");
            }

            
        }
    }

}