using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineHelpDesk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineHelpDesk.Controllers
{
    [Authorize(Roles = "Administrator,Support,Employee")]
    [Route("Dashboard")]
    public class DashboardController : Controller
    {
        private HelpDeskEntities db;

        public DashboardController(HelpDeskEntities _db)
        {
            this.db = _db;
        }

        [Route("index")]
        [Route("")]
        public IActionResult Index()
        {
            var userName = User.FindFirst(ClaimTypes.NameIdentifier);
            var account = AccountController.GetAccountByUserName(userName.Value);
            ViewBag.tickets = TicketController.GetTicketsByEmployeeId(account.AccountId);
            ViewBag.totalTickets = TicketController.GetAllTickets();
            ViewBag.totalUsers = AccountController.GetAllAccounts();
            ViewBag.finishedTickets = TicketController.GetFinishedTickets();
            ViewBag.redTickets = TicketController.GetRedTickets();
            ViewBag.sideBar = "ticket";
            return View();
        }
    }
}
