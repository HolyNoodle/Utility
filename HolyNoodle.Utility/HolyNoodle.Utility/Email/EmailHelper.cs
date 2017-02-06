using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace HolyNoodle.Utility.Email
{
    public static class EmailHelper
    {
        public static string Host { get; private set; }
        public static int Port { get; private set; }
        public static string Login { get; private set; }
        public static string Password { get; private set; }

        public static void Init(string host, int port, string login, string password)
        {
            Host = host;
            Port = port;
            Login = login;
            Password = password;
        }

        public static void SendEmail(EmailModel model)
        {
            using (var smtpClient = new SmtpClient(Host, Port))
            {
                smtpClient.EnableSsl = true;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = true;
                smtpClient.Credentials = new NetworkCredential(Login, Password);

                model.From = model.From ?? new MailAddress(Host);

                foreach (var property in model.GetType().GetProperties())
                {
                    model.Body = model.Body.Replace("#=" + property.Name + "#", property.GetValue(model).ToString());
                }
                
                var message = new MailMessage(model.From, model.To)
                {
                    Subject = model.Title,
                    Body = model.Body,
                    IsBodyHtml = true
                };

                for (var i = 0; i < model.Attachments.Count(); i++)
                {
                    message.Attachments.Add(new Attachment(model.Attachments[i]));
                }

                smtpClient.Send(message);
            }
        }
    }
}
