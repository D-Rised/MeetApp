using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeetingApp.DAL.Models;
using MeetingApp.DAL.Repositories;
using Microsoft.AspNetCore.Identity;

namespace MeetingApp.Web.Services
{
    public class AuthService
    {
        private readonly DbRepository _usersRepository;

        public AuthService(DbRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public User GetUserById(int id)
        {
            return _usersRepository.Get<User>().FirstOrDefault(x => x.Id == id);
        }
        public User GetUserByLogin(string login)
        {
            return _usersRepository.Get<User>().FirstOrDefault(x => x.login == login);
        }
        public void CreateNewUser(User user)
        {
            _usersRepository.Add(user);
        }
    }
}
