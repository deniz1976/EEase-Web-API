using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IMailService
    {
        void SendEmail(string email, string subject, string message);

        bool SendVerificationEmail(string email, string subject, string code);

        bool SendResetPasswordEmail(string email,string subject,string code);

        bool SendDeleteCodeEmail(string email, string subject, string code);
    }
}
