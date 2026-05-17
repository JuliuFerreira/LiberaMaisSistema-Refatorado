using LiberaMais.Filters;
using LiberaMais.Helper;
using LiberaMais.Models;
using LiberaMais.Models.Enums;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace LiberaMais.Controllers
{
    [PaginaParaUsuarioLogado]
    public class VendaController : Controller
    {
        private readonly IVendaRepositorio _vendaRepositorio;
        private readonly ISessao _sessao;
        private readonly IUsuarioRepositorio _usuarioRepositorio;

        public VendaController(IVendaRepositorio vendaRepositorio, ISessao sessao, IUsuarioRepositorio usuarioRepositorio)
        {
            _vendaRepositorio = vendaRepositorio;
            _sessao = sessao;
            _usuarioRepositorio = usuarioRepositorio;
        }

        public IActionResult Venda(string searchString, DateTime? startDate, DateTime? endDate)
        {
            UsuarioModel usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            List<Venda> vendas;

            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                vendas = _vendaRepositorio.ListarTodasVendas(); // Administrador vê todas as vendas
            }
            else
            {
                vendas = _vendaRepositorio.ListarVenda(usuarioLogado.Id); // Usuário comum vê apenas suas vendas
            }

            // Aplicar filtros
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower(); // Converter para minúsculas
                vendas = vendas.Where(s => s.UsuarioNome.ToLower().Contains(searchString) ||
                                           s.Cpf.ToLower().Contains(searchString) ||
                                           s.Nome.ToLower().Contains(searchString) ||
                                           s.OperacaoDescription.ToString().ToLower().Contains(searchString) ||
                                           s.BancoDescription.ToString().ToLower().Contains(searchString) ||
                                           s.ComissaoStatusDescription.ToString().ToLower().Contains(searchString) ||
                                           s.StatusDescription.ToString().ToLower().Contains(searchString)).ToList();

            }

            if (startDate.HasValue)
            {
                vendas = vendas.Where(v => v.Data >= startDate).ToList();
            }

            if (endDate.HasValue)
            {
                // Considerar a data até o final do dia, adicionando 1 dia e subtraindo 1 segundo
                DateTime endDateEndOfDay = endDate.Value.Date.AddDays(1).AddSeconds(-1);
                vendas = vendas.Where(v => v.Data <= endDateEndOfDay).ToList();
            }

            ViewBag.SearchString = searchString;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            ViewBag.TotalVendas = vendas.Count;

            return View(vendas);
        }

        public IActionResult RelatorioVendas(DateTime startDate, DateTime endDate, string searchString)
        {
            var usuarios = _usuarioRepositorio.ListarTodosUsuarios();
            ViewBag.Usuarios = usuarios;

            var vendas = _vendaRepositorio.ListarVendasApenasPagos();          


            return View(vendas);
        }

        [HttpPost]
        public IActionResult RelatorioVendasFiltrado(DateTime startDate, DateTime endDate, List<int> selectedUsers, string searchString)
        {
            var usuarios = _usuarioRepositorio.ListarTodosUsuarios();
            ViewBag.Usuarios = usuarios;

            var vendas = _vendaRepositorio.ListarVendasPorPeriodoEUsuario(startDate, endDate, selectedUsers, searchString);

            return View("RelatorioVendas", vendas);
        }


        public IActionResult Digitado()
        {
            var vendas = _vendaRepositorio.ListarTodasVendas()
                .Where(v => (int)v.Status == (int)StatusEnum.DIGITADO)
                .ToList();
            return PartialView("Venda", vendas);
        }

        public IActionResult Assinado()
        {
            var vedas = _vendaRepositorio.ListarTodasVendas()
                .Where(v => (int)v.Status == (int)StatusEnum.ASSINADO)
                .ToList();
            return PartialView("Venda", vedas);
        }

        public IActionResult Pago()
        {
            var vendas = _vendaRepositorio.ListarTodasVendas()
                .Where(v => (int)v.Status == (int)StatusEnum.PAGO)
                .ToList();
            return PartialView("Venda", vendas);
        }

        public IActionResult Cancelado()
        {
            var vendas = _vendaRepositorio.ListarTodasVendas()
                .Where(v => (int)v.Status == (int)StatusEnum.CANCELADO)
                .ToList();
            return PartialView("Venda", vendas);
        }



        public IActionResult CadastroVenda(string cpf, string nome)
        {
            ViewBag.Cpf = cpf;
            ViewBag.Nome = nome;
            return View();
        }

        [HttpPost]
        public IActionResult CadastroVenda(Venda venda)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    UsuarioModel usuarioLogado = _sessao.BuscarSessaoDoUsuario();
                    venda.UsuarioId = usuarioLogado.Id;
                    venda.UsuarioNome = usuarioLogado.Nome;
                    venda = _vendaRepositorio.Adicionar(venda);                  

                    return Json(new { success = true }); // <- chama o Javascript para perguntar se deseja digitar outra venda antes de finalziar.
                }
                else
                {
                    TempData["MensagemErro"] = "Ops, não foi possível cadastrar essa venda!";
                    return Json(new { success = false, message = "Ops, não foi possível cadastrar essa venda!" });
                }
            }
            catch (System.Exception erro)
            {
                TempData["MensagemErro"] = $"Ops, não foi possível cadastrar essa venda! Detalhe do erro: {erro.Message}";
                return Json(new { success = false, message = $"Ops, não foi possível cadastrar essa venda! Detalhe do erro: {erro.Message}" });
            }
        }



        public IActionResult DetalheVenda(long id)
        {
            var venda = _vendaRepositorio.BuscarVendaPorId(id);
            return View(venda);
        }

        public IActionResult EditarVenda(long id)
        {
            var venda = _vendaRepositorio.BuscarVendaPorId(id);
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();

            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            return View(venda);
        }


        [HttpPost]
        public IActionResult EditarVenda(Venda venda)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Recupere a venda original do banco de dados
                    var vendaOriginal = _vendaRepositorio.BuscarVendaPorId(venda.Id);

                    if (vendaOriginal == null)
                    {
                        TempData["MensagemErro"] = "Venda não encontrada!";
                        return RedirectToAction("Venda");
                    }

                    // Mantenha o UsuarioId e UsuarioNome originais
                    venda.UsuarioId = vendaOriginal.UsuarioId;
                    venda.UsuarioNome = vendaOriginal.UsuarioNome;

                    // Atualize os campos da venda original com os novos valores, exceto UsuarioId e UsuarioNome
                    vendaOriginal.Data = venda.Data;
                    vendaOriginal.Orgao = venda.Orgao;
                    vendaOriginal.Beneficio = venda.Beneficio;
                    vendaOriginal.Cpf = venda.Cpf;
                    vendaOriginal.Nome = venda.Nome.ToUpper();
                    vendaOriginal.Operacao = venda.Operacao;
                    vendaOriginal.Banco = venda.Banco;
                    vendaOriginal.Promotora = venda.Promotora;
                    vendaOriginal.Parcela = venda.Parcela;
                    vendaOriginal.Status = venda.Status;
                    vendaOriginal.DataPagamento = venda.DataPagamento;
                    vendaOriginal.Observacoes = venda.Observacoes;
                    // Salve as alterações no banco de dados
                    _vendaRepositorio.Atualizar(vendaOriginal);

                    return Json(new { success = true, message = "Venda alterada com sucesso!" });
                }
                else
                {
                    return Json(new { success = false, message = "Ops, não foi possível alterar essa venda!" });
                }
            }
            catch (System.Exception erro)
            {
                return Json(new { success = false, message = $"Ops, não foi possível alterar a venda! Detalhe do erro: {erro.Message}" });
            }
        }

        public IActionResult ExcluirVenda(int id)
        {
            var venda = _vendaRepositorio.BuscarVendaPorId(id);
            return View(venda);
        }

        [HttpPost]
        public IActionResult Apagar(int id)
        {
            try
            {
                bool apagado = _vendaRepositorio.Apagar(id);

                if (apagado)
                {
                    return Json(new { success = true, message = "Venda excluída com sucesso!" });
                }
                else
                {
                    return Json(new { success = false, message = "Ops, não foi possível excluir a venda." });
                }
            }
            catch (Exception erro)
            {
                return Json(new { success = false, message = $"Ops, não foi possível excluir a venda. Detalhes do erro: {erro.Message}" });
            }
        }


        public IActionResult EditarComissao(long id)
        {
            var venda = _vendaRepositorio.BuscarVendaPorId(id);
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();

            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            return View(venda);
        }


        [HttpPost]
        public IActionResult EditarComissao(Venda venda)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    // Recupere a venda original do banco de dados
                    var vendaOriginal = _vendaRepositorio.BuscarVendaPorId(venda.Id);

                    if (vendaOriginal == null)
                    {
                        TempData["MensagemErro"] = "Venda não encontrada!";
                        return RedirectToAction("Venda");
                    }

                    // Mantenha o UsuarioId e UsuarioNome originais
                    venda.UsuarioId = vendaOriginal.UsuarioId;
                    venda.UsuarioNome = vendaOriginal.UsuarioNome;

                    // Atualize os campos da venda original com os novos valores, exceto UsuarioId e UsuarioNome
                    vendaOriginal.Data = venda.Data;
                    vendaOriginal.Orgao = venda.Orgao;
                    vendaOriginal.Beneficio = venda.Beneficio;
                    vendaOriginal.Cpf = venda.Cpf;
                    vendaOriginal.Nome = venda.Nome.ToUpper();
                    vendaOriginal.Operacao = venda.Operacao;
                    vendaOriginal.Banco = venda.Banco;
                    vendaOriginal.Promotora = venda.Promotora;
                    vendaOriginal.Parcela = venda.Parcela;
                    vendaOriginal.Status = venda.Status;
                    vendaOriginal.DataPagamento = venda.DataPagamento;
                    vendaOriginal.Observacoes = venda.Observacoes;
                    vendaOriginal.ValorComissao = venda.ValorComissao;
                    vendaOriginal.ComissaoStatus = venda.ComissaoStatus;
                    vendaOriginal.DataComissao = venda.DataComissao;
                    vendaOriginal.Observacao2 = venda.Observacao2;
                    // Salve as alterações no banco de dados
                    _vendaRepositorio.Atualizar(vendaOriginal);

                    return Json(new { success = true, message = "Venda alterada com sucesso!" });
                }
                else
                {
                    return Json(new { success = false, message = "Ops, não foi possível alterar essa venda!" });
                }
            }
            catch (System.Exception erro)
            {
                return Json(new { success = false, message = $"Ops, não foi possível alterar a venda! Detalhe do erro: {erro.Message}" });
            }

            return View();
        }


    }
}
