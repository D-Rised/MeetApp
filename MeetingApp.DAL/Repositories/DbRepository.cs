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
        public IQueryable<Meeting> GetAllMeetingsForUser(User user)
        {
            return _context.Meetings.Where(x => x.user_Id == user.Id);
        }
        public IQueryable<Dates> GetAllDatesForMeeting(Meeting meeting)
        {
            return _context.Dates.Where(x => x.meetingId == meeting.Id);
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

        public int SaveUser(User entity)
        {
            if (entity.Id == default)
            {
                _context.Entry(entity).State = EntityState.Added;
            }
            else
            {
                _context.Entry(entity).State = EntityState.Modified;
            }
            _context.SaveChanges();

            return entity.Id;
        }

        public void DeleteUser(User entity)
        {
            _context.Users.Remove(entity);
            _context.SaveChanges();
        }
    }
}
