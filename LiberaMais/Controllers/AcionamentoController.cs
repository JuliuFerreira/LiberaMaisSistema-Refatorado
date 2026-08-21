using LiberaMais.Filters;
using LiberaMais.Helper;
using LiberaMais.Models;
using LiberaMais.Models.Enums;
using LiberaMais.Repositorio;
using LiberaMais.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security;

namespace LiberaMais.Controllers
{
    [PaginaParaUsuarioLogado]

    public class AcionamentoController : Controller
    {
        private readonly IAciomanentoRepositorio _aciomanentoRepositorio;
        private readonly IHistoricoRepositorio _historicoRepositorio;
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly ISessao _sessao;
        private readonly PermissaoService _permissaoService;

        public AcionamentoController(IAciomanentoRepositorio aciomanentoRepositorio, IUsuarioRepositorio usuarioRepositorio, ISessao sessao, PermissaoService permissaoService, IHistoricoRepositorio historicoRepositorio)
        {
            _aciomanentoRepositorio = aciomanentoRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
            _sessao = sessao;
            _permissaoService = permissaoService;
            _historicoRepositorio = historicoRepositorio;
        }

        [HttpGet]
        public IActionResult Agendamentos()
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();

            List<Acionamento> acionamentos;

            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                acionamentos = _aciomanentoRepositorio.ListarTodos();
            }
            else
            {
                acionamentos = _aciomanentoRepositorio.ListaPorUsuario(usuarioLogado.Id);
            }

            var agendamentos = acionamentos
                .SelectMany(a => a.Historicos ?? new List<Historico>())
                .GroupBy(h => h.AcionamentoId)
                .Select(g => g
                    .OrderByDescending(h => h.Data)
                    .FirstOrDefault())
                .Where(h =>
                    h != null &&
                    h.StatusEnum == StatusHistoricoEnum.Agendado &&
                    h.DataAgendamento.HasValue)
                .Select(h => new
                {
                    Id = h.Id,
                    AcionamentoId = h.AcionamentoId,
                    Nome = acionamentos
                        .FirstOrDefault(a => a.Id == h.AcionamentoId)?.Nome,
                    DataAgendamento = h.DataAgendamento
                })
                .ToList();

            return Json(agendamentos);
        }

        public IActionResult Index(string busca, int? usuarioId, int pagina = 1)
        {
            int tamanhoCorte = 10;

            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();

            bool isAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;
            ViewBag.IsAdmin = isAdmin;

            if (isAdmin)
            {
                ViewBag.Usuarios = _usuarioRepositorio.ListarTodosUsuarios();
            }

            int? usuarioIdFiltro = usuarioId;

            if(!isAdmin)
            {
                usuarioIdFiltro = usuarioLogado.Id;
            }

            ViewBag.UsuarioAtual = usuarioIdFiltro;


            int totalRegistro;

            var acionamentos = _aciomanentoRepositorio.BuscarPorNomeCpf(
                    busca,
                    pagina,
                    tamanhoCorte,
                    usuarioIdFiltro,
                    out totalRegistro
                );

            ViewBag.BuscaAtual = busca;
            ViewBag.PaginaAtual = pagina;
            ViewBag.TotalRegistros = totalRegistro;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistro / tamanhoCorte);

            var historico = _historicoRepositorio.ListarTodos();

            ViewBag.Historicos = historico;

            return View(acionamentos);
        }
        public IActionResult Criar()
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;



            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                ViewBag.Usuarios = _usuarioRepositorio.ListarTodosUsuarios();
            }

            else
            {
                ViewBag.Usuarios = new List<UsuarioModel>
                {
                usuarioLogado
                };
            }

            return View();

        }

        [HttpPost]
        public IActionResult Criar(int usuarioId, Acionamento acionamento)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            if (!_permissaoService.UsuarioTemAcessoAAcionamento(usuarioLogado, acionamento))
            {
                TempData["MensagemErro"] = "Você não tem acesso.";
                return RedirectToAction("Index");
            }


            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                ViewBag.Usuarios = _usuarioRepositorio.ListarTodosUsuarios();
            }

            else
            {
                ViewBag.Usuarios = new List<UsuarioModel>
                {
                    usuarioLogado
                };
            }


            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Não foi possível adicionar esse Lead.";
                return View(acionamento);
            }

            _aciomanentoRepositorio.Adicionar(acionamento);
            TempData["MensagemSucesso"] = "Lead adicionado com sucesso.";
            return RedirectToAction("Index");

        }

        public IActionResult Editar(int id, int usuarioId)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            var acionamento = _aciomanentoRepositorio.BuscarPorId(id);

            if (acionamento == null)
            {
                TempData["MensagemErro"] = "Lead não localizado.";
                return RedirectToAction("Index");
            }

            if (!_permissaoService.UsuarioTemAcessoAAcionamento(usuarioLogado, acionamento))
            {
                TempData["MensagemErro"] = "Você não tem acesso.";
                return RedirectToAction("Index");
            }

            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                ViewBag.Usuarios = _usuarioRepositorio.ListarTodosUsuarios();
            }

            else
            {
                ViewBag.Usuarios = new List<UsuarioModel>
                {
                    usuarioLogado
                };
            }

            return View(acionamento);
        }

        [HttpPost]
        public IActionResult Editar(Acionamento acionamento)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            var acionamentoDb = _aciomanentoRepositorio.BuscarPorId(acionamento.Id);

            if (!_permissaoService.UsuarioTemAcessoAAcionamento(usuarioLogado, acionamento))
            {
                TempData["MensagemErro"] = "Você não tem acesso.";
                return RedirectToAction("Index");
            }

            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                ViewBag.Usuarios = _usuarioRepositorio.ListarTodosUsuarios();
            }

            else
            {
                ViewBag.Usuarios = usuarioLogado.Id;
            }

            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Não foi possível efetuar a alteração.";
                return View(acionamento);
            }

            acionamentoDb.Nome = acionamento.Nome;
            acionamentoDb.Cpf = acionamento.Cpf;
            acionamentoDb.UsuarioId = acionamento.UsuarioId;

            _aciomanentoRepositorio.Atualizar(acionamentoDb);
            TempData["MensagemSucesso"] = "Alteração efetuada com sucesso";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult Apagar(int id)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            var acionamento = _aciomanentoRepositorio.BuscarPorId(id);

            if (acionamento == null)
            {
                TempData["MensagemErro"] = "Não foi possível efetuar a exclusão.";
                return RedirectToAction("Index");
            }

            if (!_permissaoService.UsuarioTemAcessoAAcionamento(usuarioLogado, acionamento))
            {
                TempData["MensagemErro"] = "Você não tem acesso.";
                return RedirectToAction("Index");
            }

            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                ViewBag.Usuarios = _usuarioRepositorio.ListarTodosUsuarios();
            }

            else
            {
                ViewBag.Usuarios = usuarioLogado.Id;
            }



            bool apagado = _aciomanentoRepositorio.Apagar(id);

            if (apagado)
            {
                TempData["MensagemSucesso"] = "Exclusão efetuada com sucesso.";
                return RedirectToAction("Index");
            }

            else
            {
                TempData["MensagemErro"] = "Não foi possível efetuar a exclusão.";
                return RedirectToAction("Index");
            }
        }


    }
}
