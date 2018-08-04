using System;
using System.Linq;

using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

using Discord.WebSocket;
using Discord.Commands;
using Discord;

using DisBot.Memes;
using DisBot.Modules.Fallback;
using System.Collections.Generic;

namespace DisBot
{
    public class Program
    {
        private DiscordSocketClient client;
        private CommandService commands;
        private IServiceProvider services;
        private ModuleFallback fallback;

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

            fallback = new ModuleFallback()
            {
                new ReactionMeme("", false, x => x.ToUpper().Contains("HANS"), (x,cc) => ":fire: Get Ze Flammenwerfer :fire:"),
                new ReactionMeme("https://i.redd.it/oz4ds1ecg5r01.gif", false, x => x.ToUpper().Contains("KOMMSTE RAN"), (x, cc) => ""),
                new ReactionMeme("", true, x => x.ToUpper().Contains("MARCO"), (x, cc) => "Polo"),
                new ReactionMeme("", true, x => x.ToUpper().Contains("CANCER"), (x, cc) => "CANCER"),
            };

            services = new ServiceCollection()
                    .AddSingleton(client)
                    .AddSingleton(commands)
                    .AddSingleton(fallback)
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
            if (messageParam.Author.Id == client.CurrentUser.Id)
                return;

            var message = messageParam as SocketUserMessage;
            if (message == null) return;


            int argPos = 0;
            var context = new CommandContext(client, message);
            if (Config.Current.IsBanned(context.User.Id))
                return;


            if ((message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos)))
            {
                var result = await commands.ExecuteAsync(context, argPos, services);
                if (!result.IsSuccess)
                    await context.Channel.SendMessageAsync($"Da würd ich an deiner Stelle nochma drüber gucken ({result.ErrorReason})");
            }
            else
            {
                await fallback.TryFallback(message.Content, context);
            }
        }
    }

}