using System.Linq;
using CCCP.DAL.Entities;
using LiteDB;

namespace CCCP.DAL.Repositories
{
    public class CoffeeCupRepository : RepositoryBase<CoffeeCups>
    {
        public int GetTotalSavedCups()
        {
            using (var db = Create())
            {
                var cc = Get(db);
                return cc.TotalSaved;
            }
        }

        public void UpdateCoffeeCupsSaved(int totalSaved)
        {
            using (var db = Create())
            {
                var cc = Get(db);
                cc.TotalSaved = totalSaved;

                Collection(db).Update(cc);
            }
        }

        private CoffeeCups Get(LiteDatabase db)
        {
            var all = Collection(db).FindAll().ToList();
            var coffeeCups = all.ToList();

            return !coffeeCups.Any() ? 
                CreateCoffeeCups(Collection(db)) :
                coffeeCups.Single();
        }

        private CoffeeCups CreateCoffeeCups(LiteCollection<CoffeeCups> collection)
        {
            var cc = new CoffeeCups();
            collection.Insert(cc);

            return cc;
        }
    }
} 