using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MySql.Data.Common;
using MySql.Data.MySqlClient;
using MySql.Data.MySqlClient.Authentication;

#nullable disable

namespace OnlineHelpDesk.Models
{
    public partial class HelpDeskEntities : DbContext
    {
        public static string connectionString { get; set; }
        public HelpDeskEntities(string _connectionString)
        {
            connectionString = _connectionString;
        }

        public HelpDeskEntities(DbContextOptions<HelpDeskEntities> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Discussion> Discussions { get; set; }
        public virtual DbSet<Period> Periods { get; set; }
        public virtual DbSet<Photo> Photos { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Status> Statuses { get; set; }
        public virtual DbSet<Ticket> Tickets { get; set; }

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}
