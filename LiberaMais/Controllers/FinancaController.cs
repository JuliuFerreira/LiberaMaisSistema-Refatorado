using LiberaMais.Filters;
using LiberaMais.Models;
using LiberaMais.Models.Enums;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace LiberaMais.Controllers
{
    [PaginaRestritaSomenteAdmin]
    public class FinancaController : Controller
    {
        private readonly IFinancaRepositorio _financaRepositorio;
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IPromotorasRepositorio _promotorasRepositorio;

        public FinancaController(IFinancaRepositorio financaRepositorio, IUsuarioRepositorio usuarioRepositorio, IPromotorasRepositorio promotorasRepositorio)
        {
            _financaRepositorio = financaRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
            _promotorasRepositorio = promotorasRepositorio;
        }


        public IActionResult Index(int? mes, int? ano, int? usuarioId)
        {
            // Se o usuário não enviou o mês/ano, assume o atual
            int mesAtual = mes ?? DateTime.Now.Month;
            int anoAtual = ano ?? DateTime.Now.Year;

            ViewBag.Mes = mesAtual;
            ViewBag.Ano = anoAtual;
            ViewBag.UsuarioSelecionado = usuarioId;
            ViewBag.Usuarios = _usuarioRepositorio.BuscarTodos();

            // Filtra pelo Mês/Ano e (opcionalmente) pelo Usuário
            var lista = _financaRepositorio.ListarPorPeriodo(mesAtual, anoAtual, usuarioId);

            return View(lista);
        }

        public IActionResult Fechamento(int mes, int ano)
        {
            // Busca todos os dados daquele mês específico, sem filtro de conta
            var lista = _financaRepositorio.ListarPorPeriodo(mes, ano, null);

            ViewBag.Mes = mes;
            ViewBag.Ano = ano;

            return View(lista);
        }

        [HttpGet]
        public IActionResult Criar()
        {
            ViewBag.Usuarios = _usuarioRepositorio.BuscarTodos();
            ViewBag.Promotoras = _promotorasRepositorio.ListarPromotora();

            var financa = new Financa();

            financa.Data = DateTime.Now;

            if (TempData["FinancaData"] != null)
            {
                financa.Data = DateTime.Parse(TempData["FinancaData"].ToString());
            }

            if (TempData["FinancaValor"] != null)
            {
                financa.Valor = decimal.Parse(
                    TempData["FinancaValor"].ToString(),
                    System.Globalization.CultureInfo.InvariantCulture);
            }

            if (TempData["FinancaTipo"] != null)
            {
                financa.Tipo = Enum.Parse<TipoFinanca>(
                    TempData["FinancaTipo"].ToString());
            }

            if (TempData["FinancaDescricao"] != null)
            {
                financa.Descricao = TempData["FinancaDescricao"]?.ToString();
            }

            return View(financa);
        }


        [HttpPost]
        public IActionResult Criar(Financa financa)
        {
            if (ModelState.IsValid)
            {
                financa.Mes = financa.Data.Month;
                financa.Ano = financa.Data.Year;

                _financaRepositorio.Adicionar(financa);
                TempData["MensagemSucesso"] = "Finança adicionada com sucesso.";

                return RedirectToAction("Index", new {mes = financa.Mes, ano = financa.Ano, usuarioId = financa.UsuarioId});
                    
            }

            else
            {
                TempData["MensagemErro"] = "Não foi possível adicionar a finança.";

            }

            ViewBag.Usuarios = _usuarioRepositorio.BuscarTodos();
            ViewBag.Promotoras = _promotorasRepositorio.ListarPromotora();
            return View(financa);
        }

        public IActionResult Editar (int id)
        {
            var financa = _financaRepositorio.BuscarPorId(id);
            ViewBag.Usuarios = _usuarioRepositorio.BuscarTodos();
            ViewBag.Promotoras = _promotorasRepositorio.ListarPromotora();

            if(financa == null)
            {
                TempData["MensagemErro"] = "Registro não localizado.";
                return RedirectToAction("Index");
            }

            return View(financa);
        }

        [HttpPost]
        public IActionResult Editar(Financa financa)
            {
            // Recarrega os dados necessários caso a validação falhe
            ViewBag.Usuarios = _usuarioRepositorio.BuscarTodos();
            ViewBag.Promotoras = _promotorasRepositorio.ListarPromotora();

            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Erro ao alterar o registro. Verifique os campos.";
                // Retorna o objeto financa para a view para manter os dados preenchidos
                return View(financa);
            }

            // Busca o objeto original do banco para garantir que não estamos perdendo dados
            var financaDb = _financaRepositorio.BuscarPorId(financa.Id);

            if (financaDb == null)
            {
                TempData["MensagemErro"] = "Registro não encontrado.";
                return RedirectToAction("Index");
            }

            // Atualiza os campos
            financaDb.Data = financa.Data;
            financaDb.Tipo = financa.Tipo;
            financaDb.ContaSocio = financa.ContaSocio;
            financaDb.PromotoraId = financa.PromotoraId;
            financaDb.Valor = financa.Valor;
            financaDb.Descricao = financa.Descricao;

            _financaRepositorio.Atualizar(financaDb);

            TempData["MensagemSucesso"] = "Registro alterado com sucesso!";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Apagar(int id)
        {
            try
            {
                var financa = _financaRepositorio.BuscarPorId(id);

                if(financa == null)
                {
                    TempData["MensagemEssro"] = "Registro não localizado.";
                    return RedirectToAction("Index");
                }                

                bool apagado = _financaRepositorio.Apagar(id);

                if (apagado)               
                {                  

                    TempData["MensagemSucesso"] = "Receita/Despesa excluida com sucesso!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["MensagemErro"] = "Não foi possível excluir essa Receira/Despesa.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception erro)
            {
                TempData["MensagemErro"] = "Ops, não foi possível excluir o registro de finança. Detalhes do erro: {erro.Message}";
                return RedirectToAction("Index");
            }

        }

    }

}



