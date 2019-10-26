using Telegram.Bot.Types.ReplyMarkups;

namespace CCCP.Telegram.Core
{
    public static class Keyboards
    {
        public static readonly ReplyKeyboardMarkup Default = new[]
        {
            new[] { "1", "2", "3" },
            new[] { "4", "5", "6" },
        };

        public static readonly ReplyKeyboardMarkup Undo = new[]
        {
            new[] { "-1", "-2", "-3" },
            new[] { "-4", "-5", "-6" },
        };

        public static ReplyKeyboardMarkup AuthenticationKb(long key)
        {
            return new[]
            {
                new[] { $"accept-{key}" },
                new[] { $"reject-{key}" },
            };
        }
    }
}