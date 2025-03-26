using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UserAuth.BE.Domain.Dto;
using UserAuth.BE.Domain.Entities;
using UserAuth.BE.Infrastructure.Repositories;

namespace UserAuth.BE.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<string> RegisterAsync(string username, string password, string role)
        {
            var existingUser = await _userRepository.GetUserByUsernameAsync(username);
            if (existingUser != null) return "User already exists";
            var user = new User();
            var hashedPassword = new PasswordHasher<User>()
                .HashPassword(user,password);
            
            user.Username = username;
            user.PasswordHash = hashedPassword;
            user.Role = role;
            
            await _userRepository.AddUserAsync(user);
            return "User registered successfully";
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user == null || (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash,password)
                == PasswordVerificationResult.Failed))
                return "Invalid credentials";
            
            return CreateToken(user);
        }
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
