using System;
using MeetingApp.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingApp.DAL
{
    public class DataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Dates> Dates { get; set; }
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }


    }
}
