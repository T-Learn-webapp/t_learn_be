using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace TLearn.Infrastructure.Services;

public interface IEmailService
{
    Task SendSubjectInvitationEmail(string to, string subjectName, string acceptLink, string permission,
        string? userName = null);

    Task SendSubjectRegistrationInvitationEmail(string to, string subjectName, string registerLink, string permission,
        string inviterName);

    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendVerificationEmail(string to, string verificationLink);
    Task SendTodoAssignmentEmail(
        string to,
        string todoTitle,
        string subjectName,
        string todoLink,
        DateTime? dueDate,
        string assignerName,
        string? description = null);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        var smtpServer = _config["Email:SmtpServer"];
        var smtpPort = int.Parse(_config["Email:SmtpPort"]!);
        var username = _config["Email:Username"];
        var password = _config["Email:Password"];
        var from = _config["Email:From"];

        using var smtpClient = new SmtpClient(smtpServer, smtpPort)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(from!),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };

        message.To.Add(to);

        await smtpClient.SendMailAsync(message);
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
            Body = $@"
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

    public async Task SendSubjectInvitationEmail(string to, string subjectName, string acceptLink, string permission,
        string? userName = null)
    {
        var greeting = string.IsNullOrEmpty(userName) ? "Hello" : $"Hello {userName}";
        var permissionText = GetPermissionText(permission);

        var subject = $"Invitation to join {subjectName} on LearnFlash";
        var body = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
            <h2 style='color: #4F46E5;'>Subject Invitation</h2>
            <p>{greeting},</p>
            <p>You have been invited to join <strong>{subjectName}</strong> with <strong>{permissionText}</strong> permissions.</p>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{acceptLink}' 
                   style='background-color: #4F46E5; color: white; padding: 12px 24px; 
                          text-decoration: none; border-radius: 5px; display: inline-block;'>
                    Accept Invitation
                </a>
            </div>
            <p>This invitation will expire in 7 days.</p>
            <hr style='margin: 20px 0;' />
            <p style='color: #6B7280; font-size: 12px;'>
                If you didn't expect this invitation, you can ignore this email.
            </p>
        </div>
    ";

        await SendEmailAsync(to, subject, body);
    }
    
    public async Task SendTodoAssignmentEmail(
    string to,
    string todoTitle,
    string subjectName,
    string todoLink,
    DateTime? dueDate,
    string assignerName,
    string? description = null)
{
    var dueDateText = dueDate.HasValue
        ? dueDate.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
        : "Không có hạn";

    var subject = $"Bạn được giao nhiệm vụ mới: {todoTitle}";

    var body = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>

            <h2 style='color: #4F46E5; margin-bottom: 20px;'>
                Bạn có nhiệm vụ mới
            </h2>

            <p>
                <strong>{assignerName}</strong> đã giao cho bạn một công việc trong subject 
                <strong>{subjectName}</strong>.
            </p>

            <div style='
                background-color: #F9FAFB;
                border: 1px solid #E5E7EB;
                border-radius: 8px;
                padding: 16px;
                margin: 20px 0;
            '>

                <p style='margin: 0 0 10px 0;'>
                    <strong>Tiêu đề:</strong>
                </p>

                <p style='font-size: 18px; color: #111827; margin: 0 0 16px 0;'>
                    {todoTitle}
                </p>

                <p style='margin: 0 0 10px 0;'>
                    <strong>Hạn hoàn thành:</strong> {dueDateText}
                </p>

                {(string.IsNullOrWhiteSpace(description)
                    ? ""
                    : $@"
                    <div style='margin-top:16px;'>
                        <p>
                            <strong>Mô tả:</strong>
                        </p>

                        <div style='
                            background-color:#FFFFFF;
                            border:1px solid #E5E7EB;
                            border-radius:6px;
                            padding:12px;
                            color:#374151;
                            line-height:1.6;
                        '>
                            {description}
                        </div>
                    </div>
                    ")}
            </div>

            <div style='text-align: center; margin: 30px 0;'>

                <a href='{todoLink}' 
                   style='
                        background-color: #4F46E5;
                        color: white;
                        padding: 12px 24px;
                        text-decoration: none;
                        border-radius: 6px;
                        display: inline-block;
                        font-weight: bold;
                   '>
                    Xem công việc
                </a>

            </div>

            <hr style='margin: 30px 0;' />

            <p style='color: #6B7280; font-size: 12px;'>
                Email này được gửi tự động từ hệ thống TLearn.
            </p>

        </div>
    ";

    await SendEmailAsync(
        to,
        subject,
        body,
        true);
}

    public async Task SendSubjectRegistrationInvitationEmail(string to, string subjectName, string registerLink,
        string permission, string inviterName)
    {
        var permissionText = GetPermissionText(permission);
        var subject = $"{inviterName} invited you to join {subjectName} on LearnFlash";
        var body = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
            <h2 style='color: #4F46E5;'>You're Invited!</h2>
            <p><strong>{inviterName}</strong> has invited you to join <strong>{subjectName}</strong> on LearnFlash.</p>
            <p>You will have <strong>{permissionText}</strong> permissions.</p>
            <p>To accept this invitation, please create a free account first:</p>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{registerLink}' 
                   style='background-color: #4F46E5; color: white; padding: 12px 24px; 
                          text-decoration: none; border-radius: 5px; display: inline-block;'>
                    Create Account & Accept Invitation
                </a>
            </div>
            <p>This invitation will expire in 7 days.</p>
            <hr style='margin: 20px 0;' />
            <p style='color: #6B7280; font-size: 12px;'>
                Already have an account? <a href='{_config["FrontendUrl"]}/login?email={Uri.EscapeDataString(to)}'>Login here</a>
            </p>
        </div>
    ";
        await SendEmailAsync(to, subject, body);
    }

    private string GetPermissionText(string permission)
    {
        return permission switch
        {
            "ViewOnly" => "view only",
            "Comment" => "view and comment",
            "Edit" => "edit content",
            "Manage" => "manage members",
            _ => "view"
        };
    }
}