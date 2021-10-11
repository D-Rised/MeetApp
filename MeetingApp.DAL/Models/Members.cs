using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeetingApp.DAL.Models
{
    public class Members
    {
        public int Id { get; set; }
        public Guid userId { get; set; }
        public Guid meetingId { get; set; }
        public string role { get; set; }
    }
}
