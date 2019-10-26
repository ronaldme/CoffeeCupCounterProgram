using System.Collections.Generic;

namespace CCCP.DAL.Entities
{
    public class User
    {
        public int Id { get; set; }
        public bool IsAdmin { get; set; }
        public string Name { get; set; }
        public long ChatId { get; set; }
        public int TotalSubmitCount { get; set; }
    }
}
