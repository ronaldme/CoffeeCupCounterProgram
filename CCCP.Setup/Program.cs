﻿using System.Collections.Generic;
using CCCP.DAL.Repositories;

namespace CCCP.Setup
{
    class Program
    {
        private static readonly Dictionary<int, string> _achivements = new Dictionary<int, string>
        {
            {1, "First one!" },
            {100, "100 saved cups" },
            {550, "Five-five-five, well done!" },
            {600, "600 cups!" },
            {666, "Devilishly!" },
            {700, "700 cups!" },
            {800, "800 cups!" },
            {900, "900 cups!" },
            {1000, "Nice, one thousand!" },
            {9001, "Its over 9000!" },
        };

        static void Main(string[] args)
        {
            var archievementRepository = new AchievementRepository();

            foreach (var entry in _achivements)
                archievementRepository.Create(entry.Key, entry.Value);
        }
    }
}
