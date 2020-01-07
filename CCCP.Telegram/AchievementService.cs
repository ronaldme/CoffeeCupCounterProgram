using System.Collections.Generic;

namespace CCCP.Telegram
{
    public class AchievementService
    {
        private Dictionary<int, string> _achivements = new Dictionary<int, string>
        {
            {1, "First one!" },
            {100, "100 saved cups" },
            {550, "Five-five-five, well done!" },
            {666, "Devilishly good!" },
            {1000, "Nice, one thousand!" },
            {9001, "Its over 9000!" },
        };

        public string GetAchievement(int saved)
        {
            _achivements.TryGetValue(saved, out string value);
            return value;
        }
    }
}