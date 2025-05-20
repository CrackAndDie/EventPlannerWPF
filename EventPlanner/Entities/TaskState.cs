namespace EventPlanner.Entities
{
    public class TaskState
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public enum TaskStateEnum
    {
        None =          0,
        NotStarted =    1,
        Started =       2,
        Done =          3,
        Failed =        4,
    }
}
