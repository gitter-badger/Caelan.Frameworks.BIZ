using System.Collections.Generic;

namespace Caelan.Frameworks.BIZ.NUnit.Data.Models
{
    public partial class User
    {
        public User()
        {
            UserRoles = new List<UserRole>();
        }

        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
