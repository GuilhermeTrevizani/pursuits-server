using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LesterBOT
{
    class Program
    {
        DiscordSocketClient Client { get; set; }
        ulong GuildId { get; } = 739257198353580112;

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            using (var services = ConfigureServices())
            {
                Client = services.GetRequiredService<DiscordSocketClient>();

                Client.Log += LogAsync;
                Client.UserJoined += Client_UserJoined;
                Client.UserLeft += Client_UserLeft;
                Client.Ready += Client_Ready;
                services.GetRequiredService<CommandService>().Log += LogAsync;

                await Client.LoginAsync(TokenType.Bot, "NzUwNjkxODQxNDUxNzUzNTEz.X0-OQg.ZDhhumvOiaE9gfPZoPy4JLL6oPo");
                await Client.StartAsync();

                await Task.Delay(Timeout.Infinite);
            }
        }

        private async Task Client_Ready()
        {
            await Client.SetGameAsync($"{Client.Guilds.FirstOrDefault(x => x.Id == GuildId)?.Users.Count} jogadores", type: ActivityType.Listening);
        }
        
        private async Task Client_UserLeft(SocketGuildUser arg)
        {
            await Client.SetGameAsync($"{Client.Guilds.FirstOrDefault(x => x.Id == GuildId)?.Users.Count} jogadores", type: ActivityType.Listening);
        }

        private async Task Client_UserJoined(SocketGuildUser arg)
        {
            await Client.SetGameAsync($"{Client.Guilds.FirstOrDefault(x => x.Id == GuildId)?.Users.Count} jogadores", type: ActivityType.Listening);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .BuildServiceProvider();
        }
    }
}