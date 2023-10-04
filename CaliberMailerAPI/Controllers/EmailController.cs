using Microsoft.AspNetCore.Mvc;

namespace CaliberMailerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly EmailService _emailService;

        public EmailController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmailAsync([FromBody] EmailRequest emailRequest)
        {
            try
            {
                if(!string.IsNullOrEmpty(emailRequest.ClientId))
                {
                    await _emailService.SendOfficeEmailAsync(emailRequest);
                }
                else
                {
                    await _emailService.SendSmtpEmailAsync(emailRequest);
                }
                return Ok("Email sent successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to send email: {ex.Message}");
            }
        }
    }
    public class EmailRequest
    {
        public string? SMTPServer { get; set; }
        public int Port { get; set; }
        public bool SSL { get; set; }
        public string? ClientId { get; set; }
        public string? TenantId { get; set; }
        public string? OfficeEmail { get; set; }
        public string? OfficeEmailPassword { get; set; }
        public List<string> EmailTo { get; set; } = new List<string>();
        public string? Subject { get; set; }
        public string? MailBody { get; set; }
        public List<string> CCMail { get; set; } = new List<string>();
        public List<byte[]> AttachmentFileBytes { get; set; } = new List<byte[]>();
        public List<string> AttachmentFileNames { get; set; } = new List<string>();
    }
}
