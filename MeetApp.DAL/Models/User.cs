using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetApp.DAL.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string login { get; set; }
        public string password { get; set; }
    }
}
