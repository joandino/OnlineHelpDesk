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
    [Route("account")]
    public class AccountController : Controller
    {
        private HelpDeskEntities db;

        public AccountController(HelpDeskEntities _db)
        {
            db = _db;
        }

        #region Profile
        [Authorize(Roles = "Administrator,Support,Employee")]
        [HttpGet]
        [Route("profile")]
        public IActionResult Profile()
        {
            var userName = User.FindFirst(ClaimTypes.NameIdentifier);
            var account = GetAccountByUserName(userName.Value);
            ViewBag.sideBar = "account";
            return View("Profile", account);
        }

        [Authorize(Roles = "Administrator,Support,Employee")]
        [HttpPost]
        [Route("profile")]
        public IActionResult Profile(Account _account)
        {
            Account account = null;
            try
            {
                account = UpdateAccount(_account);
                ViewBag.msg = "Done";
            }
            catch (Exception)
            {
                ViewBag.msg = "Failed";
            }

            return View("Profile", account);
        }

        #endregion

        #region Add
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        [Route("add")]
        public IActionResult Add()
        {
            var accountViewModel = new AccountViewModel();
            accountViewModel.Account = new Account();

            var roles = RoleController.GetRoles();
            accountViewModel.Roles = new SelectList(roles, "RoleId", "Name");

            ViewBag.sideBar = "account";

            return View("Add", accountViewModel);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [Route("add")]
        public IActionResult Add(AccountViewModel accountViewModel)
        {
            try
            {
                var insert = InsertAccount(accountViewModel);

                if (insert)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.msg = "Failed";
                    return View("Add", accountViewModel);
                }
            }
            catch (Exception)
            {
                ViewBag.msg = "Failed";
                return View("Add", accountViewModel);
            }
        }
        #endregion

        #region Edit
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        [Route("edit/{id}")]
        public IActionResult Edit(int id)
        {
            var accountViewModel = new AccountViewModel();
            accountViewModel.Account = GetAccountById(id);

            var roles = RoleController.GetRoles();
            accountViewModel.Roles = new SelectList(roles, "RoleId", "Name");

            return View("Edit", accountViewModel);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [Route("edit/{id}")]
        public IActionResult Edit(int id, AccountViewModel accountViewModel)
        {
            try
            {
                var account = GetAccountById(id);
                var password = account.Password;

                if (!string.IsNullOrEmpty(accountViewModel.Account.Password))
                {
                    password = BCrypt.Net.BCrypt.HashPassword(accountViewModel.Account.Password, BCrypt.Net.BCrypt.GenerateSalt());
                }

                accountViewModel.Account.Password = password;
                var update = UpdateAccount(accountViewModel.Account);

                if (update != null)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.msg = "Failed";
                    return View("Edit", accountViewModel);
                }
            }
            catch (Exception)
            {
                ViewBag.msg = "Failed";
                return View("Edit", accountViewModel);
            }
        }

        #endregion

        #region Delete
        [Authorize(Roles = "Administrator")]
        [Route("delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var delete = DeleteAccount(id);

                if (delete)
                {
                    ViewBag.msg = "Done";
                }
                else
                {
                    ViewBag.msg = "Failed";
                }
            }
            catch (Exception)
            {
                ViewBag.msg = "Failed";
            }

            ViewBag.sideBar = "account";
            ViewBag.accounts = GetAllAccounts();
            return View("Index");
        }
        #endregion

        #region Index
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        [Route("index")]
        [Route("")]
        public IActionResult Index()
        {
            ViewBag.accounts = GetAllAccounts();
            ViewBag.sideBar = "account";
            return View("Index");
        }
        #endregion

        #region Methods
        public static List<Account> GetAllAccounts()
        {
            List<Account> accounts = new List<Account>();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("select * from account where RoleId <> 1 and Status = 1", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var user = new Account();

                        user.AccountId = Convert.ToInt32(reader["AccountId"]);
                        user.Username = Convert.ToString(reader["Username"]);
                        user.Password = Convert.ToString(reader["Password"]);
                        user.FullName = Convert.ToString(reader["FullName"]);
                        user.Email = Convert.ToString(reader["Email"]);
                        user.RoleId = Convert.ToInt32(reader["RoleId"]);

                        Role role = new Role();
                        role = RoleController.GetRoleById(user.RoleId);

                        user.Role = role;
                        user.Status = Convert.ToBoolean(reader["Status"]);

                        accounts.Add(user);
                    }

                    if(accounts.Count > 0)
                    {
                        return accounts;
                    }
                }
            }

            return accounts;
        }
        public static Account GetAccountByUserName(string userName)
        {
            Account user = new Account();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("select * from account where UserName = @userName", connection);
                command.Parameters.AddWithValue("@userName", userName);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        user.AccountId = Convert.ToInt32(reader["AccountId"]);
                        user.Username = Convert.ToString(reader["UserName"]);
                        user.Password = Convert.ToString(reader["Password"]);
                        user.FullName = Convert.ToString(reader["FullName"]);
                        user.Email = Convert.ToString(reader["Email"]);
                        user.RoleId = Convert.ToInt32(reader["RoleId"]);
                        user.Status = Convert.ToBoolean(reader["Status"]);
                    }
                }

                if (user != null)
                {
                    return user;
                }
            }

            return null;
        }

        public static Account GetAccountById(int accountId)
        {
            Account user = new Account();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("select * from account where AccountId = @accountId", connection);
                command.Parameters.AddWithValue("@accountId", accountId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        user.AccountId = Convert.ToInt32(reader["AccountId"]);
                        user.Username = Convert.ToString(reader["UserName"]);
                        user.Password = Convert.ToString(reader["Password"]);
                        user.FullName = Convert.ToString(reader["FullName"]);
                        user.Email = Convert.ToString(reader["Email"]);
                        user.RoleId = Convert.ToInt32(reader["RoleId"]);
                        user.Status = Convert.ToBoolean(reader["Status"]);
                    }
                }

                if (user != null)
                {
                    return user;
                }
            }

            return null;
        }

        public static bool DeleteAccount(int id)
        {
            Account user = new Account();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("update account set Status = 0 where AccountId = @accountId", connection);
                command.Parameters.AddWithValue("@accountId", id.ToString());

                var delete = command.ExecuteNonQuery();

                if (Convert.ToBoolean(delete))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool InsertAccount(AccountViewModel _accountViewModel)
        {
            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"insert into account(UserName, Password, FullName, Email, RoleId, Status)
                                                            values(@userName, @password, @fullName, @email, @roleId, @status)", connection);

                command.Parameters.AddWithValue("@userName", _accountViewModel.Account.Username);

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(_accountViewModel.Account.Password, BCrypt.Net.BCrypt.GenerateSalt());
                command.Parameters.AddWithValue("@password", hashedPassword);

                command.Parameters.AddWithValue("@fullName", _accountViewModel.Account.FullName);
                command.Parameters.AddWithValue("@email", _accountViewModel.Account.Email);
                command.Parameters.AddWithValue("@roleId", _accountViewModel.Account.RoleId);
                command.Parameters.AddWithValue("@status", _accountViewModel.Account.Status);

                var insert = command.ExecuteNonQuery();

                if (Convert.ToBoolean(insert))
                {
                    return true;
                }
            }

            return false;
        }

        public static Account UpdateAccount(Account _account)
        {
            Account account = new Account();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"update account
                                                            set FullName = @fullName,
                                                                UserName = @userName,
                                                                Email = @email,
                                                                RoleId = @roleId,
                                                                Password = @password
                                                          where AccountId = @accountId", connection);
                
                command.Parameters.AddWithValue("@fullName", _account.FullName);
                command.Parameters.AddWithValue("@userName", _account.Username);
                command.Parameters.AddWithValue("@email", _account.Email);
                command.Parameters.AddWithValue("@roleId", _account.RoleId);

                //var hashedPassword = BCrypt.Net.BCrypt.HashPassword(_account.Password, BCrypt.Net.BCrypt.GenerateSalt());
                command.Parameters.AddWithValue("@password", _account.Password);

                command.Parameters.AddWithValue("@accountId", _account.AccountId);

                var update = command.ExecuteNonQuery();

                if (Convert.ToBoolean(update))
                {
                    account = GetAccountByUserName(_account.Username);
                    return account;
                }
            }

            return null;
        }

        public static Account CheckAccount(string email, string password)
        {
            Account user = new Account();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("select * from account where email = @email", connection);
                command.Parameters.AddWithValue("@email", email);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        user.AccountId = Convert.ToInt32(reader["AccountId"]);
                        user.Username = Convert.ToString(reader["UserName"]);
                        user.Password = Convert.ToString(reader["Password"]);
                        user.FullName = Convert.ToString(reader["FullName"]);
                        user.Email = Convert.ToString(reader["Email"]);
                        user.RoleId = Convert.ToInt32(reader["RoleId"]);
                        user.Status = Convert.ToBoolean(reader["Status"]);
                    }
                }

                if (user != null && !string.IsNullOrEmpty(user.Username))
                {
                    var hashedPass = BCrypt.Net.BCrypt.HashPassword(password);

                    if (BCrypt.Net.BCrypt.Verify(password, user.Password))
                    {
                        return user;
                    }
                }
            }

            return null;
        }

        public static List<Account> GetAccountsByRoleId(int roleId)
        {
            List<Account> accounts = new List<Account>();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("select * from account where RoleId = @roleId and Status = 1", connection);

                command.Parameters.AddWithValue("@roleId", roleId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var user = new Account();

                        user.AccountId = Convert.ToInt32(reader["AccountId"]);
                        user.Username = Convert.ToString(reader["Username"]);
                        user.Password = Convert.ToString(reader["Password"]);
                        user.FullName = Convert.ToString(reader["FullName"]);
                        user.Email = Convert.ToString(reader["Email"]);
                        user.RoleId = Convert.ToInt32(reader["RoleId"]);

                        Role role = new Role();
                        role = RoleController.GetRoleById(user.RoleId);

                        user.Role = role;
                        user.Status = Convert.ToBoolean(reader["Status"]);

                        accounts.Add(user);
                    }

                    if (accounts.Count > 0)
                    {
                        return accounts;
                    }
                }
            }

            return accounts;
        }
        #endregion
    }
}
