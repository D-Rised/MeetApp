using MeetApp.DAL.Models;
using System;
using System.Collections.Generic;

namespace MeetApp.Web.Models
{
    public class CreateMeetViewModel
    {
        public string Title { get; set; }

        public string UserLogin { get; set; }

        public bool FixedDate { get; set; }

        public IList<Dates> DatesList { get; set; }

        public string AlertMessage { get; set; }

        public int DateIndexToDelete { get; set; }
    }
}
