using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeetApp.DAL.Models
{
    public class Member
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public Guid MeetId { get; set; }
        public bool IsOwner { get; set; }
        public bool Ready { get; set; }
        public IList<Dates> DatesList { get; set; }
    }
}
