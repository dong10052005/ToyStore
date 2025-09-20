using Microsoft.EntityFrameworkCore;
using ToyStore.Models;
using System.Security.Cryptography;
using System.Text;

namespace ToyStore.Services
{
    public interface IAuthService
    {
        Task<UserSession?> LoginAsync(string emailOrUsername, string password, string userType);
        Task<bool> RegisterAsync(RegisterViewModel model);
        Task<bool> CreateStaffAsync(CreateStaffViewModel model);
        Task<bool> IsEmailExistsAsync(string email);
        Task<bool> IsUsernameExistsAsync(string username);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }

    public class AuthService : IAuthService
    {
        private readonly ToyStoreContext _context;

        public AuthService(ToyStoreContext context)
        {
            _context = context;
        }

        public async Task<UserSession?> LoginAsync(string emailOrUsername, string password, string userType)
        {
            switch (userType.ToLower())
            {
                case "customer":
                    return await LoginCustomerAsync(emailOrUsername, password);
                case "admin":
                case "staff":
                    return await LoginAdminAsync(emailOrUsername, password);
                default:
                    return null;
            }
        }

        private async Task<UserSession?> LoginCustomerAsync(string emailOrUsername, string password)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == emailOrUsername);

            if (customer == null || !VerifyPassword(password, customer.PasswordHash))
                return null;

            return new UserSession
            {
                UserId = customer.CustomerId,
                Username = customer.Email,
                Email = customer.Email,
                FullName = customer.FullName,
                UserType = "Customer",
                Role = "Customer",
                IsAuthenticated = true
            };
        }

        private async Task<UserSession?> LoginAdminAsync(string emailOrUsername, string password)
        {
            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Username == emailOrUsername);

            if (admin == null || !VerifyPassword(password, admin.PasswordHash))
                return null;

            return new UserSession
            {
                UserId = admin.AdminId,
                Username = admin.Username,
                Email = admin.Username, // Admin sử dụng username thay vì email
                FullName = admin.FullName ?? admin.Username,
                UserType = admin.Role ?? "Staff",
                Role = admin.Role ?? "Staff",
                IsAuthenticated = true
            };
        }

        public async Task<bool> RegisterAsync(RegisterViewModel model)
        {
            // Chỉ cho phép đăng ký Customer
            if (model.UserType.ToLower() == "customer")
            {
                return await RegisterCustomerAsync(model);
            }
            return false;
        }

        public async Task<bool> CreateStaffAsync(CreateStaffViewModel model)
        {
            if (await IsUsernameExistsAsync(model.Username))
                return false;

            var admin = new Admin
            {
                Username = model.Username,
                PasswordHash = HashPassword(model.Password),
                FullName = model.FullName,
                Role = model.Role
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> RegisterCustomerAsync(RegisterViewModel model)
        {
            if (await IsEmailExistsAsync(model.Email))
                return false;

            var customer = new Customer
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                Phone = model.Phone,
                Address = model.Address,
                CreatedAt = DateTime.Now
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> RegisterStaffAsync(RegisterViewModel model)
        {
            if (await IsUsernameExistsAsync(model.Email)) // Staff sử dụng email làm username
                return false;

            var admin = new Admin
            {
                Username = model.Email,
                PasswordHash = HashPassword(model.Password),
                FullName = model.FullName,
                Role = "Staff"
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Customers.AnyAsync(c => c.Email == email);
        }

        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            return await _context.Admins.AnyAsync(a => a.Username == username);
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }
    }
}
