using System.Configuration;
using LiteDB;

namespace CCCP.DAL.Repositories
{
    public class RepositoryBase<T>
    {
        public string ConnectionString = ConfigurationManager.ConnectionStrings["database"].ConnectionString;

        public LiteDatabase Create() => new LiteDatabase(ConnectionString);

        public LiteCollection<T> Collection(LiteDatabase db) => db.GetCollection<T>();
    }
}