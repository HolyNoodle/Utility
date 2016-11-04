using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace HolyNoodle.Utility.Email
{
    public class EmailModel
    {
        public EmailModel()
        {
            Attachments = new List<string>();
        }
        public MailAddress To { get; set; }
        public string Title { get; set; }
        public MailAddress From { get; set; }
        public string Body { get; set; }
        public List<string> Attachments { get; set; }
    }
}
