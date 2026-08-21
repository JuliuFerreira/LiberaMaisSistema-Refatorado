using LiberaMais.Models;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace LiberaMais.Controllers
{
    public class ClienteBeneficioController : Controller
    {

        private readonly IClienteBeneficioRepositorio _clienteBeneficioRepositorio;
        private readonly IClienteRepositorio _clienteRepositorio;
        private readonly IBeneficioRepositorio _beneficioRepositorio;
        private readonly IOrgaoRepositorio _orgaoRepositorio;

        public ClienteBeneficioController(IClienteBeneficioRepositorio clienteBeneficioRepositorio,
            IClienteRepositorio clienteRepositorio,
            IBeneficioRepositorio beneficioRepositorio,
            IOrgaoRepositorio orgaoRepositorio
            )
        {
            _clienteBeneficioRepositorio = clienteBeneficioRepositorio;
            _clienteRepositorio = clienteRepositorio;
            _beneficioRepositorio = beneficioRepositorio;
            _orgaoRepositorio = orgaoRepositorio;
        }

        private void CarregarCombos()
        {
            ViewBag.cliente = _clienteRepositorio.ListarTodosClientes();
            ViewBag.beneficio = _beneficioRepositorio.ListarTodos();
            ViewBag.orgao = _orgaoRepositorio.ListarTodos();
        }

        public JsonResult BuscarBeneficiosPorOrgao(int orgaoId)
        {
            var beneficios = _beneficioRepositorio.BuscarPorOrgao(orgaoId);

            return Json(beneficios);
        }

        public IActionResult Index(string busca, int pagina = 1) // Mudamos de 'termo' para 'busca'
        {
            if (pagina < 1) pagina = 1;

            int tamanhoCorte = 10;
            int totalRegistros = 0;

            // Passamos o parâmetro 'busca' para o repositório
            var clienteBeneficioUnico = _clienteBeneficioRepositorio.BuscarPorNomeCpfPaginado(busca, pagina, tamanhoCorte, out totalRegistros);

            // PADRÃO DE VOCÊS: Nomeando as ViewBags igual ao ecossistema do sistema
            ViewBag.BuscaAtual = busca;
            ViewBag.PaginaAtual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanhoCorte);

            return View(clienteBeneficioUnico);
        }
        public IActionResult Criar(int clienteId)
        {
            CarregarCombos();

            var cliente = _clienteRepositorio.BuscarClientePorId(clienteId);

            ViewBag.ClienteSelecionado = cliente;

            var model = new ClienteBeneficio
            {
                ClienteId = clienteId
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Criar(ClienteBeneficio clienteBeneficio)
        {
            CarregarCombos();


            if (!ModelState.IsValid)
            {
                CarregarCombos();
                ViewBag.ClienteSelecionado = _clienteRepositorio.BuscarClientePorId(clienteBeneficio.ClienteId);
                TempData["MensagemErro"] = "Não foi possível adicionar o beneficio.";
                return View(clienteBeneficio);

            }

            _clienteBeneficioRepositorio.Adicionar(clienteBeneficio);
            TempData["MensagemSucesso"] = "Beneficio adicionado com sucesso.";
            return RedirectToAction("Detalhes", "Clientes", new { id = clienteBeneficio.ClienteId });

        }

        public IActionResult Editar(int id)
        {
            CarregarCombos();

            var clienteBeneficio = _clienteBeneficioRepositorio.BuscarPorId(id);

            if (clienteBeneficio == null)
            {
                TempData["MensagemErro"] = "Esse benefício não foi localizado.";
                return RedirectToAction("Index", "Clientes");
            }

            return View(clienteBeneficio);
        }

        [HttpPost]
        public IActionResult Editar(ClienteBeneficio clienteBeneficio)
        {
            CarregarCombos();

            var DbClienteBeneficio = _clienteBeneficioRepositorio.BuscarPorId(clienteBeneficio.Id);

            if (DbClienteBeneficio == null)
            {
                TempData["MensagemErro"] = "Benefício não localizado.";
                return RedirectToAction("Index", "Clientes");
            }

            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Não foi possível alterar o beneficio.";
                return View(clienteBeneficio);
            }

            DbClienteBeneficio.NumeroBeneficio = clienteBeneficio.NumeroBeneficio;
            DbClienteBeneficio.SenhaOrgao = clienteBeneficio.SenhaOrgao;

            _clienteBeneficioRepositorio.Atualizar(DbClienteBeneficio);
            TempData["MensagemSucesso"] = "Beneficio alterado com sucesso.";
            return RedirectToAction("Detalhes","Clientes",new { id = DbClienteBeneficio.ClienteId });
        }

        public IActionResult Deletar(int id)
        {
            var clienteBeneficio = _clienteBeneficioRepositorio.BuscarPorId(id);
            var clienteId = clienteBeneficio.ClienteId;


            if (clienteBeneficio == null)
            {
                TempData["MensagemErro"] = "Esse beneficio não foi localizado.";
                return RedirectToAction("Detalhes", "Clientes", new { id = clienteId });
            }

            return View(clienteBeneficio);
        }

        [HttpPost]
        public IActionResult Apagar(int id)
        {

            try
            {
                var clienteBeneficio = _clienteBeneficioRepositorio.BuscarPorId(id);

                if (clienteBeneficio == null)
                {
                    TempData["MensagemErro"] = "Benefício não localizado.";
                    return RedirectToAction("Index", "Clientes");
                }

                var clienteId = clienteBeneficio.ClienteId;

                var apagado = _clienteBeneficioRepositorio.Apagar(id);


                if (apagado)
                {
                    TempData["MensagemSucesso"] = "Beneficio Excluido com sucesso.";
                    return RedirectToAction("Detalhes","Clientes", new { id = clienteId });
                }

                else
                {
                    TempData["MensagemSucesso"] = "Beneficio não foi excluido.";
                    return RedirectToAction("Detalhes", "Clientes", new { id = clienteId });
                }

            }
            catch (Exception)
            {
                TempData["MensagemErro"] = "Esse beneficio não pode ser excluido.";
                return RedirectToAction("Index", "Clientes");
            }
        }
    }
}
