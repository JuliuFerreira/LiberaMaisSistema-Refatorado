using Microsoft.AspNetCore.Mvc;

namespace LiberaMais.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View("Login");
        }

        public IActionResult RecoverSenha()
        {
            return View("RecoverSenha");
        }
    }
}

