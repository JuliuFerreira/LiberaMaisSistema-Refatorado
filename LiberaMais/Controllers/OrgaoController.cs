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
            var orgaoExistente = _orgaoRepositorio.BuscarPorNome(orgao.Nome);
            
            if(orgaoExistente != null)
            {
                TempData["MensagemErro"] = "Já existe um Órgao cadastrado com esse nome.";
                return View(orgao);
            }

            if (ModelState.IsValid)
            {
                orgao.Nome = orgao.Nome.ToUpper();
                orgao.Url = orgao.Url;
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
            var orgaoExistente = _orgaoRepositorio.BuscarPorNome(orgao.Nome);
            

            if(orgaoDb == null)
            {
                TempData["MensagemErro"] = "Nenhum não localizado.";
                return RedirectToAction("Index");
            }

            if(orgaoExistente != null && orgaoExistente.Id != orgao.Id)
            {
                TempData["MensagemErro"] = "Já existe um orgão cadastrado com esse nome.";
                return View(orgao);
            }

            if (ModelState.IsValid)
            {
                orgaoDb.Nome = orgao.Nome.ToUpper();
                orgaoDb.Url = orgao.Url;
                _orgaoRepositorio.Atualizar(orgaoDb);
                TempData["MensagemSucesso"] = "Orgão editado com sucesso!";
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
                TempData["MensagemErro"] = "Erro ao excluir o orgão, possivelmente existem beneficios cadastrados.";
            }
            return RedirectToAction("Index");


        }
    }
}
