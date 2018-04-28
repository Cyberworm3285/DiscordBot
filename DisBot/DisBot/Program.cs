using System;
using System.Linq;

using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

using Discord.WebSocket;
using Discord.Commands;
using Discord;

using DisBot.Memes;

namespace DisBot
{
    public class Program
    {
        private DiscordSocketClient client;
        private CommandService commands;
        private IServiceProvider services;

        static void Main(string[] args) => new Program()
            .Start()
            .GetAwaiter()
            .GetResult();

        public async Task Start()
        {
            Looter.Init();

            string token = Config.Current.Token;

            client = new DiscordSocketClient();

            await client.SetGameAsync("mit deiner Mudda");

            commands = new CommandService();

            services = new ServiceCollection()
                    .AddSingleton(client)
                    .AddSingleton(commands)
                    .BuildServiceProvider();

            await InstallCommands();

            client.Log += Log;

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        public async Task InstallCommands()
        {
            client.MessageReceived += HandleCommand;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public Task Log(LogMessage m)
        {
            Console.WriteLine(m);
            return Task.CompletedTask;
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;


            int argPos = 0;
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))) return;
            var context = new CommandContext(client, message);

            if (Config.Current.IsBanned(context.User.Id))
                return;

            var result = await commands.ExecuteAsync(context, argPos, services);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync($"Da würd ich an deiner Stelle nochma drüber gucken ({result.ErrorReason})");
        }
    }

}