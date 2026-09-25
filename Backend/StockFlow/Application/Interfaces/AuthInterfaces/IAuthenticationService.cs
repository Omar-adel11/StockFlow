using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.AuthDTOs;
using Application.DTOs.userDtos;

namespace Application.Interfaces.AuthInterfaces
{
    public interface IAuthenticationService
    {
        Task<UserDTO?> LoginAsync(LoginDTO loginDTO);

        //Signup

        Task<UserDTO?> Signup(SignupDTO signupDTO);

        //forget password
        Task<string> ForgotPasswordAsync(string email);

        // Verify OTP
        Task<string> CheckOtpAsync(CheckOtpDTO checkOtpDTO);
        // Reset Password

        Task<string> ResetPasswordAsync(ResetPassDto resetPassDto);

        // Change Password

        Task<string> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO, string email);

        Task<UserDTO?> refresh(RefreshRequestDto refreshRequestDto);
        Task logout(RefreshRequestDto refreshRequestDto);
    }
}
