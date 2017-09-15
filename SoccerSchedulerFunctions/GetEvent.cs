using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using Objects;
using Newtonsoft.Json;

namespace SoccerSchedulerFunctions
{
    public static class GetEvent
    {
        [FunctionName("GetEvent")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = "event/{id}")]HttpRequestMessage req,
            [Table("events", "event", "{id}")] EventTableEntity @event,
            string id,
            TraceWriter log)
        {
            if (@event == null)
            {
                log.Warning("failed to find ");
            }

            var responses = JsonConvert.DeserializeObject<Response[]>
                (@event.ResponsesJson);

            var responseCode = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => q.Key == "responseCode")
                .Value;

            var matchedResponse = responses
                .FirstOrDefault(response => response.ResponseCode == responseCode);

            if (matchedResponse != null)
            {
                return req.CreateResponse(HttpStatusCode.OK, new
                {
                    EventDateAndTime = @event.EventDateAndTime,
                    Location = @event.Location,
                    MyResponse = matchedResponse.IsPlaying,
                    Responses = responses.Select(response => new
                    {
                        Name = response.Name,
                        Playing = response.IsPlaying
                    })
                });
            }

            return req.CreateResponse(HttpStatusCode.NotFound, "Invalid response code");
        }
    }
}
