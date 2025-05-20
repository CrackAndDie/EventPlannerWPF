using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using EventPlanner.Managers;

namespace EventPlanner.Entities
{
    [Table("User")]
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public int OrganizationId { get; set; }
        public int RoleId { get; set; }

        public Role GetRole(DbContextManager manager)
        {
            return manager.Roles.FirstOrDefault(x => x.Id == RoleId);
        }

        public Organization GetOrg(DbContextManager manager)
        {
            return manager.Organizations.FirstOrDefault(x => x.Id == OrganizationId);
        }
    }
}
