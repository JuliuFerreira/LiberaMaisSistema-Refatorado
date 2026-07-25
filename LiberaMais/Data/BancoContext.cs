using Microsoft.EntityFrameworkCore;
using LiberaMais.Models;
using System.Reflection.Metadata;
using LiberaMais.Data.Map;

namespace LiberaMais.Data
{
    public class BancoContext : DbContext
    {
        public BancoContext()
        {
        }

        public BancoContext(DbContextOptions<BancoContext> options) : base(options)
        {
        }
        public DbSet<Cliente> Clientes { get; set; }

        public DbSet<Venda> Vendas { get; set; }

        public DbSet<Endereco> Enderecos { get; set; }

        public DbSet<Promotora> Promotoras { get; set; }

        public DbSet<Banco> Bancos { get; set; }

        public DbSet<UsuarioModel> Usuarios { get; set; }

        public DbSet<Financa> financas { get; set; }

        //public DbSet<Receita> receitas { get; set; }

        //public DbSet<Despesa> despesas { get; set; }

        public DbSet<PromotoraBanco> PromotoraBancos { get; set; }

        public DbSet<Util> utils { get; set; }

        public DbSet<Orgao> orgaos { get; set; }

        public DbSet<Beneficio> beneficios { get; set; }

        public DbSet<ClienteBeneficio> clienteBeneficio { get; set; }

        public DbSet<Venda> Venda { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new VendaMap());

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Cliente>().HasKey(c => c.Id);
            modelBuilder.Entity<Cliente>().ToTable("Cliente");
            modelBuilder.Entity<Endereco>().HasKey(e => e.Id);
            modelBuilder.Entity<Endereco>().ToTable("Endereco");
            //modelBuilder.Entity<Venda>().HasKey(j => j.Id);
            modelBuilder.Entity<Promotora>().HasKey(j => j.Id);
            modelBuilder.Entity<Promotora>().ToTable("Promotora");
            modelBuilder.Entity<Banco>().HasKey(j => j.Id);
            modelBuilder.Entity<Banco>().ToTable("Banco");
            modelBuilder.Entity<Financa>().HasKey(j => j.Id);
            modelBuilder.Entity<Financa>().ToTable("Financa");
            //modelBuilder.Entity<Receita>().HasKey(j => j.Id);
            //modelBuilder.Entity<Receita>().ToTable("Receita");
            //modelBuilder.Entity<Despesa>().HasKey(j => j.Id);
            //modelBuilder.Entity<Despesa>().ToTable("Despesa");
            modelBuilder.Entity<UsuarioModel>().ToTable("Usuarios");
            modelBuilder.Entity<PromotoraBanco>().HasKey(j => j.Id);
            modelBuilder.Entity<PromotoraBanco>().ToTable("PromotoraBanco");
            modelBuilder.Entity<Util>().HasKey(u => u.Id);
            modelBuilder.Entity<Util>().ToTable("Util");
            modelBuilder.Entity<Orgao>().HasKey(o => o.Id);
            modelBuilder.Entity<Orgao>().ToTable("Orgao");
            modelBuilder.Entity<Beneficio>().HasKey(b => b.Id);
            modelBuilder.Entity<Beneficio>().ToTable("Beneficio");
            modelBuilder.Entity<ClienteBeneficio>().HasKey(c => c.Id);
            modelBuilder.Entity<ClienteBeneficio>().ToTable("ClienteBeneficio");
            modelBuilder.Entity<Venda>().HasKey(v => v.Id);
            modelBuilder.Entity<Venda>().ToTable("Venda");

            // Configura relacionamento:
            // PromotoraBanco possui uma Promotora
            // e uma Promotora possui vários vínculos em PromotoraBanco
            modelBuilder.Entity<PromotoraBanco>()
            .HasOne(pb => pb.Promotora)
            .WithMany(p => p.PromotoraBancos)
            .HasForeignKey(pb => pb.PromotoraId);

            // Configura relacionamento:
            // PromotoraBanco possui um Banco
            // e um Banco possui vários vínculos em PromotoraBanco
            modelBuilder.Entity<PromotoraBanco>()
            .HasOne(pb => pb.Banco)
            .WithMany(b => b.PromotoraBancos)
            .HasForeignKey(pb => pb.BancoId);

            //modelBuilder.Entity<Venda>()
            //.Property(v => v.Status)
            //.HasConversion<int>();

            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }



        }
    }
}

