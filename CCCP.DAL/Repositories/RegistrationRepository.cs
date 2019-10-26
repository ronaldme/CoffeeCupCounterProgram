using System;
using System.Collections.Generic;
using System.Linq;
using CCCP.DAL.Entities;

namespace CCCP.DAL.Repositories
{
    public class RegistrationRepository : RepositoryBase<Registration>
    {
        public IEnumerable<Registration> GetRegistrations(int userId)
        {
            using (var db = Create())
                return Collection(db).Find(c => c.UserId == userId);
        }

        public Tuple<Registration, Registration> GetFirstAndLastRegistrations()
        {
            using (var db = Create())
            {
                var registrations = Collection(db).FindAll().ToList();
                if (!registrations.Any()) return null;

                var first = registrations.First();
                var last = registrations.Last();

                return new Tuple<Registration, Registration>(first, last);
            }
        }
    }
}