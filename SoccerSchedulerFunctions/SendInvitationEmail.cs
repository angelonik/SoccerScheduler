using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Objects;
using SendGrid.Helpers.Mail;
using System.Text;

namespace SoccerSchedulerFunctions
{
    public static class SendInvitationEmail
    {
        [FunctionName("SendInvitationEmail")]
        public static void Run(
            [QueueTrigger("emails", Connection = "AzureWebJobsStorage")]EmailDetails emailDetails,
            [SendGrid] out Mail email,
            TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {emailDetails}");
            email = new Mail();
            var personalization = new Personalization();
            personalization.AddTo(new Email(emailDetails.Email));
            email.AddPersonalization(personalization);

            var sb = new StringBuilder();
            sb.Append($"Hi {emailDetails.Name},");
            sb.Append($"<p>You're invited to join us on {emailDetails.EventDateAndTime} at {emailDetails.Location}.</p>");
            sb.Append($"<p>Let us know if you can make it by clicking <a href=\"{emailDetails.ResponseUrl}\">here</a></p>");
            log.Info(sb.ToString());

            var messageContent = new Content("text/html", sb.ToString());
            email.AddContent(messageContent);
            email.Subject = "Your invitation...";
            email.From = new Email("mark@soundcode.org");
        }
    }
}
