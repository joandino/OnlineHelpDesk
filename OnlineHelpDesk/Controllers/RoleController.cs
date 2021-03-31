using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using OnlineHelpDesk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineHelpDesk.Controllers
{
    [Route("account")]
    public class RoleController : Controller
    {
        private HelpDeskEntities db;

        public RoleController(HelpDeskEntities _db)
        {
            db = _db;
        }
        public IActionResult Index()
        {
            return View();
        }

        #region Methods
        public static Role GetRoleById(int roleId)
        {
            Role role = new Role();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("select * from role where RoleId = @roleId", connection);
                command.Parameters.AddWithValue("@roleId", roleId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        role.RoleId = Convert.ToInt32(reader["RoleId"]);
                        role.Name = Convert.ToString(reader["Name"]);
                        role.Status = Convert.ToBoolean(reader["Status"]);

                    }
                }

                if (role != null)
                {
                    return role;
                }
            }

            return null;
        }

        public static List<Role> GetRoles()
        {
            List<Role> roles = new List<Role>();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("select * from role where RoleId <> 1", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var role = new Role();

                        role.RoleId = Convert.ToInt32(reader["RoleId"]);
                        role.Name = Convert.ToString(reader["Name"]);
                        role.Status = Convert.ToBoolean(reader["Status"]);

                        roles.Add(role);
                    }

                    if (roles.Count > 0)
                    {
                        return roles;
                    }
                }
            }

            return roles;
        }

        #endregion
    }
}
