using System;

namespace CCCP.DAL.Entities
{
    public class Registration
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public int SubmitCount { get; set; }
    }
}