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

namespace OnlineHelpDesk.Controllers
{
    [Authorize(Roles = "Administrator")]
    [Route("status")]
    public class StatusController : Controller
    {
        private HelpDeskEntities db;

        public StatusController(HelpDeskEntities _db)
        {
            db = _db;
        }

        #region Index
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        [Route("index")]
        [Route("")]
        public IActionResult Index()
        {
            ViewBag.statuses = GetAllStatuses();
            ViewBag.sideBar = "status";
            return View("Index");
        }
        #endregion

        #region Add
        [HttpGet]
        [Route("add")]
        public IActionResult Add()
        {
            ViewBag.sideBar = "status";
            return View("Add", new Status());
        }

        [HttpPost]
        [Route("add")]
        public IActionResult Add(Status status)
        {
            try
            {
                var insert = InsertStatus(status);
                return RedirectToAction("Index");
            }
            catch
            {
                ViewBag.msg = "Failed";
                return View("Add", new Status());
            }
        }
        #endregion

        #region Delete
        [HttpGet]
        [Route("delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var delete = DeleteStatus(id);

                if (delete)
                {
                    ViewBag.sideBar = "status";
                    ViewBag.msg = "Done";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.msg = "Failed";
                    ViewBag.statuses = GetAllStatuses();
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                ViewBag.msg = "Failed";
                ViewBag.statuses = GetAllStatuses();
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Edit
        [HttpGet]
        [Route("edit/{id}")]
        public IActionResult Edit(int id)
        {
            var status = GetStatusById(id);
            return View("Edit", status);
        }

        [HttpPost]
        [Route("edit/{id}")]
        public IActionResult Edit(int id, Status status)
        {
            try
            {
                var update = UpdateStatus(status);

                if (update)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.msg = "Failed";
                    status = GetStatusById(id);
                    return View("Edit", status);
                }
            }
            catch
            {
                ViewBag.msg = "Failed";
                status = GetStatusById(id);
                return View("Edit", status);
            }
        }

        #endregion

        #region Methods
        public static List<Status> GetAllStatuses()
        {
            List<Status> statuses = new List<Status>();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("select * from status where Display = 1", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var status = new Status();

                        status.StatusId = Convert.ToInt32(reader["StatusId"]);
                        status.Name = Convert.ToString(reader["Name"]);
                        status.Color = Convert.ToString(reader["Color"]);
                        status.Display = Convert.ToBoolean(reader["Display"]);

                        statuses.Add(status);
                    }

                    if (statuses.Count > 0)
                    {
                        return statuses;
                    }
                }
            }

            return statuses;
        }

        public static Status GetStatusById(int statusId)
        {
            Status status = new Status();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("select * from status where StatusId = @statusId", connection);
                command.Parameters.AddWithValue("@statusId", statusId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        status.StatusId = Convert.ToInt32(reader["StatusId"]);
                        status.Name = Convert.ToString(reader["Name"]);
                        status.Color = Convert.ToString(reader["Color"]);
                        status.Display = Convert.ToBoolean(reader["Display"]);
                    }
                }

                if (status != null)
                {
                    return status;
                }
            }

            return null;
        }

        public static bool InsertStatus(Status _status)
        {
            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"insert into status(Name, Color, Display)
                                                            values(@name, @color, @display)", connection);

                command.Parameters.AddWithValue("@name", _status.Name);
                command.Parameters.AddWithValue("@color", _status.Color);
                command.Parameters.AddWithValue("@display", _status.Display);

                var insert = command.ExecuteNonQuery();

                if (Convert.ToBoolean(insert))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool UpdateStatus(Status _status)
        {
            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"update status
                                                            set Name = @name,
                                                                Color = @color,
                                                                Display = @display
                                                          where StatusId = @statusId", connection);

                command.Parameters.AddWithValue("@name", _status.Name);
                command.Parameters.AddWithValue("@color", _status.Color);
                command.Parameters.AddWithValue("@display", _status.Display);

                command.Parameters.AddWithValue("@statusId", _status.StatusId);

                var update = command.ExecuteNonQuery();

                if (Convert.ToBoolean(update))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool DeleteStatus(int id)
        {
            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("update status set Display = 0 where StatusId = @statusId", connection);
                command.Parameters.AddWithValue("@statusId", id.ToString());

                var delete = command.ExecuteNonQuery();

                if (Convert.ToBoolean(delete))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
