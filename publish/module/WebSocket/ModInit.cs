using Shared;
using Shared.Engine;
using Shared.Models.Module;
using System;
using System.Text.Json.Nodes;

namespace WebSocket
{
    public class ModInit
    {
        public static void loaded(InitspaceModel conf)
        {
            conf.nws.EventsAsync(string.Empty, "user_id", "msg", "Hello World");


            EventListener.RchRegistry += async e =>
            {
                var rch = new RchClient(e.connectionId);
                await rch.Eval("Lampa.Noty.show('Hello WebSocket')");

                var info = rch.InfoConnected();
                info.obs["account"] = await rch.Eval<JsonNode>("Lampa.Storage.get('account', '{}')");
            };


            EventListener.NwsMessage += e =>
            {
                if (e.method == "whoami")
                {
                    Console.WriteLine($"{e.method} | {e.payload}");
                    conf.nws.SendAsync(e.connectionId, "whoareyou", "I don't know");
                }
            };

            /*
var client = window.nwsClient['127.0.0.1:9118']

client.on('whoareyou', function(message) {
  console.log('nws', message)
});

client.invoke("whoami", Lampa.Storage.get('lampac_unic_id', '')); 
            */
        }
    }
}
