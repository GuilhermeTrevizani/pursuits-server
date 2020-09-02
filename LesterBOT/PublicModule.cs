using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LesterBOT
{
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        [Command("online")]
        [Alias("on")]
        public Task OnlineAsync()
        {
            try
            {
                var txt = new WebClient().DownloadString("http://api.altv.mp/servers/list");
                var servers = JsonConvert.DeserializeObject<List<ServerALTV>>(txt);
                var players = servers.FirstOrDefault(x => x.host == "191.252.157.190")?.players ?? 0;
                return ReplyAsync($"O servidor está com {players} jogador{(players != 1 ? "es" : string.Empty)} online!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(JsonConvert.SerializeObject(ex));
                return ReplyAsync("Não consegui recuperar as informações da minha base de dados. A culpa é do sistema!");
            }
        }
    }
}