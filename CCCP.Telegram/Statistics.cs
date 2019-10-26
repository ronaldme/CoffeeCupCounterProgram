using System.Linq;
using System.Threading.Tasks;
using CCCP.DAL.Repositories;
using CCCP.Telegram.Core;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = CCCP.DAL.Entities.User;

namespace CCCP.Telegram
{
    public static class Statistics
    {
        public static async Task SendUserStats(this TelegramBotClient bot, Message message,
            User userInfo, RegistrationRepository registrationRepository, int savedCups)
        {
            var totalSubmitCount = userInfo.TotalSubmitCount;
            string registrationInfo = null;

            var registrations = registrationRepository.GetRegistrations(userInfo.Id).ToList();

            if (registrations.Any())
            {
                var firstRegistration = registrations.First();
                var lastRegistration = registrations.Last();
                registrationInfo = $"First registration: {firstRegistration.Timestamp}\n" +
                                   $"Last registration: {lastRegistration.Timestamp}";
            }

            await bot.Send(message, $"Total submitted: {totalSubmitCount} of total: {savedCups}.\n\n" +
                                    $"{registrationInfo}",
                false);
        }

        public static async Task SendStatsExtended(this TelegramBotClient bot, Message message,
            RegistrationRepository registrationRepository, int savedCups)
        {
            var firstAndLastRegistration = registrationRepository.GetFirstAndLastRegistrations();

            await bot.Send(message, $"{Helpers.TotalSavedText(savedCups)}\n\n" +
                                    $"Tower size: {Helpers.GetTotalTowerSize(savedCups):#.##} meters.\n" +
                                    $"Total length: {Helpers.GetTotalLength(savedCups):#.##} meters.\n\n" +

                                    $"First registration: {firstAndLastRegistration.Item1.Timestamp}.\n" +
                                    $"Last registration: {firstAndLastRegistration.Item2.Timestamp}.\n",
                false);
        }
    }
}
