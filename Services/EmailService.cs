using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Diagnostics;


namespace lms.android.Services;

public class EmailService
{
    private const string SmtpServer = "smtp.gmail.com";
    private const int SmtpPort = 587;
    private const string SenderEmail = "msulemanriaz33@gmail.com";
    private const string SenderPassword = "zgitjxzgkzjoywvf"; 

    public async Task<bool> SendOtpEmailAsync(string recipientEmail, string otpCode)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("LMS Portal Auth", SenderEmail));
            message.To.Add(new MailboxAddress("", recipientEmail));
            message.Subject = "Reset Your Password - LMS OTP Verification";
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd;'>
                        <h2>LMS Password Recovery</h2>
                        <p>We received a request to reset your account password. Use the verification code below to proceed:</p>
                        <h1 style='color: #FFC107; letter-spacing: 5px; text-align: center;'>{otpCode}</h1>
                        <p>This validation code will expire shortly. If you did not make this request, please disregard this message.</p>
                    </div>"
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(SmtpServer, SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(SenderEmail, SenderPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SMTP Email Error ]: {ex.Message}");
            Debug.WriteLine($"[SMTP StackTrace]: {ex.StackTrace}");

            return false;
        }
    }
}