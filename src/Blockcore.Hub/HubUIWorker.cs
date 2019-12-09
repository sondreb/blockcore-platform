using Blockcore.Platform.Networking;
using Blockcore.Platform.Networking.Actions;
using Blockcore.Platform.Networking.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebWindows;

namespace Blockcore.Hub
{
    /// <summary>
    /// Hub UI worker ensures that the UI window is created and forwards events to and from the window.
    /// </summary>
    public class HubUIWorker : BackgroundService
    {
        private readonly ILogger<HubUIWorker> log;
        private readonly HubManager manager;
        private readonly PubSub.Hub hub = PubSub.Hub.Default;
        private readonly IEnumerable<IEvent> events;
        private readonly IEnumerable<IAction> actions;
        private WebWindow window;
        private Dictionary<string, Type> actionMap;
        private MethodInfo publishMethod;

        public HubUIWorker(ILogger<HubUIWorker> log, HubManager manager, IEnumerable<IEvent> events, IEnumerable<IAction> actions)
        {
            this.log = log;
            this.manager = manager;
            this.events = events;
            this.actions = actions;
            this.actionMap = new Dictionary<string, Type>();
            
            var options = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

            // Register event handler for any and all IEvent that happens and forward them to the UI.
            hub.Subscribe<IEvent>(this, (e) =>
            {
                try
                {
                    var uiEvent = new UIEvent();
                    uiEvent.Type = e.GetType().Name;
                    uiEvent.Body = JsonSerializer.Serialize(e, e.GetType(), options);
                    window.SendMessage(JsonSerializer.Serialize(uiEvent, options));
                }
                catch (Exception ex)
                {
                    log.LogError("Failed to serialize and send UI event.", ex);
                }
            });

            // Make a map of all the actions for quicker lookup based on string key.
            foreach (var action in actions)
            {
                var type = action.GetType();
                actionMap.Add(type.Name, type);
            }

            // Cache this for performance, verify if actually does any difference.
            publishMethod = hub.GetType().GetMethod("Publish");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            window = new WebWindow("Blockcore Hub");
            window.OnWebMessageReceived += OnWebMessageReceived;
            window.NavigateToLocalFile("wwwroot/index.html");
            window.WaitForExit();
        }

        private void OnWebMessageReceived(object sender, string e)
        {
            var @event = JsonSerializer.Deserialize<UIEvent>(e);

            if (!actionMap.ContainsKey(@event.Type))
            {
                throw new ApplicationException("The action is not supported.");
            }

            var actionType = actionMap[@event.Type];

            if (@event.Body == null)
            {
                @event.Body = "{}";
            }

            var action = (IAction)JsonSerializer.Deserialize(@event.Body, actionType);
            
            // We must call the generic one or else the handlers won't be raised.
            publishMethod.MakeGenericMethod(actionType).Invoke(hub, new object[1] { action });
        }
    }
}
