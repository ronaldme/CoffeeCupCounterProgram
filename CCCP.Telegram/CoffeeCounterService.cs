using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

namespace CCCP.Telegram
{
    public class CoffeeCounterService
    {
        private static readonly ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static TelegramBotClient bot;
        private int _savedCups;
        private float plasticPerCupInGram = 2.8f;

        private string path = ConfigurationManager.AppSettings["infoFile"];

        public CoffeeCounterService()
        {
            if (File.Exists(path))
            {
                string readText = File.ReadAllText(path);
                _savedCups = int.Parse(readText);
            }
            else File.WriteAllText(path, "0");
        }

        public void Start()
        {
            bot = new TelegramBotClient(ConfigurationManager.AppSettings["telegramToken"]);
            bot.OnMessage += BotOnMessageReceived;

            bot.StartReceiving(Array.Empty<UpdateType>());

            _log.Info($"{nameof(CoffeeCounterService)} started");
        }

        public void Stop() { }

        private async void BotOnMessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            if (message == null || message.Type != MessageType.Text) return;

            _log.Info($"Received: {e.Message.Text}");

            var text = message.Text.Split(' ').First().ToLower();
            var isParsed = int.TryParse(text, out int nrOfCups);

            if (isParsed)
            {
                HandleCupsSaved(nrOfCups, message).GetAwaiter().GetResult();
                return;
            }

            switch (text)
            {
                case "/start":
                    await SendDefault(message, "Cups saved");
                    break;
                case "/undo":
                    await bot.SendTextMessageAsync(message.Chat.Id,
                        "Cups", replyMarkup: Keyboards.Undo);
                    break;
                default:
                    await SendDefault(message, "/start - Start saving\n" +
                                               "/undo - Enter number of cups to undo");
                    break;
            }
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

            File.WriteAllText(path, _savedCups.ToString());
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