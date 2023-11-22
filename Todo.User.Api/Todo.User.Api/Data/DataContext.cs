using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Todo.User.Api.Models;

namespace Todo.User.Api.Data
{
    public class DataContext : DbContext
    {
        public string connectionString = "Data source=DataBases/Users.db";
        public DataContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<UserInfo> Users { get; set; }
    }
}
