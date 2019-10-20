using CCCP.DAL.Entities;

namespace CCCP.DAL.Repositories
{
    public class UserRepository : RepositoryBase<User>
    {
        public User GetUser(long chatId)
        {
            using (var db = Create())
            {
                return Collection(db).FindOne(c => c.ChatId == chatId);
            }
        }

        public void CreateUser(string name, long chatId)
        {
            using (var db = Create())
            {
                Collection(db).Insert(new User
                {
                    Name = name,
                    ChatId = chatId,
                });
            }
        }

        public void UpdateCount(long chatId, int nrOfCups)
        {
            using (var db = Create())
            {
                var user = GetUser(chatId);
                user.NumberOfCupSubmitted = user.NumberOfCupSubmitted += nrOfCups;

                Collection(db).Update(user);
            }
        }
    }
}