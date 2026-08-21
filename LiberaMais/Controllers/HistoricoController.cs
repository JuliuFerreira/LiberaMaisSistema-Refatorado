using LiberaMais.Helper;
using LiberaMais.Models;
using LiberaMais.Models.Enums;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;

namespace LiberaMais.Controllers
{
    public class HistoricoController : Controller
    {
        private readonly IHistoricoRepositorio _historicoRepositorio;
        private readonly IAciomanentoRepositorio _aciomanentoRepositorio;
        private readonly ISessao _sessao;
        private readonly IUsuarioRepositorio _usuarioRepositorio;

        public HistoricoController (IHistoricoRepositorio historicoRepositorio, IAciomanentoRepositorio acionamentoRepositorio, ISessao sessao, IUsuarioRepositorio usuarioRepositorio)
        {
            _aciomanentoRepositorio = acionamentoRepositorio;
            _historicoRepositorio = historicoRepositorio;
            _sessao = sessao;
            _usuarioRepositorio = usuarioRepositorio;
        }

        public IActionResult Index(int acionamentoId)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();

            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            var acionamento = _aciomanentoRepositorio.BuscarPorId(acionamentoId);
           

            if(acionamento == null)
            {
                return NotFound();
            }

            ViewBag.Acionamento = acionamento;

            if(usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                var historico = _historicoRepositorio.ListarPorAcionamento(acionamentoId);
                return View(historico);
            }

            if(acionamento.UsuarioId != usuarioLogado.Id)
            {
                return Forbid();
            }

            var historicoUsuario = _historicoRepositorio.ListarPorAcionamento(acionamentoId);

            return View(historicoUsuario);
        }

        public IActionResult Criar(int acionamentoId)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            var acionamento = _aciomanentoRepositorio.BuscarPorId(acionamentoId);
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            if (acionamento == null)
            {
                return NotFound();
            }

            if(usuarioLogado.Perfil == PerfilEnum.Admin)
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

            var ultimoHistorico = _historicoRepositorio.ListarPorAcionamento(acionamentoId).OrderByDescending(a => a.Data).FirstOrDefault();//

                var historico = new Historico
                {
                    AcionamentoId = acionamentoId,
                    Data = DateTime.Now,
                    Telefone = ultimoHistorico?.Telefone,//
                };

            ViewBag.Acionamento = acionamento;

            return View("Criar", historico);//
        }

        [HttpPost]
        public IActionResult Criar (Historico historico)
        {

            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Dados incompletos ou errados.";
                return RedirectToAction("Index", new {acionamentoId = historico.AcionamentoId});
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


            historico.Data = historico.Data.Date.Add(DateTime.Now.TimeOfDay);
            _historicoRepositorio.Adicionar(historico);
            TempData["MensagemSucesso"] = "Historico adicionado com sucesso.";
            return RedirectToAction("Index", new { acionamentoId = historico.AcionamentoId });

        }

        //public IActionResult Editar(int id)
        //{
        //    var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
        //    ViewBag.Usuarios = usuarioLogado.Perfil == PerfilEnum.Admin;

        //    var historico = _historicoRepositorio.BuscarPorId(id);

        //    if (historico == null)
        //    {
        //        TempData["MensagemErro"] = "Histórico não localizado.";
        //        return RedirectToAction("Index");
        //    }

        //    return View(historico);
        //}

        [HttpPost]
        public IActionResult Apagar(int id)
        {
            var historico = _historicoRepositorio.BuscarPorId(id);

            if (historico == null)
            {
                TempData["MensagemErro"] = "Histórico não localizado.";
                return RedirectToAction("Index");
            }

            var acionamentoId = historico.AcionamentoId;

            _historicoRepositorio.Apagar(id);

            TempData["MensagemSucesso"] = "Registro excluído com sucesso.";

            return RedirectToAction("Index", new { acionamentoId });
        }
    }


}
