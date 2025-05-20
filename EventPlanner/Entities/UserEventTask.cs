using System.ComponentModel.DataAnnotations.Schema;

namespace EventPlanner.Entities
{
    [Table("UserEventTask")]
    public class UserEventTask
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int EventTaskId { get; set; }
    }
}
