using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace TLearn.Infrastructure.Services;

public interface IEmailService
{
    Task SendVerificationEmail(string to, string verificationLink);
}
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendVerificationEmail(string to, string verificationLink)
    {
        using var client = new SmtpClient(_config["Email:SmtpServer"], int.Parse(_config["Email:SmtpPort"]));
        client.Credentials = new NetworkCredential(_config["Email:Username"], _config["Email:Password"]);
        client.EnableSsl = true;

        var mail = new MailMessage
        {
            From = new MailAddress(_config["Email:From"], "LearnFlash"),
            Subject = "Xác thực email của bạn",
            Body =  $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <h2 style='color: #4F46E5;'>Chào mừng  đến với TLearn!</h2>
                <p>Cảm ơn bạn đã đăng ký tài khoản. Vui lòng xác thực email của bạn bằng cách nhấp vào link bên dưới:</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{verificationLink}' 
                       style='background-color: #4F46E5; color: white; padding: 12px 24px; 
                              text-decoration: none; border-radius: 5px; display: inline-block;'>
                        Xác thực email
                    </a>
                </div>
                <p>Hoặc copy link sau vào trình duyệt:</p>
                <p style='background-color: #f3f4f6; padding: 10px; word-break: break-all;'>
                    {verificationLink}
                </p>
                <p>Link này sẽ hết hạn sau 24 giờ.</p>
                <hr style='margin: 20px 0;' />
                <p style='color: #6B7280; font-size: 12px;'>
                    Nếu bạn không đăng ký tài khoản TLearn, vui lòng bỏ qua email này.
                </p>
            </div>
        
            ",
            IsBodyHtml = true
        };
        mail.To.Add(to);

        await client.SendMailAsync(mail);
    }
}