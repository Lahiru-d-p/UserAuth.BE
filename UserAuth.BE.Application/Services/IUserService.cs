using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserAuth.BE.Application.Services
{
    public interface IUserService
    {
        Task<string> RegisterAsync(string username, string password, string role);
        Task<string> LoginAsync(string username, string password);
    }

}
