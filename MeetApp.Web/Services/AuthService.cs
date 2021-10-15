using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeetApp.DAL.Models;
using MeetApp.DAL.Repositories;
using Microsoft.AspNetCore.Identity;

namespace MeetApp.Web.Services
{
    public class AuthService
    {
        private readonly DbRepository _usersRepository;

        public AuthService(DbRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public User GetUserById(Guid id)
        {
            return _usersRepository.Get<User>().FirstOrDefault(x => x.Id == id);
        }
        public User GetUserByLogin(string login)
        {
            return _usersRepository.Get<User>().FirstOrDefault(x => x.UserName == login);
        }
        public void CreateNewUser(User user)
        {
            _usersRepository.Add(user);
        }
    }
}
