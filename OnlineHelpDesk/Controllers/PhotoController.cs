using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using OnlineHelpDesk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineHelpDesk.Controllers
{
    public class PhotoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        #region Methods
        public static bool InsertPhoto(Photo _photo)
        {
            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"insert into photo(Name, TicketId)
                                                            values(@name, @ticketId)", connection);

                command.Parameters.AddWithValue("@name", _photo.Name);
                command.Parameters.AddWithValue("@ticketId", _photo.TicketId);

                var insert = command.ExecuteNonQuery();

                if (Convert.ToBoolean(insert))
                {
                    return true;
                }
            }

            return false;
        }

        public static List<Photo> GetPhotosByTicketId(int ticketId)
        {
            List<Photo> photos = new List<Photo>();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"select * from photo where TicketId = @ticketId", connection);

                command.Parameters.AddWithValue("@ticketId", ticketId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Photo photo = new Photo();

                        photo.PhotoId = Convert.ToInt32(reader["PhotoId"]);
                        photo.Name = Convert.ToString(reader["Name"]);
                        photo.TicketId = Convert.ToInt32(reader["TicketId"]);

                        photos.Add(photo);
                    }

                    if (photos.Count > 0)
                    {
                        return photos;
                    }
                }
            }

            return photos;
        }

        #endregion
    }
}
