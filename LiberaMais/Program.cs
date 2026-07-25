using LiberaMais.Data;
using LiberaMais.Helper;
using LiberaMais.Repositorio;
using LiberaMais.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic; // Adicionado para a List de culturas
using System.Globalization;

namespace LiberaMais
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Conexão com a string
            var connectionString = builder.Configuration.GetConnectionString("DataBase");

            builder.Services.AddDbContext<BancoContext>(options =>
            {
                options.UseSqlServer(connectionString,
                sqlOptions => sqlOptions.EnableRetryOnFailure());
            });

            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddTransient<IClienteRepositorio, ClienteRepositorio>();
            builder.Services.AddTransient<IEnderecoRepositorio, EnderecoRepositorio>();
            builder.Services.AddTransient<IVendaRepositorio, VendaRepositorio>();
            builder.Services.AddTransient<IPromotorasRepositorio, PromotorasRepositorio>();
            builder.Services.AddTransient<IBancosRepositorio, BancosRepositorio>();
            builder.Services.AddTransient<IUsuarioRepositorio, UsuarioRepositorio>();
            builder.Services.AddTransient<IFinancaRepositorio, FinancaRepositorio>();
            //builder.Services.AddTransient<IReceitaRepositorio, ReceitaRepositorio>();
            //builder.Services.AddTransient<IDespesaRepositorio, DespesaRepositorio>();
            builder.Services.AddTransient<IPromotoraBancoRepositorio, PromotoraBancoRepositorio>();
            builder.Services.AddTransient<ISessao, Sessao>();
            builder.Services.AddTransient<IEmail, Email>();
            builder.Services.AddTransient<IUtilRepositorio, UtilRepositorio>();
            builder.Services.AddTransient<IOrgaoRepositorio, OrgaoRepositorio>();
            builder.Services.AddTransient<IBeneficioRepositorio, BeneficioRepositorio>();
            builder.Services.AddTransient<IClienteBeneficioRepositorio, ClienteBeneficioRepositorio>();
            builder.Services.AddScoped<PermissaoService>();

            builder.Services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromHours(2);
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Configuração de Cultura pt-BR (Coloque ANTES do UseRouting)
            var supportedCultures = new[] { "pt-BR" };
            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            app.UseRequestLocalization(localizationOptions);

            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=login}/{action=Index}/{id?}");

            app.Run();
        }
    }
}