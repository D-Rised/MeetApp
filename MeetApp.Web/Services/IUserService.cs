using MeetApp.DAL.Models;
using System;
using System.Collections.Generic;

namespace MeetApp.Web.Services
{
    public interface IUserService
    {
        public List<User> GetAllUsers();

        public User GetUserById(Guid id);

        public User GetUserByLogin(string login);

        public void DeleteUser(User user);
    }
}
