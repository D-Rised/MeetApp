using System;
using MeetingApp.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingApp.DAL
{
    public class DataContext : DbContext
    {
        DbSet<User> Users { get; set; }
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
    }
}
