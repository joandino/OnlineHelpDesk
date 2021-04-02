using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using OnlineHelpDesk.Models;
using OnlineHelpDesk.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineHelpDesk.Controllers
{
    [Authorize(Roles = "Administrator")]
    [Route("subcategory")]
    public class SubCategoryController : Controller
    {
        private HelpDeskEntities db;

        public SubCategoryController(HelpDeskEntities _db)
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
            ViewBag.subcategories = GetAllSubCategories();
            ViewBag.sideBar = "subcategory";
            return View("Index");
        }
        #endregion

        #region Add
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        [Route("add")]
        public IActionResult Add()
        {
            var subCategoryViewModel = new SubCategoryViewModel();
            subCategoryViewModel.SubCategory = new SubCategory();

            var categories = CategoryController.GetAllCategories();
            subCategoryViewModel.Categories = new SelectList(categories, "CategoryId", "Name");

            ViewBag.sideBar = "subcategory";

            return View("Add", subCategoryViewModel);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [Route("add")]
        public IActionResult Add(SubCategoryViewModel subCategoryViewModel)
        {
            try
            {
                var insert = InsertSubCategory(subCategoryViewModel);

                if (insert)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.msg = "Failed";
                    return View("Add", subCategoryViewModel);
                }
            }
            catch (Exception)
            {
                ViewBag.msg = "Failed";
                return View("Add", subCategoryViewModel);
            }
        }
        #endregion

        #region Edit
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        [Route("edit/{id}")]
        public IActionResult Edit(int id)
        {
            var subCategoryViewModel = new SubCategoryViewModel();
            subCategoryViewModel.SubCategory = GetSubCategoryById(id);

            var categories = CategoryController.GetAllCategories();
            subCategoryViewModel.Categories = new SelectList(categories, "CategoryId", "Name");

            return View("Edit", subCategoryViewModel);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [Route("edit/{id}")]
        public IActionResult Edit(int id, SubCategoryViewModel subCategoryViewModel)
        {
            try
            {
                var update = UpdateSubCategory(subCategoryViewModel.SubCategory);

                if (update)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.msg = "Failed";
                    return View("Edit", subCategoryViewModel);
                }
            }
            catch (Exception)
            {
                ViewBag.msg = "Failed";
                return View("Edit", subCategoryViewModel);
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
                var delete = DeleteSubCategory(id);

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

            ViewBag.sideBar = "subcategory";
            ViewBag.subcategories = GetAllSubCategories();
            return View("Index");
        }
        #endregion

        #region Methods
        public static List<SubCategory> GetAllSubCategories()
        {
            List<SubCategory> subCategories = new List<SubCategory>();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("select * from subcategory where Status = 1", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var subCategory = new SubCategory();

                        subCategory.SubCategoryId = Convert.ToInt32(reader["SubCategoryId"]);
                        subCategory.Name = Convert.ToString(reader["Name"]);

                        subCategory.CategoryId = Convert.ToInt32(reader["CategoryId"]);
                        subCategory.Category = CategoryController.GetCategoryById(subCategory.CategoryId);

                        subCategory.Status = Convert.ToBoolean(reader["Status"]);

                        subCategories.Add(subCategory);
                    }

                    if (subCategories.Count > 0)
                    {
                        return subCategories;
                    }
                }
            }

            return subCategories;
        }

        public static List<SubCategory> GetSubCategoriesByCategoryId(int categoryId)
        {
            List<SubCategory> subCategories = new List<SubCategory>();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("select * from subcategory where CategoryId = @categoryId and Status = 1", connection);

                command.Parameters.AddWithValue("@categoryId", categoryId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var subCategory = new SubCategory();

                        subCategory.SubCategoryId = Convert.ToInt32(reader["SubCategoryId"]);
                        subCategory.Name = Convert.ToString(reader["Name"]);

                        subCategory.CategoryId = Convert.ToInt32(reader["CategoryId"]);
                        subCategory.Category = CategoryController.GetCategoryById(subCategory.CategoryId);

                        subCategory.Status = Convert.ToBoolean(reader["Status"]);

                        subCategories.Add(subCategory);
                    }

                    if (subCategories.Count > 0)
                    {
                        return subCategories;
                    }
                }
            }

            return subCategories;
        }

        public static bool InsertSubCategory(SubCategoryViewModel _subCategoryViewModel)
        {
            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"insert into subcategory(Name, CategoryId, Status)
                                                            values(@name, @categoryId, @status)", connection);

                command.Parameters.AddWithValue("@name", _subCategoryViewModel.SubCategory.Name);
                command.Parameters.AddWithValue("@categoryId", _subCategoryViewModel.SubCategory.CategoryId);
                command.Parameters.AddWithValue("@status", _subCategoryViewModel.SubCategory.Status);

                var insert = command.ExecuteNonQuery();

                if (Convert.ToBoolean(insert))
                {
                    return true;
                }
            }

            return false;
        }

        public static SubCategory GetSubCategoryById(int subCategoryId)
        {
            SubCategory subCategory = new SubCategory();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("select * from subcategory where SubCategoryId = @subCategoryId", connection);
                command.Parameters.AddWithValue("@subCategoryId", subCategoryId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        subCategory.SubCategoryId = Convert.ToInt32(reader["SubCategoryId"]);
                        subCategory.Name = Convert.ToString(reader["Name"]);
                        subCategory.CategoryId = Convert.ToInt32(reader["CategoryId"]);
                        subCategory.Status = Convert.ToBoolean(reader["Status"]);
                    }
                }

                if (subCategory != null)
                {
                    return subCategory;
                }
            }

            return null;
        }

        public static bool UpdateSubCategory(SubCategory _subCategory)
        {
            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"update subcategory
                                                            set Name = @name,
                                                                CategoryId = @categoryId,
                                                                Status = @status
                                                          where SubCategoryId = @subCategoryId", connection);

                command.Parameters.AddWithValue("@name", _subCategory.Name);
                command.Parameters.AddWithValue("@categoryId", _subCategory.CategoryId);
                command.Parameters.AddWithValue("@status", _subCategory.Status);

                command.Parameters.AddWithValue("@subCategoryId", _subCategory.SubCategoryId);

                var update = command.ExecuteNonQuery();

                if (Convert.ToBoolean(update))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool DeleteSubCategory(int subCategoryId)
        {
            Account user = new Account();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("update subcategory set Status = 0 where SubCategoryId = @subCategoryId", connection);
                command.Parameters.AddWithValue("@subCategoryId", subCategoryId);

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
