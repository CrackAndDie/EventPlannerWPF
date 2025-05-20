using System.ComponentModel.DataAnnotations.Schema;

namespace EventPlanner.Entities
{
    [Table("Organization")]
    public class Organization
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
