using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.AuthInterfaces
{
    public interface IServiceManager
    {
        IAuthenticationService AuthService { get; }
        
    }
}
