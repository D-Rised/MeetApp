using MeetApp.DAL.Models;
using System;
using System.Collections.Generic;

namespace MeetApp.Web.Models
{
    public class MeetListViewModel
    {
        public IList<Meet> OwnedMeets { get; set; }
        public IList<Meet> MemberMeets { get; set; }
        public string AlertMessage { get; set; }
    }
}
