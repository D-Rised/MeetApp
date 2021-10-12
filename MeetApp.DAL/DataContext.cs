using MeetApp.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetApp.DAL
{
    public class DataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Meet> Meets { get; set; }
        public DbSet<Dates> Dates { get; set; }
        public DbSet<Member> Members { get; set; }
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
    }
}
