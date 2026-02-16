using Microsoft.EntityFrameworkCore;
using System;

namespace MariaDB
{
    public class MariaDBContext : DbContext
    {
        /// <summary>
        /// Таблица users
        /// </summary>
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connection = "server=localhost;database=mydatabase;user=myuser;password=mypassword";
            optionsBuilder.UseMySql(connection, new MySqlServerVersion(new Version(10, 0, 11)));
        }
    }

    public class User
    {
        public int Id { get; set; }

        /// <summary>
        /// поле token в таблице users
        /// </summary>
        public string token { get; set; }
    }
}
