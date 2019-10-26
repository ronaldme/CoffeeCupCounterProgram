namespace CCCP.Telegram.Core
{
    internal static class Helpers
    {
        /// <summary>
        /// Total size when placing all cups into each other.
        /// </summary>
        public static float GetTotalTowerSize(int nrOfCups) => (float)(Constants.CupSizeInMm + Constants.IncreasementSizeInMm *
                                                              (nrOfCups - 1)) / 1000;

        /// <summary>
        /// Total length when placing all cups next to each other.
        /// </summary>
        public static float GetTotalLength(int nrOfCups) => (float)(nrOfCups * Constants.CupSizeInMm) / 1000;

        public static string TotalSavedText(int savedCups) => $"Total saved cups: {savedCups} \n" +
                                                              $"Total saved plastic: {Constants.PlasticPerCupInGram * savedCups} grams";
    }
}