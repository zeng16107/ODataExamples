using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ODataExamples.API.Classes
{
    /// <summary>
    ///   Manage Event Grid operations
    /// </summary>
    /// <returns></returns>
    public class EventHelper{

        /// <summary>
        ///   Handle send to event grid topic
        /// </summary>
        /// <param name="eventTopic"></param>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PublishEventAsync(string eventTopic, object eventData){

            // Create new HttpClient instance to handle call to event grid
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("aeg-sas-key", ConfigurationManager.AppSettings["EventGridSasKey"]);

            // Create new instance of event list
            var gridEvents = new List<GridEvent<object>>();

            // Create and load up event data
            var gridEvent = new GridEvent<object>
            {
                Id = Guid.NewGuid().ToString(),
                Subject = eventTopic,
                EventType = eventTopic,
                EventTime = DateTime.UtcNow,
                Data = eventData
            };

            // Assign event to event array
            gridEvents.Add(gridEvent);

            // Serialize event data to JSON
            var json = JsonConvert.SerializeObject(gridEvents);

            // Prepare request to event grid topic endpoint
            var request = new HttpRequestMessage(HttpMethod.Post, ConfigurationManager.AppSettings["EventGridTopicEndpoint"])
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            // Send event
            return await client.SendAsync(request);
        }
    }

    /// <summary>
    ///   Event data class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class GridEvent<T> where T : class
    {
        public string Id { get; set; }
        public string Subject { get; set; }
        public string EventType { get; set; }
        public T Data { get; set; }
        public DateTime EventTime { get; set; }
    }
}