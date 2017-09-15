using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using Objects;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CreateEvent
{
    public static class CreateEvent
    {
        [FunctionName("CreateEvent")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req,
            [Queue("emails", Connection = "AzureWebJobsStorage")] IAsyncCollector<EmailDetails> emailsQueue,
            [Table("events", Connection = "AzureWebJobsStorage")] IAsyncCollector<EventTableEntity> eventsTable,
            TraceWriter log)
        {
            var eventDetails = await req.Content.ReadAsAsync<EventDetails>();
            var responses = new List<Response>();
            var eventId = Guid.NewGuid().ToString("n");

            foreach (var invitee in eventDetails.Invitees)
            {
                log.Info($"Inviting {invitee.Name} ({invitee.Email})");
                var accessCode = Guid.NewGuid().ToString("n");
                var emailDetails = new EmailDetails
                {
                    EventDateAndTime = eventDetails.EventDateAndTime,
                    Location = eventDetails.Location,
                    Name = invitee.Name,
                    Email = invitee.Email,
                    ResponseUrl = $"http://127.0.0.1:10000/devstoreaccount1/web/index.html?" +
                        $"event={eventId}&code={accessCode}"
                };

                await emailsQueue.AddAsync(emailDetails);
                responses.Add(new Response
                {
                    Name = invitee.Name,
                    Email = invitee.Email,
                    IsPlaying = "unknown",
                    ResponseCode = accessCode
                });
            }

            await eventsTable.AddAsync(new EventTableEntity
            {
                PartitionKey = "event",
                RowKey = eventId,
                EventDateAndTime = eventDetails.EventDateAndTime,
                Location = eventDetails.Location,
                ResponsesJson = JsonConvert.SerializeObject(responses)
            });

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
