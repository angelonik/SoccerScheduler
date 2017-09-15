using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Objects;
using Newtonsoft.Json;

namespace SoccerSchedulerFunctions
{
    public static class UpdateResponse
    {
        [FunctionName("UpdateResponse")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put",
                Route = "event/{id}/response/{responseCode}")]HttpRequestMessage req,
            [Table("events", Connection = "AzureWebJobsStorage")]CloudTable eventsTable,
            string id,
            string responseCode,
            TraceWriter log)
        {
            log.Info($"Updating response {responseCode} for event {id}");

            var operation = TableOperation.Retrieve<EventTableEntity>("event", id);
            var executionResult = eventsTable.Execute(operation);

            var @event = executionResult.Result as EventTableEntity;
            if(@event == null)
            {
                log.Warning($"failed to find event {id}");
                return req.CreateResponse(HttpStatusCode.NotFound, "Invalid event");
            }

            log.Info("deserializing");
            var responses = JsonConvert.DeserializeObject<Response[]>(@event.ResponsesJson);
            var responseToUpdate = responses
                .FirstOrDefault(r => r.ResponseCode == responseCode);

            if(responseCode == null)
            {
                log.Warning($"failed to find response {responseCode}");
                return req.CreateResponse(HttpStatusCode.NotFound, "Invalid event");
            }
            log.Info("Getting body");

            var response = await req.Content.ReadAsAsync<Response>();
            responseToUpdate.IsPlaying = response.IsPlaying;
            @event.ResponsesJson = JsonConvert.SerializeObject(responses);

            operation = TableOperation.Replace(@event);
            eventsTable.Execute(operation);

            return req.CreateResponse(HttpStatusCode.OK, "Updated successfully");
        }
    }
}
