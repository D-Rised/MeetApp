using MeetApp.DAL.Models;
using MeetApp.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MeetApp.Web.Models;

namespace MeetApp.Web.Services
{
    public class UserService : IUserService
    {
        private readonly IMeetRepository _meetRepository;

        public UserService(IMeetRepository meetRepository)
        {
            _meetRepository = meetRepository;
        }

        public List<User> GetAllUsers()
        {
            return _meetRepository.GetAllUsers().ToList();
        }

        public User GetUserById(Guid id)
        {
            return _meetRepository.Get<User>().FirstOrDefault(x => x.Id == id);
        }

        public User GetUserByLogin(string login)
        {
            return _meetRepository.Get<User>().FirstOrDefault(x => x.UserName == login);
        }

        public void DeleteUser(User user)
        {
            if (user != null)
                _meetRepository.DeleteUser(user);
        }
    }
}
