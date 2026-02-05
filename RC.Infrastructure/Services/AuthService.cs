using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RC.Application.DTOs.Auth;
using RC.Application.Interfaces.Services;
using RC.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RC.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            // Check if user exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email already registered"
                };
            }

            // Create user
            var user = new AppUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = request.DateOfBirth,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            // Assign default role
            await _userManager.AddToRoleAsync(user, "Customer");

            // Generate email confirmation token
            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Send verification email
            await _emailService.SendEmailVerificationAsync(user.Email!, emailToken);

            // Generate JWT token
            var token = await GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful. Please verify your email.",
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email!,
                    PhoneNumber = user.PhoneNumber ?? "",
                    IsVerified = user.IsVerified,
                    Roles = new List<string> { "Customer" }
                }
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }

            if (user.IsDeleted)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Account has been deactivated"
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }

            var token = await GenerateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                TokenExpiration = DateTime.UtcNow.AddDays(7),
                User = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email!,
                    PhoneNumber = user.PhoneNumber ?? "",
                    ProfileImageUrl = user.ProfileImageUrl,
                    IsVerified = user.IsVerified,
                    Roles = roles.ToList()
                }
            };
        }

        public async Task<Application.DTOs.Common.ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal that user doesn't exist
                return Application.DTOs.Common.ApiResponse<string>.SuccessResponse("", "If the email exists, a password reset link has been sent.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            await _emailService.SendPasswordResetEmailAsync(user.Email!, token);

            return Application.DTOs.Common.ApiResponse<string>.SuccessResponse("", "Password reset email sent successfully");
        }

        public async Task<Application.DTOs.Common.ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Application.DTOs.Common.ApiResponse<string>.ErrorResponse("Invalid request");
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (!result.Succeeded)
            {
                return Application.DTOs.Common.ApiResponse<string>.ErrorResponse(
                    "Password reset failed",
                    result.Errors.Select(e => e.Description).ToList()
                );
            }

            return Application.DTOs.Common.ApiResponse<string>.SuccessResponse("", "Password reset successful");
        }

        public async Task<Application.DTOs.Common.ApiResponse<string>> VerifyEmailAsync(VerifyEmailRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Application.DTOs.Common.ApiResponse<string>.ErrorResponse("Invalid request");
            }

            var result = await _userManager.ConfirmEmailAsync(user, request.Token);

            if (!result.Succeeded)
            {
                return Application.DTOs.Common.ApiResponse<string>.ErrorResponse("Email verification failed");
            }

            user.IsVerified = true;
            await _userManager.UpdateAsync(user);

            return Application.DTOs.Common.ApiResponse<string>.SuccessResponse("", "Email verified successfully");
        }

        public async Task<Application.DTOs.Common.ApiResponse<string>> ChangePasswordAsync(string userId, ChangePasswordRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Application.DTOs.Common.ApiResponse<string>.ErrorResponse("User not found");
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                return Application.DTOs.Common.ApiResponse<string>.ErrorResponse(
                    "Password change failed",
                    result.Errors.Select(e => e.Description).ToList()
                );
            }

            return Application.DTOs.Common.ApiResponse<string>.SuccessResponse("", "Password changed successfully");
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            // TODO: Implement refresh token logic
            throw new NotImplementedException();
        }

        private async Task<string> GenerateJwtToken(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? "YourSuperSecretKeyMinimum32CharactersLong!"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
