using System;
using CCCP.DAL.Entities;

namespace CCCP.DAL.Repositories
{
    public class UserRepository : RepositoryBase<User>
    {
        public User GetUser(long chatId)
        {
            using var db = Create();
            return Collection(db).FindOne(c => c.ChatId == chatId);
        }

        public void CreateUser(string name, long chatId)
        {
            using var db = Create();
            Collection(db).Insert(new User
            {
                Name = name,
                ChatId = chatId,
            });
        }

        public void UpdateCount(long chatId, int nrOfCups)
        {
            using var db = Create();
            var user = GetUser(chatId);
            user.TotalSubmitCount = user.TotalSubmitCount += nrOfCups;

            var registrations = db.GetCollection<Registration>();
            registrations.Insert(new Registration
            {
                SubmitCount = nrOfCups,
                Timestamp = DateTime.Now,
                UserId = user.Id,
            });
                
            Collection(db).Update(user);
        }
    }
}