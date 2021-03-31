using Microsoft.AspNetCore.Mvc;
using OnlineHelpDesk.Models;
using OnlineHelpDesk.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineHelpDesk.Controllers
{
    [Route("login")]
    public class LoginController : Controller
    {
        private HelpDeskEntities db;

        public LoginController(HelpDeskEntities _db)
        {
            this.db = _db;
        }

        [Route("index")]
        [Route("")]
        [Route("~/")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("SignOut")]
        public IActionResult SignOut()
        {
            SecurityManager securityManager = new SecurityManager();
            securityManager.SignOut(HttpContext);
            return RedirectToAction("Index");
        }

        [Route("AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }

        [HttpPost]
        [Route("process")]
        public IActionResult Process(string email, string password)
        {
            var account = AccountController.CheckAccount(email, password);

            if (account != null)
            {
                SecurityManager securityManager = new SecurityManager();
                securityManager.SignIn(HttpContext, account);
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                ViewBag.error = "Invalid";
                return View("Index");
            }
        }
    }
}
