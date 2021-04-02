using Microsoft.AspNetCore.Mvc;
using OnlineHelpDesk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Authorization;
using OnlineHelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace OnlineHelpDesk.Controllers
{
    [Route("ticket")]
    public class TicketController : Controller
    {
        private HelpDeskEntities db;
        [Obsolete]
        private IHostingEnvironment hostingEnvironment;

        [Obsolete]
        public TicketController(HelpDeskEntities _db, IHostingEnvironment _hostingEnvironment)
        {
            db = _db;
            hostingEnvironment = _hostingEnvironment;
        }

        #region Send
        [Authorize(Roles = "Employee")]
        [HttpGet]
        [Route("send")]
        public IActionResult Send()
        {
            var ticketViewModel = new TicketViewModel();
            ticketViewModel.Ticket = new Ticket();
            var categories = CategoryController.GetAllCategories();
            ticketViewModel.Categories = new SelectList(categories, "CategoryId", "Name");

            var statuses = StatusController.GetAllStatuses();
            ticketViewModel.Statuses = new SelectList(statuses, "StatusId", "Name");

            var periods = PeriodController.GetAllPeriods();
            ticketViewModel.Periods = new SelectList(periods, "PeriodId", "Name");
            ViewBag.sideBar = "ticket";
            return View("Send", ticketViewModel);
        }

        [HttpPost]
        [Route("send")]
        [Obsolete]
        public IActionResult Send(TicketViewModel ticketViewModel, IFormFile[] files)
        {
            try
            {
                var userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var account = AccountController.GetAccountByUserName(userName);
                ticketViewModel.Ticket.EmployeeId = account.AccountId;

                var insert = InsertTicket(ticketViewModel.Ticket);
                var newTicket = GetTicketByEmployeeId((int)ticketViewModel.Ticket.EmployeeId);

                if (files != null && files.Length > 0)
                {
                    foreach (var file in files)
                    {
                        var fileName = DateTime.Now.ToString("MMddyyyyhhmmss") + file.FileName;
                        var path = Path.Combine(hostingEnvironment.WebRootPath, "uploads", fileName);
                        var stream = new FileStream(path, FileMode.Create);
                        file.CopyToAsync(stream);

                        //Save photo to database
                        var photo = new Photo();
                        photo.Name = fileName;
                        photo.TicketId = newTicket.TicketId;
                        PhotoController.InsertPhoto(photo);
                    }
                }

                if (insert)
                {
                    TempData["msg"] = "Done";
                }
                else
                {
                    TempData["msg"] = "Failed";
                }
            }
            catch
            {
                TempData["msg"] = "Failed";
            }

            return RedirectToAction("Send");
        }
        #endregion

        #region History
        [Authorize(Roles = "Employee")]
        [HttpGet]
        [Route("history")]
        public IActionResult History()
        {
            var userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var account = AccountController.GetAccountByUserName(userName);
            ViewBag.tickets = GetTicketsByEmployeeId(account.AccountId);
            ViewBag.sideBar = "ticket";
            return View("History");
        }
        #endregion

        #region List
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        [Route("list")]
        public IActionResult List()
        {
            ViewBag.tickets = GetAllTickets();
            ViewBag.sideBar = "ticket";
            return View("List");
        }
        #endregion

        #region Assign
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        [Route("assign")]
        public IActionResult Assign()
        {
            ViewBag.tickets = GetTicketsWithNoSupporter();
            ViewBag.sideBar = "ticket";
            return View("Assign");
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [Route("assign")]
        public IActionResult Assign(int id, int supporterId, int statusId)
        {
            var ticket = GetTicketById(id);

            if (ticket.SupporterId == 0 || ticket.SupporterId == null)
            {
                ticket.SupporterId = supporterId;
            }
            else if (ticket.StatusId != statusId)
            {
                ticket.StatusId = statusId;
            }

            UpdateTicket(ticket);

            ViewBag.tickets = GetTicketsWithNoSupporter();
            ViewBag.sideBar = "ticket";
            return RedirectToAction("List");
        }
        #endregion

        #region Assigned
        //Load tickets that are assigned to each supporter
        [Authorize(Roles = "Support")]
        [HttpGet]
        [Route("assigned")]
        public IActionResult Assigned()
        {
            var userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var account = AccountController.GetAccountByUserName(userName);
            ViewBag.tickets = GetTicketsBySupporterId(account.AccountId);
            ViewBag.sideBar = "ticket";
            return View("Assigned");
        }
        #endregion

        #region Details
        [Authorize(Roles = "Administrator, Support, Employee")]
        [HttpGet]
        [Route("details/{id}")]
        public IActionResult Details(int id)
        {
            ViewBag.ticket = GetTicketById(id);
            ViewBag.supporters = AccountController.GetAccountsByRoleId(2);
            ViewBag.statuses = StatusController.GetAllStatuses();
            ViewBag.discussions = DiscussionController.GetDiscussionsByTicketId(id);
            ViewBag.sideBar = "ticket";
            return View("Details");
        }
        #endregion

        #region SendDiscussion
        [Authorize(Roles = "Support, Employee")]
        [HttpPost]
        [Route("send_discussion")]
        public IActionResult SendDiscussion(int ticketId, string message)
        {
            var userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var account = AccountController.GetAccountByUserName(userName);
            var discussion = new Discussion();
            discussion.CreatedDate = DateTime.Now;
            discussion.Content = message;
            discussion.TicketId = ticketId;
            discussion.AccountId = account.AccountId;

            DiscussionController.InsertDiscussion(discussion);

            ViewBag.sideBar = "ticket";
            return RedirectToAction("Details", new { id = ticketId });
        }
        #endregion

        #region FillSubCategories
        [Route("FillSubCategories")]
        [HttpGet]
        public JsonResult FillSubCategories(int categoryId)
        {
            var ticketViewModel = new TicketViewModel();
            ViewBag.subCategories = SubCategoryController.GetSubCategoriesByCategoryId(categoryId);
            ticketViewModel.SubCategories = new SelectList(ViewBag.subcategories, "SubCategoryId", "Name");
            return Json(ticketViewModel);
        }
        #endregion

        #region Methods
        public static bool InsertTicket(Ticket _ticket)
        {
            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"insert into ticket(Title, Description, CreatedDate, StatusId, CategoryId, SubCategoryId, PeriodId, EmployeeId)
                                                            values(@title, @description, @createdDate, @statusId, @categoryId, @subCategoryId, @periodId, @employeeId)", connection);

                command.Parameters.AddWithValue("@title", _ticket.Title);
                command.Parameters.AddWithValue("@description", _ticket.Description);
                command.Parameters.AddWithValue("@createdDate", DateTime.Now);
                command.Parameters.AddWithValue("@statusId", _ticket.StatusId);
                command.Parameters.AddWithValue("@categoryId", _ticket.CategoryId);
                command.Parameters.AddWithValue("@subCategoryId", _ticket.SubCategoryId);
                command.Parameters.AddWithValue("@periodId", _ticket.PeriodId);
                command.Parameters.AddWithValue("@employeeId", _ticket.EmployeeId);

                var insert = command.ExecuteNonQuery();

                if (Convert.ToBoolean(insert))
                {
                    return true;
                }
            }

            return false;
        }

        public static Ticket GetTicketByEmployeeId(int employeeId)
        {
            Ticket ticket = new Ticket();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"select * from ticket where EmployeeId = @employeeId order by CreatedDate desc limit 1", connection);

                command.Parameters.AddWithValue("@employeeId", employeeId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ticket.TicketId = Convert.ToInt32(reader["TicketId"]);
                        ticket.Title = Convert.ToString(reader["Title"]);
                        ticket.Description = Convert.ToString(reader["Description"]);
                        ticket.CreatedDate = Convert.ToDateTime(reader["CreatedDate"]);
                        ticket.StatusId = Convert.ToInt32(reader["StatusId"]);
                        ticket.CategoryId = Convert.ToInt32(reader["CategoryId"]);
                        ticket.SubCategoryId = Convert.ToInt32(reader["SubCategoryId"]);
                        ticket.PeriodId = Convert.ToInt32(reader["PeriodId"]);
                        ticket.EmployeeId = Convert.ToInt32(reader["EmployeeId"]);
                    }
                }

                if (ticket != null)
                {
                    return ticket;
                }
            }

            return null;
        }

        public static List<Ticket> GetTicketsByEmployeeId(int employeeId)
        {
            List<Ticket> tickets = new List<Ticket>();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"select * from ticket where EmployeeId = @employeeId order by CreatedDate desc", connection);

                command.Parameters.AddWithValue("@employeeId", employeeId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Ticket ticket = new Ticket();

                        ticket.TicketId = Convert.ToInt32(reader["TicketId"]);
                        ticket.Title = Convert.ToString(reader["Title"]);
                        ticket.Description = Convert.ToString(reader["Description"]);
                        ticket.CreatedDate = Convert.ToDateTime(reader["CreatedDate"]);

                        ticket.StatusId = Convert.ToInt32(reader["StatusId"]);
                        var status = StatusController.GetStatusById(ticket.StatusId);
                        ticket.Status = status;

                        ticket.CategoryId = Convert.ToInt32(reader["CategoryId"]);
                        var category = CategoryController.GetCategoryById(ticket.CategoryId);
                        ticket.Category = category;

                        ticket.SubCategoryId = Convert.ToInt32(reader["SubCategoryId"]);
                        var subCategory = SubCategoryController.GetSubCategoryById(ticket.SubCategoryId);
                        ticket.SubCategory = subCategory;

                        ticket.PeriodId = Convert.ToInt32(reader["PeriodId"]);
                        var period = PeriodController.GetPeriodById(ticket.PeriodId);
                        ticket.Period = period;

                        ticket.EmployeeId = Convert.ToInt32(reader["EmployeeId"]);

                        tickets.Add(ticket);
                    }
                }

                if (tickets != null && tickets.Count > 0)
                {
                    return tickets;
                }
            }

            return tickets;
        }

        public static List<Ticket> GetTicketsBySupporterId(int supporterId)
        {
            List<Ticket> tickets = new List<Ticket>();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"select * from ticket where SupporterId = @supporterId order by CreatedDate desc", connection);

                command.Parameters.AddWithValue("@supporterId", supporterId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Ticket ticket = new Ticket();

                        ticket.TicketId = Convert.ToInt32(reader["TicketId"]);
                        ticket.Title = Convert.ToString(reader["Title"]);
                        ticket.Description = Convert.ToString(reader["Description"]);
                        ticket.CreatedDate = Convert.ToDateTime(reader["CreatedDate"]);

                        ticket.StatusId = Convert.ToInt32(reader["StatusId"]);
                        var status = StatusController.GetStatusById(ticket.StatusId);
                        ticket.Status = status;

                        ticket.CategoryId = Convert.ToInt32(reader["CategoryId"]);
                        var category = CategoryController.GetCategoryById(ticket.CategoryId);
                        ticket.Category = category;

                        ticket.SubCategoryId = Convert.ToInt32(reader["SubCategoryId"]);
                        var subCategory = SubCategoryController.GetSubCategoryById(ticket.SubCategoryId);
                        ticket.SubCategory = subCategory;

                        ticket.PeriodId = Convert.ToInt32(reader["PeriodId"]);
                        var period = PeriodController.GetPeriodById(ticket.PeriodId);
                        ticket.Period = period;

                        ticket.EmployeeId = Convert.ToInt32(reader["EmployeeId"]);
                        var employee = AccountController.GetAccountById((int)ticket.EmployeeId);
                        ticket.Employee = employee;

                        ticket.SupporterId = Convert.ToInt32(reader["SupporterId"]);

                        tickets.Add(ticket);
                    }
                }

                if (tickets != null && tickets.Count > 0)
                {
                    return tickets;
                }
            }

            return tickets;
        }

        public static List<Ticket> GetAllTickets()
        {
            List<Ticket> tickets = new List<Ticket>();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"select * from ticket order by TicketId desc", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Ticket ticket = new Ticket();

                        ticket.TicketId = Convert.ToInt32(reader["TicketId"]);
                        ticket.Title = Convert.ToString(reader["Title"]);
                        ticket.Description = Convert.ToString(reader["Description"]);
                        ticket.CreatedDate = Convert.ToDateTime(reader["CreatedDate"]);

                        ticket.StatusId = Convert.ToInt32(reader["StatusId"]);
                        var status = StatusController.GetStatusById(ticket.StatusId);
                        ticket.Status = status;

                        ticket.CategoryId = Convert.ToInt32(reader["CategoryId"]);
                        var category = CategoryController.GetCategoryById(ticket.CategoryId);
                        ticket.Category = category;

                        ticket.SubCategoryId = Convert.ToInt32(reader["SubCategoryId"]);
                        var subCategory = SubCategoryController.GetSubCategoryById(ticket.SubCategoryId);
                        ticket.SubCategory = subCategory;

                        ticket.PeriodId = Convert.ToInt32(reader["PeriodId"]);
                        var period = PeriodController.GetPeriodById(ticket.PeriodId);
                        ticket.Period = period;

                        ticket.EmployeeId = Convert.ToInt32(reader["EmployeeId"]);
                        ticket.Employee = AccountController.GetAccountById((int)ticket.EmployeeId);

                        var supporterId = reader["SupporterId"];

                        if (!(supporterId is DBNull))
                        {
                            ticket.SupporterId = Convert.ToInt32(supporterId);
                            ticket.Supporter = AccountController.GetAccountById((int)ticket.SupporterId);
                        }
                        else
                        {
                            ticket.SupporterId = null;
                        }

                        tickets.Add(ticket);
                    }
                }

                if (tickets != null && tickets.Count > 0)
                {
                    return tickets;
                }
            }

            return tickets;
        }

        public static List<Ticket> GetFinishedTickets()
        {
            List<Ticket> tickets = new List<Ticket>();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"select * from ticket where StatusId = 3", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Ticket ticket = new Ticket();

                        ticket.TicketId = Convert.ToInt32(reader["TicketId"]);
                        ticket.Title = Convert.ToString(reader["Title"]);
                        ticket.Description = Convert.ToString(reader["Description"]);
                        ticket.CreatedDate = Convert.ToDateTime(reader["CreatedDate"]);

                        ticket.StatusId = Convert.ToInt32(reader["StatusId"]);
                        var status = StatusController.GetStatusById(ticket.StatusId);
                        ticket.Status = status;

                        ticket.CategoryId = Convert.ToInt32(reader["CategoryId"]);
                        var category = CategoryController.GetCategoryById(ticket.CategoryId);
                        ticket.Category = category;

                        ticket.SubCategoryId = Convert.ToInt32(reader["SubCategoryId"]);
                        var subCategory = SubCategoryController.GetSubCategoryById(ticket.SubCategoryId);
                        ticket.SubCategory = subCategory;

                        ticket.PeriodId = Convert.ToInt32(reader["PeriodId"]);
                        var period = PeriodController.GetPeriodById(ticket.PeriodId);
                        ticket.Period = period;

                        ticket.EmployeeId = Convert.ToInt32(reader["EmployeeId"]);
                        ticket.Employee = AccountController.GetAccountById((int)ticket.EmployeeId);

                        var supporterId = reader["SupporterId"];

                        if (!(supporterId is DBNull))
                        {
                            ticket.SupporterId = Convert.ToInt32(supporterId);
                            ticket.Supporter = AccountController.GetAccountById((int)ticket.SupporterId);
                        }
                        else
                        {
                            ticket.SupporterId = null;
                        }

                        tickets.Add(ticket);
                    }
                }

                if (tickets != null && tickets.Count > 0)
                {
                    return tickets;
                }
            }

            return tickets;
        }

        public static List<Ticket> GetRedTickets()
        {
            List<Ticket> tickets = new List<Ticket>();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"select * from ticket where StatusId in (6, 7)", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Ticket ticket = new Ticket();

                        ticket.TicketId = Convert.ToInt32(reader["TicketId"]);
                        ticket.Title = Convert.ToString(reader["Title"]);
                        ticket.Description = Convert.ToString(reader["Description"]);
                        ticket.CreatedDate = Convert.ToDateTime(reader["CreatedDate"]);

                        ticket.StatusId = Convert.ToInt32(reader["StatusId"]);
                        var status = StatusController.GetStatusById(ticket.StatusId);
                        ticket.Status = status;

                        ticket.CategoryId = Convert.ToInt32(reader["CategoryId"]);
                        var category = CategoryController.GetCategoryById(ticket.CategoryId);
                        ticket.Category = category;

                        ticket.SubCategoryId = Convert.ToInt32(reader["SubCategoryId"]);
                        var subCategory = SubCategoryController.GetSubCategoryById(ticket.SubCategoryId);
                        ticket.SubCategory = subCategory;

                        ticket.PeriodId = Convert.ToInt32(reader["PeriodId"]);
                        var period = PeriodController.GetPeriodById(ticket.PeriodId);
                        ticket.Period = period;

                        ticket.EmployeeId = Convert.ToInt32(reader["EmployeeId"]);
                        ticket.Employee = AccountController.GetAccountById((int)ticket.EmployeeId);

                        var supporterId = reader["SupporterId"];

                        if (!(supporterId is DBNull))
                        {
                            ticket.SupporterId = Convert.ToInt32(supporterId);
                            ticket.Supporter = AccountController.GetAccountById((int)ticket.SupporterId);
                        }
                        else
                        {
                            ticket.SupporterId = null;
                        }

                        tickets.Add(ticket);
                    }
                }

                if (tickets != null && tickets.Count > 0)
                {
                    return tickets;
                }
            }

            return tickets;
        }

        public static List<Ticket> GetTicketsWithNoSupporter()
        {
            List<Ticket> tickets = new List<Ticket>();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"select * from ticket where SupporterId is null order by TicketId desc", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Ticket ticket = new Ticket();

                        ticket.TicketId = Convert.ToInt32(reader["TicketId"]);
                        ticket.Title = Convert.ToString(reader["Title"]);
                        ticket.Description = Convert.ToString(reader["Description"]);
                        ticket.CreatedDate = Convert.ToDateTime(reader["CreatedDate"]);

                        ticket.StatusId = Convert.ToInt32(reader["StatusId"]);
                        var status = StatusController.GetStatusById(ticket.StatusId);
                        ticket.Status = status;

                        ticket.CategoryId = Convert.ToInt32(reader["CategoryId"]);
                        var category = CategoryController.GetCategoryById(ticket.CategoryId);
                        ticket.Category = category;

                        ticket.SubCategoryId = Convert.ToInt32(reader["SubCategoryId"]);
                        var subCategory = SubCategoryController.GetSubCategoryById(ticket.SubCategoryId);
                        ticket.SubCategory = subCategory;

                        ticket.PeriodId = Convert.ToInt32(reader["PeriodId"]);
                        var period = PeriodController.GetPeriodById(ticket.PeriodId);
                        ticket.Period = period;

                        ticket.EmployeeId = Convert.ToInt32(reader["EmployeeId"]);
                        ticket.Employee = AccountController.GetAccountById((int)ticket.EmployeeId);

                        var supporterId = reader["SupporterId"];

                        if (!(supporterId is DBNull))
                        {
                            ticket.SupporterId = Convert.ToInt32(supporterId);
                            ticket.Supporter = AccountController.GetAccountById((int)ticket.SupporterId);
                        }
                        else
                        {
                            ticket.SupporterId = null;
                        }

                        tickets.Add(ticket);
                    }
                }

                if (tickets != null && tickets.Count > 0)
                {
                    return tickets;
                }
            }

            return tickets;
        }

        public static Ticket GetTicketById(int ticketId)
        {
            Ticket ticket = new Ticket();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"select * from ticket where TicketId = @ticketId", connection);

                command.Parameters.AddWithValue("@ticketId", ticketId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ticket.TicketId = Convert.ToInt32(reader["TicketId"]);
                        ticket.Title = Convert.ToString(reader["Title"]);
                        ticket.Description = Convert.ToString(reader["Description"]);
                        ticket.CreatedDate = Convert.ToDateTime(reader["CreatedDate"]);

                        ticket.StatusId = Convert.ToInt32(reader["StatusId"]);
                        ticket.Status = StatusController.GetStatusById(ticket.StatusId);

                        ticket.CategoryId = Convert.ToInt32(reader["CategoryId"]);
                        ticket.Category = CategoryController.GetCategoryById(ticket.CategoryId);

                        ticket.SubCategoryId = Convert.ToInt32(reader["SubCategoryId"]);
                        ticket.SubCategory = SubCategoryController.GetSubCategoryById(ticket.SubCategoryId);

                        ticket.PeriodId = Convert.ToInt32(reader["PeriodId"]);
                        ticket.Period = PeriodController.GetPeriodById(ticket.PeriodId);

                        ticket.EmployeeId = Convert.ToInt32(reader["EmployeeId"]);
                        ticket.Employee = AccountController.GetAccountById((int)ticket.EmployeeId);

                        var supporterId = reader["SupporterId"];

                        if (!(supporterId is DBNull))
                        {
                            ticket.SupporterId = Convert.ToInt32(reader["SupporterId"]);
                            ticket.Supporter = AccountController.GetAccountById((int)ticket.SupporterId);
                        }
                        else
                        {
                            ticket.SupporterId = null;
                        }

                        ticket.Photos = PhotoController.GetPhotosByTicketId(ticketId);
                    }

                    if (ticket != null)
                    {
                        return ticket;
                    }
                }

                return null;
            }
        }

        public static bool UpdateTicket(Ticket _ticket)
        {
            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"update ticket
                                                            set Title = @title,
	                                                            Description = @description,
                                                                StatusId = @statusId,
                                                                CategoryId = @categoryId,
                                                                SubCategoryId = @subCategoryId,
                                                                PeriodId = @periodId,
                                                                EmployeeId = @employeeId,
                                                                SupporterId = @supporterId
                                                            where TicketId = @ticketId", connection);

                command.Parameters.AddWithValue("@title", _ticket.Title);
                command.Parameters.AddWithValue("@description", _ticket.Description);
                command.Parameters.AddWithValue("@statusId", _ticket.StatusId);
                command.Parameters.AddWithValue("@categoryId", _ticket.CategoryId);
                command.Parameters.AddWithValue("@subCategoryId", _ticket.SubCategoryId);
                command.Parameters.AddWithValue("@periodId", _ticket.PeriodId);
                command.Parameters.AddWithValue("@employeeId", _ticket.EmployeeId);
                command.Parameters.AddWithValue("@supporterId", _ticket.SupporterId);

                command.Parameters.AddWithValue("@ticketId", _ticket.TicketId);

                var update = command.ExecuteNonQuery();

                if (Convert.ToBoolean(update))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
