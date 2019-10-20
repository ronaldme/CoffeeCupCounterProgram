using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using CCCP.DAL.Repositories;
using log4net;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CCCP.Telegram
{
    public class CoffeeCounterService
    {
        private Dictionary<long, string> Users { get; } = new Dictionary<long, string>();

        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static TelegramBotClient bot;
        private int _savedCups;
        private float plasticPerCupInGram = 2.8f;
        
        public CoffeeCounterService()
        {
            var count = new CoffeeCupRepository();
            _savedCups = count.GetTotalSavedCups();

            _log.Info($"Initial startup saved cups: {_savedCups}");
        }

        public void Start()
        {
            bot = new TelegramBotClient(ConfigurationManager.AppSettings["telegramToken"]);
            bot.OnMessage += BotOnMessageReceived;

            bot.StartReceiving(Array.Empty<UpdateType>());

            _log.Info($"{nameof(CoffeeCounterService)} started");
        }

        public void Stop() => _log.Info($"{nameof(CoffeeCounterService)} stopped");

        private async void BotOnMessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            if (message == null || message.Type != MessageType.Text) return;

            _log.Info($"Received: {e.Message.Text}");

            var userRepo = new UserRepository();
            var user = userRepo.GetUser(message.Chat.Id);

            if (user == null)
            {
                await HandleUnknownUser(e, message);
                return;
            }

            var text = message.Text.Split(' ').First().ToLower();
            var isParsed = int.TryParse(text, out int nrOfCups);

            if (isParsed)
            {
                HandleCupsSaved(nrOfCups, message).GetAwaiter().GetResult();
                return;
            }

            await HandleOptions(text, message, user.IsAdmin);
        }

        private async Task HandleOptions(string text, Message message, bool isAdmin)
        {
            if (isAdmin) AdminOptions(text, message);

            switch (text)
            {
                case "/start":
                    await SendDefault(message, "Enter number of cups saved");
                    break;
                case "/users" when isAdmin:
                {
                    foreach (var user in Users)
                    {
                        ReplyKeyboardMarkup kb = new[]
                        {
                            new[] { $"accept-{user.Key}" },
                            new[] { $"reject-{user.Key}" },
                        };

                        await bot.SendTextMessageAsync(
                            message.Chat.Id,
                            $"Authenticate: {user.Value}",
                            replyMarkup: kb);
                        break;
                    }
                    await SendDefault(message, "No users to authenticate");
                    break;
                }
                case "stats":
                case "/stats":
                    await SendDefault(message, $"Total saved cups: {_savedCups} \n" +
                                               $"Total saved plastic: {plasticPerCupInGram * _savedCups}");
                    break;
                case "/undo":
                    await bot.SendTextMessageAsync(message.Chat.Id,
                        "Cups", replyMarkup: Keyboards.Undo);
                    break;
                default:
                    await SendDefault(message, "/start - Start saving\n" +
                                               "/undo - Enter number of cups to undo" +
                                               (isAdmin ? "\n/users - show users to authenticate" : null));
                    break;
            }
        }

        private void AdminOptions(string text, Message message)
        {
            if (text.StartsWith("accept"))
            {
                var chatId = GetChatId("accept", text);

                var repo = new UserRepository();
                repo.CreateUser(Users[chatId], message.Chat.Id);

                Users.Remove(chatId);
            }

            if (text.StartsWith("reject")) Users.Remove(GetChatId("reject", text));
        }

        private long GetChatId(string remove, string text) =>
            Convert.ToInt64(text.Substring(remove.Length + 1));

        private async Task HandleUnknownUser(MessageEventArgs e, Message message)
        {
            _log.Info($"Unknown user send a message. User: {message.Chat.FirstName}, ChatId: {message.Chat.Id}");

            if (e.Message.Text == "/authenticate")
            {
                await bot.SendTextMessageAsync(message.Chat.Id, "Please await your verification.\n\n");
                await Task.Delay(2500);
                await bot.SendTextMessageAsync(message.Chat.Id,
                    "In the meantime please listen to some relaxing music: https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                    replyMarkup: new ReplyKeyboardRemove());

                if (!Users.ContainsKey(message.Chat.Id)) Users.Add(message.Chat.Id, message.Chat.FirstName);
            }
            else await bot.SendTextMessageAsync(message.Chat.Id, "Please authenticate with the admin first.\n\n /authenticate");
        }

        private async Task HandleCupsSaved(int nrOfCups, Message message)
        {
            _savedCups += nrOfCups;

            if (nrOfCups > 0)
            {
                await SendDefault(message,
                    $"Thanks for saving {nrOfCups} {(nrOfCups > 1 ? "cups" : "cup")}, which is equal to {plasticPerCupInGram * nrOfCups} grams of plastic. \n\n" +
                    $"Total saved cups: {_savedCups} \n" +
                    $"Total saved plastic: {plasticPerCupInGram * _savedCups} grams");
            }
            else
            {
                await SendDefault(message,
                    $"{nrOfCups} undo done.\n\n" +
                    $"Total saved cups: {_savedCups} \n" +
                    $"Total saved plastic: {plasticPerCupInGram * _savedCups}\n\n" +
                    "/start");
            }

            var ccr = new CoffeeCupRepository();
            ccr.UpdateCoffeeCupsSaved(_savedCups);

            var user = new UserRepository();
            user.UpdateCount(message.Chat.Id, nrOfCups);
        }

        private async Task SendDefault(Message message, string text)
        {
            await bot.SendTextMessageAsync(
                message.Chat.Id,
                text,
                replyMarkup: Keyboards.Default);
        }
    }
}