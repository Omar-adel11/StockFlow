using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Interfaces.AuthInterfaces;
using Application.Services.Auth;
using Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class ServiceManager : IServiceManager
    {
        private readonly Lazy<IAuthenticationService> _authService;

        public ServiceManager(UserManager<User> _userManager,
        ITokenService _tokenService,
        IRefreshTokenService _refreshTokenService,

        IOTPService _oTPService,
        IEmailService _emailService,
        IHostingEnvironment _env)
        {
            _authService = new Lazy<IAuthenticationService>(() => new AuthenticationService(_userManager, _tokenService, _refreshTokenService, _oTPService, _emailService, _env));
        }
        public IAuthenticationService AuthService => _authService.Value;
    }
}
