using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeetingApp.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingApp.DAL.Repositories
{
    public class UsersRepository
    {
        private readonly DataContext _context;
        
        public UsersRepository(DataContext context)
        {
            _context = context;
        }

        public IQueryable<User> GetUsers()
        {
            return _context.Users.OrderBy(x => x.login);
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
