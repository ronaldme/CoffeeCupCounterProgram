using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CCCP.Telegram.Core
{
    public static class BotHelpers
    {
        public static async Task Send(this TelegramBotClient bot, Message message, string text, bool keyboard = true)
        {
            await bot.SendTextMessageAsync(
                message.Chat.Id,
                text,
                replyMarkup: keyboard ? Keyboards.Default : null);
        }
    }
}