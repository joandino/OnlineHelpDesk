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
    [Route("period")]
    public class PeriodController : Controller
    {
        private HelpDeskEntities db;

        public PeriodController(HelpDeskEntities _db)
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
            ViewBag.periods = GetAllPeriods();
            ViewBag.sideBar = "period";
            return View("Index");
        }
        #endregion

        #region Add
        [HttpGet]
        [Route("add")]
        public IActionResult Add()
        {
            ViewBag.sideBar = "period";
            return View("Add", new Period());
        }

        [HttpPost]
        [Route("add")]
        public IActionResult Add(Period period)
        {
            try
            {
                var insert = InsertPeriod(period);
                return RedirectToAction("Index");
            }
            catch
            {
                ViewBag.msg = "Failed";
                return View("Add", new Period());
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
                var delete = DeletePeriod(id);

                if (delete)
                {
                    ViewBag.msg = "Done";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.msg = "Failed";
                    ViewBag.periods = GetAllPeriods();
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                ViewBag.msg = "Failed";
                ViewBag.periods = GetAllPeriods();
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Edit
        [HttpGet]
        [Route("edit/{id}")]
        public IActionResult Edit(int id)
        {
            var period = GetPeriodById(id);
            return View("Edit", period);
        }

        [HttpPost]
        [Route("edit/{id}")]
        public IActionResult Edit(int id, Period period)
        {
            try
            {
                var update = UpdatePeriod(period);

                if (update)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.msg = "Failed";
                    period = GetPeriodById(id);
                    return View("Edit", period);
                }
            }
            catch
            {
                ViewBag.msg = "Failed";
                period = GetPeriodById(id);
                return View("Edit", period);
            }
        }

        #endregion

        #region Methods
        public static List<Period> GetAllPeriods()
        {
            List<Period> periods = new List<Period>();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("select * from period where Status = 1", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var period = new Period();

                        period.PeriodId = Convert.ToInt32(reader["PeriodId"]);
                        period.Name = Convert.ToString(reader["Name"]);
                        period.Color = Convert.ToString(reader["Color"]);
                        period.Status = Convert.ToBoolean(reader["Status"]);

                        periods.Add(period);
                    }

                    if (periods.Count > 0)
                    {
                        return periods;
                    }
                }
            }

            return periods;
        }

        public static Period GetPeriodById(int periodId)
        {
            Period period = new Period();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("select * from period where PeriodId = @periodId", connection);
                command.Parameters.AddWithValue("@periodId", periodId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        period.PeriodId = Convert.ToInt32(reader["PeriodId"]);
                        period.Name = Convert.ToString(reader["Name"]);
                        period.Color = Convert.ToString(reader["Color"]);
                        period.Status = Convert.ToBoolean(reader["Status"]);
                    }
                }

                if (period != null)
                {
                    return period;
                }
            }

            return null;
        }

        public static bool InsertPeriod(Period _period)
        {
            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"insert into period(Name, Color, Status)
                                                            values(@name, @color, @status)", connection);

                command.Parameters.AddWithValue("@name", _period.Name);
                command.Parameters.AddWithValue("@color", _period.Color);
                command.Parameters.AddWithValue("@status", _period.Status);

                var insert = command.ExecuteNonQuery();

                if (Convert.ToBoolean(insert))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool UpdatePeriod(Period _period)
        {
            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"update period
                                                            set Name = @name,
                                                                Color = @color,
                                                                Status = @status
                                                          where PeriodId = @periodId", connection);

                command.Parameters.AddWithValue("@name", _period.Name);
                command.Parameters.AddWithValue("@color", _period.Color);
                command.Parameters.AddWithValue("@status", _period.Status);

                command.Parameters.AddWithValue("@periodId", _period.PeriodId);

                var update = command.ExecuteNonQuery();

                if (Convert.ToBoolean(update))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool DeletePeriod(int id)
        {
            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("update period set Status = 0 where PeriodId = @periodId", connection);
                command.Parameters.AddWithValue("@periodId", id.ToString());

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
