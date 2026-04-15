using System.Linq;
using praktika1.Models;

namespace praktika1.Helpers
{
    public static class EntityFrameworkHelper
    {
        public static User LoginEF(string email, string password)
        {
            string hash = Md5Helper.ComputeMD5(password);
            using (var context = new AppDbContext())
            {
                return context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == hash);
            }
        }

        public static bool RegisterEF(string email, string password)
        {
            using (var context = new AppDbContext())
            {
                var guestRole = context.Roles.FirstOrDefault(r => r.Name == "Гость");
                if (guestRole == null) return false;
                var user = new User
                {
                    Email = email,
                    PasswordHash = Md5Helper.ComputeMD5(password),
                    RoleId = guestRole.Id,
                    RegistrationDate = System.DateTime.Now
                };
                context.Users.Add(user);
                return context.SaveChanges() > 0;
            }
        }
    }
}