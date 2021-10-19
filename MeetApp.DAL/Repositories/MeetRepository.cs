using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeetApp.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetApp.DAL.Repositories
{
    public class MeetRepository
    {
        private readonly DataContext _context;
        
        public MeetRepository(DataContext context)
        {
            _context = context;
        }

        public IQueryable<User> GetAllUsers()
        {
            return _context.Users;
        }

        public IQueryable<Meet> GetAllMeets()
        {
            var meets = _context.Meets.Include(x => x.DatesList).Include(x => x.MembersList);
            return meets;
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
        public void DeleteMeet(Meet meet)
        {
            _context.Meets.Remove(meet);
            _context.SaveChanges();
        }

        public void DeleteUser(User entity)
        {
            _context.Users.Remove(entity);
            _context.SaveChanges();
        }

        public void DeleteMember(Member entity)
        {
            List<Dates> allDates = _context.Dates.ToList();
            for (int i = 0; i < allDates.Count; i++)
            {
                if (allDates[i].UserId == entity.UserId)
                {
                    _context.Dates.Remove(allDates[i]);
                }
            }
            _context.Members.Remove(entity);
            _context.SaveChanges();
        }
    }
}
