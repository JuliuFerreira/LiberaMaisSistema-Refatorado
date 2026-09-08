using LiberaMais.Helper;
using LiberaMais.Models;
using LiberaMais.Models.Enums;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;

namespace LiberaMais.Controllers
{
    public class AgendamentoController : Controller
    {
        private readonly IAgendamentoRepositorio _agendamentoRepositorio;
        private readonly IClienteBeneficioRepositorio _clienteRepositorio;
        private readonly IUsuarioRepositorio _usuariosRepositorio;
        private readonly ISessao _sessao;
        public AgendamentoController(IAgendamentoRepositorio agendamentoRepositorio, IClienteBeneficioRepositorio clienteRepositorio, IUsuarioRepositorio usuariosRepositorio, ISessao sessao)
        {
            _agendamentoRepositorio = agendamentoRepositorio;
            _clienteRepositorio = clienteRepositorio;
            _usuariosRepositorio = usuariosRepositorio;
            _sessao = sessao;
        }

        public IActionResult Agendamentos()
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();

            List<Agendamento> agendamentos;

            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                agendamentos = _agendamentoRepositorio.ListarTodos();
            }
            else
            {
                agendamentos = _agendamentoRepositorio.BuscarPorUsuario(usuarioLogado.Id);
            }

            var resultado = agendamentos.Where(a => a.DataAgendamento != null).Select(a => new
            {
                Id = a.Id,
                ClienteId = a.ClienteId,
                Nome = a.Cliente?.Nome,
                DataAgendamento = a.DataAgendamento
            }).ToList();

            return Json(resultado);
        }



        public IActionResult Index(string termo, int? usuarioId, int pagina = 1)
        {
            int tamanhoCorte = 10;
                
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            bool isAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;
            ViewBag.IsAdmin = isAdmin;

            if(isAdmin)
            {
                ViewBag.Usuarios = _usuariosRepositorio.ListarTodosUsuarios();
            }

            int? usuarioIdFiltro = usuarioId;

            if(!isAdmin)
            {
                usuarioIdFiltro = usuarioLogado.Id;
            }

            ViewBag.UsuarioAtual = usuarioIdFiltro;

            int totalRegistro;

            var agendamento = _agendamentoRepositorio.BuscarCompleto(
                termo,
                pagina,
                tamanhoCorte,
                usuarioIdFiltro,
                out totalRegistro
                
                );

            ViewBag.BuscaAtual = termo;
            ViewBag.PaginaAtual = pagina;
            ViewBag.TotalRegistros = totalRegistro;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistro / tamanhoCorte);             



                return View(agendamento);
        }

        public IActionResult BuscarCliente (string termo)
        {
            var clientes = _agendamentoRepositorio.BuscarCliente(termo);

            return Json(clientes);
        }

        public IActionResult Criar()
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            if(usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                ViewBag.Usuarios = _usuariosRepositorio.ListarTodosUsuarios();
            }

            else
            {
                ViewBag.Usuarios = new List<UsuarioModel>
                {
                    usuarioLogado
                };
            }

            var agendamento = new Agendamento
            {
                DataCadastro = DateTime.Now,
                DataAgendamento = DateTime.Now
            };


            return View(agendamento);
        }

        [HttpPost]
        public IActionResult Criar (Agendamento agendamento)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                ViewBag.Usuarios = _usuariosRepositorio.ListarTodosUsuarios();
            }

            else
            {
                ViewBag.Usuarios = new List<UsuarioModel>
                {
                    usuarioLogado
                };

                agendamento.UsuarioId = usuarioLogado.Id;
            }

            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Erro ao criar o agendamento.";
                return View(agendamento);
            }

            _agendamentoRepositorio.Adicionar(agendamento);
            TempData["MensagemSucesso"] = "Agendamento criado com sucesso.";
            return RedirectToAction("Index");

        }

        public IActionResult Editar (int id)
        {
            var agendamento = _agendamentoRepositorio.BuscarPorId(id);

            if (agendamento == null)
            {
                TempData["MensagemErro"] = "Agendamento não localizado.";
                return RedirectToAction("Index");
            }

            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                ViewBag.Usuarios = _usuariosRepositorio.ListarTodosUsuarios();
            }

            else
            {
                ViewBag.Usuarios = new List<UsuarioModel>
                {
                    usuarioLogado
                };

                agendamento.UsuarioId = usuarioLogado.Id;
            }
                       

            return View(agendamento);

        }

        [HttpPost]
        public IActionResult Editar(Agendamento agendamento)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            var agendamentoDb = _agendamentoRepositorio.BuscarPorId(agendamento.Id);

            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                ViewBag.Usuarios = _usuariosRepositorio.ListarTodosUsuarios();
            }

            else
            {
                ViewBag.Usuarios = new List<UsuarioModel>
                {
                    usuarioLogado
                };

                agendamento.UsuarioId = usuarioLogado.Id;
            }

            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Erro ao criar o agendamento.";
                return View(agendamento);
            }

            agendamentoDb.UsuarioId = agendamento.UsuarioId;
            agendamentoDb.DataAgendamento = agendamento.DataAgendamento;
            agendamentoDb.Informacoes = agendamento.Informacoes;

            _agendamentoRepositorio.Atualizar(agendamentoDb);
            TempData["MensagemSucesso"] = "Agendamento alterado com sucesso.";
            return RedirectToAction("Index");


        }

        [HttpPost]
        public IActionResult Apagar(int id)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();

            var agendamento = _agendamentoRepositorio.BuscarPorId(id);

            if (agendamento == null)
            {
                TempData["MensagemErro"] = "Agendamento não localizado.";
                return RedirectToAction("Index");
            }

            _agendamentoRepositorio.Apagar(id);

            TempData["MensagemSucesso"] = "Agendamento finalizado com sucesso.";

            return RedirectToAction("Index");
        }
    }
}
