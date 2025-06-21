using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    /// <summary>
    /// Service interface that manages email sending operations.
    /// Provides email delivery for user verification, password reset, and account deletion processes.
    /// </summary>
    public interface IMailService
    {
        /// <summary>
        /// Sends a general message to the specified email address.
        /// </summary>
        /// <param name="email">Recipient's email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="message">Email content</param>
        void SendEmail(string email, string subject, string message);

        /// <summary>
        /// Sends an email containing a verification code to verify the user account.
        /// </summary>
        /// <param name="email">Recipient's email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="code">Verification code</param>
        /// <returns>True if email sent successfully, false otherwise</returns>
        bool SendVerificationEmail(string email, string subject, string code);

        /// <summary>
        /// Sends an email containing the necessary code for the user to reset their password.
        /// </summary>
        /// <param name="email">Recipient's email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="code">Password reset code</param>
        /// <returns>True if email sent successfully, false otherwise</returns>
        bool SendResetPasswordEmail(string email,string subject,string code);

        /// <summary>
        /// Sends an email containing the necessary code for the user to confirm account deletion.
        /// </summary>
        /// <param name="email">Recipient's email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="code">Account deletion confirmation code</param>
        /// <returns>True if email sent successfully, false otherwise</returns>
        bool SendDeleteCodeEmail(string email, string subject, string code);
    }
}
