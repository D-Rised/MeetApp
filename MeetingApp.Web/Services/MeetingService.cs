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
    public class MeetingService
    {
        private readonly UsersRepository _usersRepository;
        private readonly MeetingsRepository _meetingsRepository;

        public MeetingService(UsersRepository usersRepository, MeetingsRepository meetingsRepository)
        {
            _usersRepository = usersRepository;
            _meetingsRepository = meetingsRepository;
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
