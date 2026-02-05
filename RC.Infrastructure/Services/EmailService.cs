using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using RC.Application.DTOs.Booking;
using RC.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RC.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(
                    _configuration["Email:SenderName"] ?? "CityCars Azerbaijan",
                    _configuration["Email:SenderEmail"] ?? "noreply@citycars.az"
                ));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = body };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(
                    _configuration["Email:SmtpHost"] ?? "smtp.gmail.com",
                    int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
                    SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]
                );

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Log error (in production use proper logging)
                Console.WriteLine($"Email send failed: {ex.Message}");
            }
        }

        public async Task SendWelcomeEmailAsync(string to, string userName)
        {
            var subject = "Welcome to CityCars Azerbaijan!";
            var body = $@"
            <html>
            <body>
                <h2>Welcome to CityCars Azerbaijan, {userName}!</h2>
                <p>Thank you for registering with us.</p>
                <p>We're excited to have you on board.</p>
                <p>Start exploring our wide range of premium vehicles today!</p>
                <br/>
                <p>Best regards,<br/>CityCars Azerbaijan Team</p>
            </body>
            </html>
        ";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendBookingConfirmationEmailAsync(string to, BookingDetailsDto booking)
        {
            var subject = $"Booking Confirmation - #{booking.BookingNumber}";
            var body = $@"
            <html>
            <body>
                <h2>Booking Confirmation</h2>
                <p>Dear Customer,</p>
                <p>Your booking has been confirmed!</p>
                
                <h3>Booking Details:</h3>
                <ul>
                    <li><strong>Booking Number:</strong> {booking.BookingNumber}</li>
                    <li><strong>Car:</strong> {booking.CarBrand} {booking.CarModel}</li>
                    <li><strong>Pickup Date:</strong> {booking.StartDate:dd MMM yyyy}</li>
                    <li><strong>Return Date:</strong> {booking.EndDate:dd MMM yyyy}</li>
                    <li><strong>Pickup Location:</strong> {booking.PickupLocationName}</li>
                    <li><strong>Return Location:</strong> {booking.ReturnLocationName}</li>
                    <li><strong>Total Amount:</strong> ${booking.TotalAmount:F2}</li>
                </ul>
                
                <p>Please arrive 15 minutes before your pickup time.</p>
                <p>Don't forget to bring:</p>
                <ul>
                    <li>Valid driver's license</li>
                    <li>ID/Passport</li>
                    <li>Credit card for deposit</li>
                </ul>
                
                <p>For any questions, please contact us at support@citycars.az</p>
                <br/>
                <p>Best regards,<br/>CityCars Azerbaijan Team</p>
            </body>
            </html>
        ";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string email, string token)
        {
            var backendUrl = _configuration["App:BackendUrl"]; // https://localhost:7203
            var encodedToken = HttpUtility.UrlEncode(token);
            var encodedEmail = HttpUtility.UrlEncode(email);

            // wwwroot/reset-password.html sayfasına yönlendir
            var resetLink = $"{backendUrl}/reset-password.html?email={encodedEmail}&token={encodedToken}";

            var subject = "Reset Your Password - CityCars Azerbaijan";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                   color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; padding: 15px 30px; background: #dc3545; 
                  color: white; text-decoration: none; border-radius: 5px; 
                  font-weight: bold; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Password Reset Request</h1>
        </div>
        <div class='content'>
            <h2>Hello!</h2>
            <p>We received a request to reset your password for your CityCars Azerbaijan account.</p>
            <p>Click the button below to reset your password:</p>
            
            <div style='text-align: center;'>
                <a href='{resetLink}' class='button'>Reset Password</a>
            </div>
            
            <p style='margin-top: 20px; color: #666; font-size: 14px;'>
                If the button doesn't work, copy and paste this link into your browser:<br>
                <a href='{resetLink}' style='color: #dc3545; word-break: break-all;'>{resetLink}</a>
            </p>
            
            <p style='margin-top: 20px; color: #999; font-size: 12px;'>
                This link will expire in 24 hours.<br>
                If you didn't request a password reset, please ignore this email.
            </p>
        </div>
        <div class='footer'>
            <p>© 2026 CityCars Azerbaijan. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendEmailVerificationAsync(string to, string verificationToken)
        {
            var verificationUrl = $"{_configuration["App:FrontendUrl"]}/verify-email?token={verificationToken}&email={to}";

            var subject = "Verify Your Email";
            var body = $@"
            <html>
            <body>
                <h2>Email Verification</h2>
                <p>Thank you for registering with CityCars Azerbaijan!</p>
                <p>Please verify your email address by clicking the link below:</p>
                <p><a href='{verificationUrl}'>Verify Email</a></p>
                <p>This link will expire in 24 hours.</p>
                <br/>
                <p>Best regards,<br/>CityCars Azerbaijan Team</p>
            </body>
            </html>
        ";

            await SendEmailAsync(to, subject, body);
        }
    }
}
