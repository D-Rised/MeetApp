using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeetApp.DAL.Models
{
    public class Member
    {
        public int Id { get; set; }
        public Guid userId { get; set; }
        public Guid meetId { get; set; }
        public string role { get; set; }
        public string state { get; set; }
        public IList<Dates> datesList { get; set; }
    }
}
