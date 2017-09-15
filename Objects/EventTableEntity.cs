using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Objects
{
    public class EventTableEntity : TableEntity
    {
        public DateTime EventDateAndTime { get; set; }
        public string Location { get; set; }
        public string ResponsesJson { get; set; }
    }
}
