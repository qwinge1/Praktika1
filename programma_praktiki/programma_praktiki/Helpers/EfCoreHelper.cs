using System.Linq;
using Microsoft.EntityFrameworkCore;
using programma_praktiki.Models;

namespace programma_praktiki.Helpers
{
    public class EfCoreHelper
    {
        private readonly AppDbContext _context;

        public EfCoreHelper(AppDbContext context)
        {
            _context = context;
        }

        public User? LoginEF(string email, string password)
        {
            string hash = Md5Helper.ComputeMD5(password);
            return _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Email == email && u.PasswordHash == hash);
        }

        public bool RegisterEF(string email, string password)
        {
            var guestRole = _context.Roles.FirstOrDefault(r => r.Name == "Гость");
            if (guestRole == null) return false;

            var user = new User
            {
                Email = email,
                PasswordHash = Md5Helper.ComputeMD5(password),
                RoleId = guestRole.Id
                // RegistrationDate не задаём — БД поставит CURRENT_TIMESTAMP
            };
            _context.Users.Add(user);
            return _context.SaveChanges() > 0;
        }
    }
}