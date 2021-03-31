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
    [Route("category")]
    public class CategoryController : Controller
    {
        private HelpDeskEntities db;

        public CategoryController(HelpDeskEntities _db)
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
            ViewBag.categories = GetAllCategories();
            ViewBag.sideBar = "category";
            return View("Index");
        }
        #endregion

        #region Add
        [HttpGet]
        [Route("add")]
        public IActionResult Add()
        {
            ViewBag.sideBar = "category";
            return View("Add", new Category());
        }

        [HttpPost]
        [Route("add")]
        public IActionResult Add(Category category)
        {
            try
            {
                var insert = InsertCategory(category);
                return RedirectToAction("Index");
            }
            catch
            {
                ViewBag.msg = "Failed";
                return View("Add", new Category());
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
                var delete = DeleteCategory(id);

                if (delete)
                {
                    ViewBag.msg = "Done";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.msg = "Failed";
                    ViewBag.categories = GetAllCategories();
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                ViewBag.msg = "Failed";
                ViewBag.categories = GetAllCategories();
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Edit
        [HttpGet]
        [Route("edit/{id}")]
        public IActionResult Edit(int id)
        {
            var category = GetCategoryById(id);
            return View("Edit", category);
        }

        [HttpPost]
        [Route("edit/{id}")]
        public IActionResult Edit(int id, Category category)
        {
            try
            {
                var update = UpdateCategory(category);

                if (update)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.msg = "Failed";
                    category = GetCategoryById(id);
                    return View("Edit", category);
                }
            }
            catch
            {
                ViewBag.msg = "Failed";
                category = GetCategoryById(id);
                return View("Edit", category);
            }
        }

        #endregion

        #region Methods
        public static List<Category> GetAllCategories()
        {
            List<Category> categories = new List<Category>();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("select * from category where Status = 1", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var category = new Category();

                        category.CategoryId = Convert.ToInt32(reader["CategoryId"]);
                        category.Name = Convert.ToString(reader["Name"]);
                        category.Status = Convert.ToBoolean(reader["Status"]);

                        categories.Add(category);
                    }

                    if (categories.Count > 0)
                    {
                        return categories;
                    }
                }
            }

            return categories;
        }

        public static Category GetCategoryById(int categoryId)
        {
            Category category = new Category();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("select * from category where CategoryId = @categoryId", connection);
                command.Parameters.AddWithValue("@categoryId", categoryId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        category.CategoryId = Convert.ToInt32(reader["CategoryId"]);
                        category.Name = Convert.ToString(reader["Name"]);
                        category.Status = Convert.ToBoolean(reader["Status"]);
                    }
                }

                if (category != null)
                {
                    return category;
                }
            }

            return null;
        }

        public static bool InsertCategory(Category _category)
        {
            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"insert into category(Name, Status)
                                                            values(@name, @status)", connection);

                command.Parameters.AddWithValue("@name", _category.Name);
                command.Parameters.AddWithValue("@status", _category.Status);

                var insert = command.ExecuteNonQuery();

                if (Convert.ToBoolean(insert))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool UpdateCategory(Category _category)
        {
            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"update category
                                                            set Name = @name,
                                                                Status = @status
                                                          where CategoryId = @categoryId", connection);

                command.Parameters.AddWithValue("@name", _category.Name);
                command.Parameters.AddWithValue("@status", _category.Status);

                command.Parameters.AddWithValue("@categoryId", _category.CategoryId);

                var update = command.ExecuteNonQuery();

                if (Convert.ToBoolean(update))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool DeleteCategory(int id)
        {
            Category category = new Category();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("update category set Status = 0 where CategoryId = @categoryId", connection);
                command.Parameters.AddWithValue("@categoryId", id.ToString());

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
