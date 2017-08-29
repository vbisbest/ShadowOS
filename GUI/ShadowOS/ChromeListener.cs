using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ShadowOS
{
    public class ChromeListener
    {
        List<WebSocket> webSockets = new List<WebSocket>();
        public delegate void ChromeEventHandler(ChromeNetworkEvent networkEvent);
        public event ChromeEventHandler ChromeEvent;

        public ChromeListener()
        {
        }

        public void Connect(int port)
        {
            string uri = "http://localhost:" + port.ToString();

            try
            {
                Chrome c = new Chrome(uri);
                var sessions = c.GetAvailableSessions();
                Console.WriteLine("Available debugging sessions");
                foreach (var s in sessions)
                {
                    EstablishSession(s.webSocketDebuggerUrl);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void EstablishSession(string url)
        {
            WebSocket webSocket = new WebSocket(url);
            webSocket.MessageReceived += webSocket_MessageReceived;
            webSocket.Opened += webSocket_Opened;
            webSocket.Error += webSocket_Error;
            webSocket.Closed += webSocket_Closed;
            webSocket.AutoSendPingInterval = 5000;
            webSocket.EnableAutoSendPing = true;

            // Add to our collection
            webSockets.Add(webSocket);

            webSocket.Open();

        }

        void webSocket_Closed(object sender, EventArgs e)
        {
            WebSocket webSocket = (WebSocket)sender;
            webSockets.Remove(webSocket);

        }

        void webSocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            //throw new NotImplementedException();
            // Ignore socket errors
        }

        void webSocket_Opened(object sender, EventArgs e)
        {
            string enableNetwork = "{\"id\":1,\"method\": \"Network.enable\"}";
            WebSocket webSocket = (WebSocket)sender;
            webSocket.Send(enableNetwork);
        }

        void webSocket_MessageReceived(object sender, WebSocket4Net.MessageReceivedEventArgs e)
        {
            if (ChromeEvent != null)
            {
                ChromeNetworkEvent networkEvent = JsonConvert.DeserializeObject<ChromeNetworkEvent>(e.Message);
                if (networkEvent.method == "Network.requestWillBeSent")
                {
                    ChromeEvent(networkEvent);
                }
            }
        }
    }

    public class ChromeNetworkEvent
    {
        public string method { get; set; }

        [JsonProperty(PropertyName = "params")]
        public ChromeParams parameters { get; set;}
    }

    public class ChromeParams
    {
        public string documentURL {get; set;}
        public ChromeNetworkRequest request {get; set;}
    }

    public class ChromeNetworkRequest
    {
        public string url {get; set;}
        public string method { get; set; }
        public dynamic headers { get; set; }
        public string postData { get; set; }
        public string HeaderString
        {
            get
            {
                StringBuilder h = new StringBuilder();
                foreach (JProperty current in headers)
                {  
                    h.AppendLine(String.Format("{0}: {1}", current.Name.ToString(), current.Value.ToString()));
                }

                return h.ToString();
            }
        }

        public Uri uri
        {
            get
            {
                if (url.Length > 1000)
                {
                    return new Uri(url.Substring(0, 1000));
                }
                else
                {
                    return new Uri(url);
                }
            }
        }
    }

}
