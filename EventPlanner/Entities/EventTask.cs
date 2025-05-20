using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventPlanner.Entities
{
    [Table("EventTask")]
    public class EventTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime PlanEndDate { get; set; }
        public DateTime ActualEndDate { get; set; }
        public int EventId { get; set; }
        public int StateId { get; set; }
    }
}
