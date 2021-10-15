using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetApp.DAL.Models
{
    public class User : IdentityUser<Guid>
    {
        public DateTime RegistrationDate { get; set; }
    }
}
