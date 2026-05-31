using LiberaMais.Filters;
using LiberaMais.Models;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace LiberaMais.Controllers
{
    [PaginaRestritaSomenteAdmin]
    public class OrgaoController : Controller
    {
        private readonly IOrgaoRepositorio _orgaoRepositorio;

        public OrgaoController(IOrgaoRepositorio orgaoRepositorio)
        {
            _orgaoRepositorio = orgaoRepositorio;
        }

        public IActionResult Index()
        {
            List<Orgao> orgao = _orgaoRepositorio.ListarTodos();


            return View(orgao);
        }

        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Criar(Orgao orgao)
        {
            
            if (ModelState.IsValid)
            {
                _orgaoRepositorio.Adicionar(orgao);
                TempData["MensagemSucesso"] = "Orgão adicionado com sucesso.";
                return RedirectToAction("Index");
            }

            TempData["MensagemErro"] = "Não foi possível criar o orgão.";

            return View(orgao);
        }

        public IActionResult Editar(int id)
        {
            var orgao = _orgaoRepositorio.BuscarPorId(id);

            if(orgao == null)
            {
                TempData["MensagemErro"] = "Orgão não localizado.";
                return RedirectToAction("Index");
            }

            return View(orgao);

        }

        [HttpPost]
        public IActionResult Editar(Orgao orgao)
        {
            var orgaoDb = _orgaoRepositorio.BuscarPorId(orgao.Id);

            if(orgaoDb == null)
            {
                TempData["MensagemErro"] = "Nenhum não localizado.";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                orgaoDb.Nome = orgao.Nome;
                _orgaoRepositorio.Atualizar(orgaoDb);
                TempData["MensagemSucesso"] = "Orgão editado com sucesso.";
                return RedirectToAction("Index");
            }

            TempData["MensagemErro"] = "Não foi possível editar o orgão.";

            return View(orgao);

        }

        public IActionResult Deletar (int id)
        {
            var orgao = _orgaoRepositorio.BuscarPorId(id);

            if(orgao == null)
            {
                TempData["MensagemErro"] = "Orgão não localizado.";
                return RedirectToAction("Index");
            }

            return View(orgao);
        }

        [HttpPost]
        public IActionResult Apagar (int id)
        {
            try
            {
                bool apagado = _orgaoRepositorio.Apagar(id);

                if (apagado)
                {
                    TempData["MensagemSucesso"] = "Orgão excluido com sucesso.";
                    return RedirectToAction("Index");
                }

                else
                {
                    TempData["MensagemErro"] = "Não foi possível apagar esse orgão.";
                    return RedirectToAction("Index");
                }
            }

            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Erro ao excluir o orgão.";
            }
            return RedirectToAction("Index");


        }
    }
}
