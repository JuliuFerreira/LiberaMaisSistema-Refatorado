using LiberaMais.Data;
using LiberaMais.Helper;
using LiberaMais.Models;
using LiberaMais.Repositorio;
using Microsoft.EntityFrameworkCore;

namespace TestProject
{
    [TestClass]
    public sealed class Test1
    {
        private BancoContext _context;
        private OrgaoRepositorio _orgaoRepositorio;
        private const string ConnectionString = "Data Source=DESKTOP-1UA88TQ\\SQLEXPRESS;Initial Catalog=LiberaMais;Integrated Security=True;TrustServerCertificate=True;";

        [TestInitialize]
        public void Setup()
        {
            // 1. Configura as opções apontando para o SQL Server
            var options = new DbContextOptionsBuilder<BancoContext>()
                .UseSqlServer(ConnectionString)
                .Options;

            _context = new BancoContext(options);

            // 2. Garante que o banco de testes existe e está atualizado com as migrations
        }

        [TestCleanup]
        public void TearDown()
        {
            // Libera o contexto e apaga o banco para não deixar lixo local
            _context?.Dispose();
        }

        [TestMethod]
        public void AtualizarOrgao()
        {
            var novoOrgao = new Orgao()
            {
                Nome = "INSS"
            };

            _orgaoRepositorio.Adicionar(novoOrgao);

            novoOrgao.Nome = "SIAPE";

            _orgaoRepositorio.Atualizar(novoOrgao);

            var orgaoAtualizado =
                _orgaoRepositorio.BuscarPorId(novoOrgao.Id);

            Assert.AreEqual(
                "SIAPE",
                orgaoAtualizado.Nome);
        }
    }
}
