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
    }
}