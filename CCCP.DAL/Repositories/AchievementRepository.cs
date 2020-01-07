using CCCP.DAL.Entities;

namespace CCCP.DAL.Repositories
{
    public class AchievementRepository : RepositoryBase<Achievement>
    {
        public Achievement GetAchievement(int savedCups)
        {
            using var db = Create();
            return Collection(db).FindOne(c => c.SavedCups == savedCups);
        }

        public void Create(int savedCups, string message)
        {
            using var db = Create();
            Collection(db).Insert(new Achievement
            {
                SavedCups = savedCups,
                Message = message,
            });
        }
    }
}