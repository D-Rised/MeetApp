using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeetApp.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetApp.DAL.Repositories
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
            return _context.Users;
        }

        public IQueryable<Meet> GetAllMeets()
        {
            var meets = _context.Meets.Include(x => x.datesList).Include(x => x.membersList);
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
            _context.Members.Remove(entity);
            _context.SaveChanges();
        }
    }
}
