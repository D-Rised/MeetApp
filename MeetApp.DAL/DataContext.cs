using MeetApp.DAL.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace MeetApp.DAL
{
    public class DataContext : IdentityDbContext<User, Role, Guid>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Meet> Meets { get; set; }

        public DbSet<Dates> Dates { get; set; }

        public DbSet<Member> Members { get; set; }
    }
}
