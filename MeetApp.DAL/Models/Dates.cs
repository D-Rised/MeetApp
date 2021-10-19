using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetApp.DAL.Models
{
    public class Dates
    {
        public int Id { get; set; }
        public Guid MeetId { get; set; }
        public Guid UserId { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
    }
}
