using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetApp.Web.Models
{
    public enum ResultCode
    {
        OK,
        Null,
        Launched,
        AlreadyExist,
        NotFound
    }

    public class StatusResult
    {
        public string Text { get; set; }

        public ResultCode Code { get; set; }

        public StatusResult(string text, ResultCode code)
        {
            Text = text;
            Code = code;
        }
    }
}
