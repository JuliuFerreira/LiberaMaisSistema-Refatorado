using LiberaMais.Filters;
using LiberaMais.Helper;
using LiberaMais.Models;
using LiberaMais.Models.Enums;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace LiberaMais.Controllers
{
    public class RegraBancoController : Controller
    {
        private readonly IRegraBancoRepositorio _regraBancoRepositorio;
        private readonly IPromotoraBancoRepositorio _promotoraBancoRepositorio;
        private readonly ISessao _sessao;

        public RegraBancoController(IRegraBancoRepositorio regraBancoRepositorio, IPromotoraBancoRepositorio promotoraBancoRepositorio, ISessao sessao)
        {
            _regraBancoRepositorio = regraBancoRepositorio;
            _promotoraBancoRepositorio = promotoraBancoRepositorio;
            _sessao = sessao;
        }

        public IActionResult Index(string nome, int pagina = 1)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            if (pagina < 1)
            {
                pagina = 1;
            }

            const int tamanhoPagina = 10;
            int totalRegistros;

            var listaRegras = _regraBancoRepositorio.BuscarPorNome(nome, pagina, tamanhoPagina, out totalRegistros);

            ViewBag.NomeAtual = nome;
            ViewBag.PaginaAtual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanhoPagina);


            return View(listaRegras);
            
        }


        [PaginaRestritaSomenteAdmin]
        public IActionResult Criar()
        {
            
            ViewBag.PromotoraBanco = _promotoraBancoRepositorio.ListarPromotoraBanco();

            var regra = new RegraBanco
            {
                DataAtualizacao = DateTime.Now
            };

             

            return View(regra);

        }

        [PaginaRestritaSomenteAdmin]
        [HttpPost]
        public IActionResult Criar(RegraBanco regraBanco)
        {
            if(regraBanco == null)
            {
                TempData["MensagemErro"] = "Não foi possível adicinar as regras do Banco / Promotora selecionada.";
                return View();
            }

            _regraBancoRepositorio.Adicionar(regraBanco);
            TempData["MensagemSucesso"] = "Regra adicionada com sucesso.";
            return RedirectToAction("Index");
        }

        public IActionResult Detalhe(int id)
        {
            var regra = _regraBancoRepositorio.BuscarPorId(id);

            return View(regra);
        }

        [PaginaRestritaSomenteAdmin]
        public IActionResult Editar (int id)
        {
            var regra = _regraBancoRepositorio.BuscarPorId(id);

            if(regra == null)
            {
                TempData["MensagemErro"] = "Não foi possível localizar as regras do Banco / Promotora selecionada.";
                return RedirectToAction("Index");
            }

            ViewBag.PromotoraBanco = _promotoraBancoRepositorio.ListarPromotoraBanco();            

            return View(regra);
        }

        [PaginaRestritaSomenteAdmin]
        [HttpPost]
        public IActionResult Editar(RegraBanco regraBanco)
        {

            ModelState.Remove("PromotoraBanco");


            if (!ModelState.IsValid)
            {
                ViewBag.PromotoraBanco = _promotoraBancoRepositorio.ListarPromotoraBanco();

                return View(regraBanco);
            }

            var regraBancoDb = _regraBancoRepositorio.BuscarPorId(regraBanco.Id);

            if(regraBancoDb == null)
            {
                TempData["MensagemErro"] = "Não foi possível localizar as regras do Banco / Promotora selecionada.";
                return RedirectToAction("Index");
            }


            regraBancoDb.PromotoraBancoId = regraBanco.PromotoraBancoId;
            regraBancoDb.RegrasValores = regraBanco.RegrasValores;
            regraBancoDb.RegraIdade = regraBanco.RegraIdade;
            regraBancoDb.BancoRegraGeral = regraBanco.BancoRegraGeral;
            regraBancoDb.BancoNaoPortado = regraBanco.BancoNaoPortado;
            regraBancoDb.BancoComRegra = regraBanco.BancoComRegra;
            regraBancoDb.DataAtualizacao = DateTime.Now;

            _regraBancoRepositorio.Atualizar(regraBancoDb);
            TempData["MensagemSucesso"] = "Regra alterada com sucesso.";
            return RedirectToAction("Index");
        }

        public IActionResult Apagar(int id)
        {
            var regra = _regraBancoRepositorio.BuscarPorId(id);

            if(regra == null)
            {
                TempData["MensagemErro"] = "Não foi possível Excluir as regras do Banco / Promotora selecionada.";
                return RedirectToAction("Index");
            }

            _regraBancoRepositorio.Apagar(id);
            TempData["MensagemSucesso"] = "Regra Excluida com sucesso.";
            return RedirectToAction("Index");
        }
    }
}
