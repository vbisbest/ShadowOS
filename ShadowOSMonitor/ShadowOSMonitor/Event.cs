using System;
using Newtonsoft.Json;

namespace ShadowOSMonitor
{
    public class Event
    {
        #region Computed Properties

        public string EventType { get; set; } = "";
        public string Action { get; set; } = "";
        public string Details { get; set; } = "";

        #endregion

        #region Constructors
        public Event()
        {
        }

        public Event(string eventType, string action, string details)
        {
            this.EventType = eventType;
            this.Action = action;
            this.Details = details;
        }
        #endregion
    }

    public class ShadowOSEvent
    {
        [JsonProperty("event_type")]
        public string EventType { get; set; }
        [JsonProperty("data")]
        public EventDetail Data { get; set; }
    }

    public class EventDetail
    {
        [JsonProperty("class")]
        public string Class { get; set; }
        [JsonProperty("action")]
        public string Action { get; set; }
        [JsonProperty("data")]
        public string Data { get; set; }
        [JsonProperty("method")]
        public string Method { get; set; }
        [JsonProperty("headers")]
        public string Headers { get; set; }
        [JsonProperty("scheme")]
        public string Scheme { get; set; }
        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
}