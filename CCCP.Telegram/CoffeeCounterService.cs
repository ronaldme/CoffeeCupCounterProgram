using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using CCCP.DAL.Repositories;
using CCCP.Telegram.Core;
using log4net;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using User = CCCP.DAL.Entities.User;

namespace CCCP.Telegram
{
    public class CoffeeCounterService
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string _postUrl = ConfigurationManager.AppSettings["postUrl"];
        private static TelegramBotClient bot;
        private int _savedCups;
        private readonly CoffeeCupRepository _coffeeCupRepository = new CoffeeCupRepository();
        private readonly UserRepository _userRepository = new UserRepository();
        private readonly RegistrationRepository _registrationRepository = new RegistrationRepository();
        private readonly AchievementService _achievementService = new AchievementService();

        private Dictionary<long, string> Users { get; } = new Dictionary<long, string>();

        public CoffeeCounterService()
        {
            _savedCups = _coffeeCupRepository.GetTotalSavedCups();

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

            try
            {
                _log.Info($"Received: {e.Message.Text}");

                var user = _userRepository.GetUser(message.Chat.Id);

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

                await HandleOptions(text, message, user);
            }
            catch (Exception exception)
            {
                _log.Error($"Could not handle message {message.Text}", exception);
            }
        }

        private async Task HandleOptions(string text, Message message, User userInfo)
        {
            var isAdmin = userInfo.IsAdmin;
            if (isAdmin) AdminOptions(text, message);

            switch (text)
            {
                case "start":
                case "/start":
                    await bot.Send(message, "Enter number of cups saved");
                    break;
                case "users" when isAdmin:
                case "/users" when isAdmin:
                {
                    foreach (var user in Users)
                    {
                        await bot.SendTextMessageAsync(message.Chat.Id,
                            $"Authenticate: {user.Value}",
                            replyMarkup: Keyboards.AuthenticationKb(user.Key));
                        return;
                    }
                    await bot.Send(message, "No users to authenticate");
                    break;
                }
                case "stats":
                case "/stats":
                    await bot.Send(message, Helpers.TotalSavedText(_savedCups));
                    break;
                case "stats_extended":
                case "/stats_extended":
                    await bot.SendStatsExtended(message, _registrationRepository, _savedCups);
                    break;
                case "user_stats":
                case "/user_stats":
                    await bot.SendUserStats(message, userInfo, _registrationRepository, _savedCups);
                    break;
                case "undo":
                case "/undo":
                    await bot.SendTextMessageAsync(message.Chat.Id,
                        "Cups", replyMarkup: Keyboards.Undo);
                    break;
                default:
                    await bot.Send(message, "/start - Start saving\n" +
                                               "/undo - Enter number of cups to undo\n" +
                                               "/stats - Get basic statistics\n" +
                                               "/stats_extended - Get extended statistics!\n" +
                                               "/user_stats - See your own stats\n" +
                                               (isAdmin ? "\n/users - show users to authenticate" : null));
                    break;
            }
        }

        private void AdminOptions(string text, Message message)
        {
            if (text.StartsWith("accept"))
            {
                var chatId = GetChatId("accept", text);
                _userRepository.CreateUser(Users[chatId], message.Chat.Id);

                Users.Remove(chatId);
            }

            if (text.StartsWith("reject")) Users.Remove(GetChatId("reject", text));
        }

        private long GetChatId(string remove, string text) =>
            Convert.ToInt64(text.Substring(remove.Length + 1));

        private async Task HandleUnknownUser(MessageEventArgs e, Message message)
        {
            _log.Info($"Unknown user send coffeeCupRepository message. User: {message.Chat.FirstName}, ChatId: {message.Chat.Id}");

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
                var achievement = _achievementService.GetAchievement(_savedCups);
                if (achievement != null) await bot.Send(message, $"Unlocked achievement: {achievement}");

                await bot.Send(message,
                    $"Thanks for saving {nrOfCups} {(nrOfCups > 1 ? "cups" : "cup")} " +
                    $"({Constants.PlasticPerCupInGram * nrOfCups} grams of plastic) \n\n" +
                    $"{Helpers.TotalSavedText(_savedCups)}");
            }
            else if (nrOfCups < 0)
                await bot.Send(message, $"{nrOfCups} undo done.\n\n" +
                                        $"{Helpers.TotalSavedText(_savedCups)}");

            _coffeeCupRepository.UpdateCoffeeCupsSaved(_savedCups);
            _userRepository.UpdateCount(message.Chat.Id, nrOfCups);
            _userRepository.UpdateCount(message.Chat.Id, nrOfCups);

            if (_postUrl != null) PostHelper.PostCount(_postUrl, _savedCups);
        }
    }
}