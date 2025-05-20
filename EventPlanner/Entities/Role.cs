namespace EventPlanner.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public enum RoleEnum
    {
        None =          0,
        Director =      1,
        StaffManager =  2,
        EventManager =  4,
        TaskManager =   5,
        Staff =         6,
    }
}
