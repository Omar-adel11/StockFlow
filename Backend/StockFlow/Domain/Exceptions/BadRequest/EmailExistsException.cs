using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions.BadRequest
{
    public class EmailExistsException() : BadRequestException("Email is already exists")
    {
    }
}
