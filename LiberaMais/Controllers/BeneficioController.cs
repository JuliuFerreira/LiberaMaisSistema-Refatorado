using LiberaMais.Filters;
using LiberaMais.Models;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;

namespace LiberaMais.Controllers
{
    [PaginaRestritaSomenteAdmin]
    public class BeneficioController : Controller
    {
        private readonly IBeneficioRepositorio _BeneficioRepositorio;
        private readonly IOrgaoRepositorio _OrgaoRepositorio;

        public BeneficioController(IBeneficioRepositorio beneficioRepositorio, IOrgaoRepositorio orgaoRepositorio)
        {
            _BeneficioRepositorio = beneficioRepositorio;
            _OrgaoRepositorio = orgaoRepositorio;
        }

        public IActionResult Index()
        {

            var beneficio = _BeneficioRepositorio.ListarTodos();

            return View(beneficio);
        }

        public IActionResult Criar()
        {
            ViewBag.orgao = _OrgaoRepositorio.ListarTodos();
            return View();
        }

        [HttpPost]
        public IActionResult Criar(Beneficio beneficio, int orgaoId)
        {
            // Carrega a lista de órgãos para a ViewBag caso precise retornar à View com erro
            ViewBag.orgao = _OrgaoRepositorio.ListarTodos();

            // 1. Garante que o OrgaoId recebido seja atribuído ao model antes das validações
            beneficio.OrgaoId = orgaoId;

            // 2. Validação de Duplicidade: Verifica se já existe um registro idêntico com os 3 atributos
            // Buscamos se há algum benefício cadastrado que possua o mesmo Órgão, Código e Descrição
            var todosBeneficios = _BeneficioRepositorio.ListarTodos(); // Ou o método equivalente do seu repositório

            bool beneficioDuplicado = todosBeneficios.Any(b =>
                b.OrgaoId == beneficio.OrgaoId &&
                b.Codigo == beneficio.Codigo &&
                b.Descricao.ToUpper() == beneficio.Descricao.ToUpper()
            );

            if (beneficioDuplicado)
            {
                TempData["MensagemErro"] = "Já existe um benefício cadastrado com este Órgão, Código e Descrição.";
                return View(beneficio);
            }

            // 3. Validação do ModelState antes de salvar
            if (ModelState.IsValid)
            {
                try
                {
                    // Formata a descrição para maiúsculo
                    beneficio.Descricao = beneficio.Descricao.ToUpper();

                    _BeneficioRepositorio.Adicionar(beneficio);

                    TempData["MensagemSucesso"] = "Benefício adicionado com sucesso.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["MensagemErro"] = $"Não foi possível adicionar o benefício. Erro: {ex.Message}";
                }
            }
            else
            {
                TempData["MensagemErro"] = "Não foi possível adicionar o benefício. Verifique os dados informados.";
            }

            return View(beneficio);
        }

        public IActionResult Editar (int id)
        {
            ViewBag.orgao = _OrgaoRepositorio.ListarTodos();
            var beneficio = _BeneficioRepositorio.BuscarPorId(id);

            if(beneficio == null)
            {
                TempData["MensagemErro"] = "Beneficio não encontrado.";
                return RedirectToAction("Index");
            }

            return View(beneficio);
        }

        [HttpPost]
        public IActionResult Editar(Beneficio beneficio)
        {
            // Carrega a lista de órgãos para a ViewBag caso precise retornar à View
            ViewBag.orgao = _OrgaoRepositorio.ListarTodos();

            // 1. Busca o registro original diretamente do banco de dados
            var DbBeneficio = _BeneficioRepositorio.BuscarPorId(beneficio.Id);

            if (DbBeneficio == null)
            {
                TempData["MensagemErro"] = "Benefício não encontrado.";
                return RedirectToAction("Index");
            }

            // 2. Validação de Duplicidade
            var todosBeneficios = _BeneficioRepositorio.ListarTodos();

            bool beneficioDuplicado = todosBeneficios.Any(b =>
                b.Id != beneficio.Id &&
                b.OrgaoId == beneficio.OrgaoId &&
                b.Codigo == beneficio.Codigo &&
                b.Descricao.ToUpper() == beneficio.Descricao.ToUpper()
            );

            if (beneficioDuplicado)
            {
                TempData["MensagemErro"] = "Já existe outro benefício cadastrado com este Órgão, Código e Descrição.";

                // CORREÇÃO AQUI: Recarrega o objeto do Órgão para que a View consiga exibir o Nome na tela
                beneficio.Orgaos = _OrgaoRepositorio.BuscarPorId(beneficio.OrgaoId);

                return View(beneficio);
            }

            // 3. Validação do ModelState
            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Não é possível editar o benefício. Verifique os dados.";

                // Garante o Órgão carregado também neste erro do ModelState
                beneficio.Orgaos = _OrgaoRepositorio.BuscarPorId(beneficio.OrgaoId);

                return View(beneficio);
            }

            try
            {
                // 4. Atualiza apenas os campos editáveis
                DbBeneficio.Codigo = beneficio.Codigo;
                DbBeneficio.Descricao = beneficio.Descricao.ToUpper();

                _BeneficioRepositorio.Atualizar(DbBeneficio);

                TempData["MensagemSucesso"] = "Benefício atualizado com sucesso.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = $"Erro ao atualizar o benefício: {ex.Message}";

                // Garante o Órgão carregado se der alguma exceção no banco
                beneficio.Orgaos = _OrgaoRepositorio.BuscarPorId(beneficio.OrgaoId);

                return View(beneficio);
            }
        }

        public IActionResult Deletar(int id)
        {
            ViewBag.orgao = _OrgaoRepositorio.ListarTodos();

            var beneficio = _BeneficioRepositorio.BuscarPorId(id);

            if (beneficio == null)
            {
                TempData["MensagemErro"] = "Beneficio não encontrado.";
                return RedirectToAction("Index");
            }

            return View(beneficio);

        }

        [HttpPost]
        public IActionResult Apagar(int id)
        {
            try
            {
                var apagado = _BeneficioRepositorio.Apagar(id);

                if (apagado)
                {
                    TempData["MensagemSucesso"] = "Beneficio excluido com sucesso.";
                    return RedirectToAction("Index");
                }

                else
                {
                    TempData["MensagemErro"] = "Beneficio não excluido.";
                    return RedirectToAction("Index");

                }
            }

            catch (Exception)
            {
                TempData["MensagemErro"] = "Erro ao excluir o beneficio, possivelmente exsite Cliente(s) cadastrados.";                
            }

            return RedirectToAction("Index");
        }
    }
}
