using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using OnlineHelpDesk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineHelpDesk.Controllers
{
    [Route("discussion")]
    public class DiscussionController : Controller
    {
        private HelpDeskEntities db;
        [Obsolete]
        private IHostingEnvironment hostingEnvironment;

        [Obsolete]
        public DiscussionController(HelpDeskEntities _db, IHostingEnvironment _hostingEnvironment)
        {
            db = _db;
            hostingEnvironment = _hostingEnvironment;
        }

        #region Methods
        public static List<Discussion> GetDiscussionsByTicketId(int tikcetId)
        {
            List<Discussion> discussions = new List<Discussion>();

            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("select * from discussion where TicketId = @ticketId", connection);

                command.Parameters.AddWithValue("@ticketId", tikcetId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var discussion = new Discussion();

                        discussion.Content = Convert.ToString(reader["Content"]);
                        discussion.CreatedDate = Convert.ToDateTime(reader["CreatedDate"]);
                        discussion.TicketId = Convert.ToInt32(reader["TicketId"]);

                        discussion.AccountId = Convert.ToInt32(reader["AccountId"]);
                        var account = AccountController.GetAccountById(discussion.AccountId);
                        discussion.Account = account;

                        discussions.Add(discussion);
                    }

                    if (discussions.Count > 0)
                    {
                        return discussions;
                    }
                }
            }

            return discussions;
        }

        public static bool InsertDiscussion(Discussion _discussion)
        {
            using (MySqlConnection connection = HelpDeskEntities.GetConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"insert into discussion(CreatedDate, Content, TicketId, AccountId)
                                                            values(@createdDate, @content, @ticketId, @accountId)", connection);

                command.Parameters.AddWithValue("@createdDate", _discussion.CreatedDate);
                command.Parameters.AddWithValue("@content", _discussion.Content);
                command.Parameters.AddWithValue("@ticketId", _discussion.TicketId);
                command.Parameters.AddWithValue("@accountId", _discussion.AccountId);

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
