using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace LogUserRequest
{

    public class AppDbContext : DbContext
    {
        public DbSet<LogModelSql> jurnal { get; set; }

        public DbSet<UserInfoModelSql> unfo { get; set; }

        public DbSet<HeaderModelSql> headers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=cache/userlog.sql"); // ;Pooling=true;
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<LogModelSql>()
        //                .HasIndex(j => new { j.uid, j.time })
        //                .IsDescending(false, true);
        //}
    }

    public class LogModelSql
    {
        public int Id { get; set; }

        public DateTime time { get; set; }

        public string uri { get; set; }

        public string uid { get; set; }

        public string unfo { get; set; }

        public string header { get; set; }
    }

    public class UserInfoModelSql
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public string IP { get; set; }

        public string Country { get; set; }

        public string UserAgent { get; set; }
    }


    public class HeaderModelSql
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        // Словарь будет храниться как JSON строка
        public string HeadersJson { get; set; }

        // Свойство для работы со словарем
        [NotMapped] // Не сохранять в БД напрямую
        public Dictionary<string, string> Headers
        {
            get => string.IsNullOrEmpty(HeadersJson)
                ? new Dictionary<string, string>()
                : JsonSerializer.Deserialize<Dictionary<string, string>>(HeadersJson);
            set => HeadersJson = JsonSerializer.Serialize(value);
        }
    }
}
