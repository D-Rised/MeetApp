using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeetingApp.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingApp.DAL.Repositories
{
    public class DbRepository
    {
        private readonly DataContext _context;
        
        public DbRepository(DataContext context)
        {
            _context = context;
        }

        public IQueryable<User> GetAllUsers()
        {
            return _context.Users.OrderBy(x => x.login);
        }

        public IQueryable<Meeting> GetAllMeetings()
        {
            var meetings = _context.Meetings.Include(x => x.datesList).Include(c => c.membersList);
            return meetings;
        }

        public IQueryable<T> Get<T>() where T : class
        {
            return _context.Set<T>().AsQueryable();
        }

        public void Add<T>(T newEntity) where T : class
        {
            _context.Set<T>().Add(newEntity);
            _context.SaveChanges();
        }

        public void SaveAll()
        {
            _context.SaveChanges();
        }
        public void DeleteMeeting(Meeting meeting)
        {
            _context.Meetings.Remove(meeting);
            _context.SaveChanges();
        }

        public void DeleteUser(User entity)
        {
            _context.Users.Remove(entity);
            _context.SaveChanges();
        }
    }
}
