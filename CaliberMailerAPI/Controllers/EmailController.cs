using CaliberMailerAPI.Data;
using CaliberMailerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CaliberMailerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly EmailService _emailService;

        public EmailController(DataContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }


        [HttpPost("send")]
        public async Task<IActionResult> SendEmailAsync([FromBody] MailsModel emailRequest)
        {
            try
            {
                // Fetch the profile based on the ProfileId
                var profile = await _context.AD_MAIL_PROFILE.FirstOrDefaultAsync(p => p.ProfileId == emailRequest.ProfileId);

                // Check if the profile exists
                if (profile == null)
                {
                    return BadRequest("Profile not found.");
                }

                try
                {
                    // Send email based on the presence of ClientId in the profile
                    if (!string.IsNullOrEmpty(profile.ClientId))
                    {
                        await _emailService.SendOfficeEmailAsync(emailRequest, profile);
                    }
                    else
                    {
                        await _emailService.SendSmtpEmailAsync(emailRequest, profile);
                    }
                }
                catch (Exception ex)
                {
                    await LogMailAsync(emailRequest, "Failed", ex.Message);
                    return BadRequest($"Failed to send email: {ex.Message}");
                }

                await LogMailAsync(emailRequest, "Successful", "No errors");
                return Ok("Email sent successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to send email: {ex.Message}");
            }
        }

        private async Task LogMailAsync(MailsModel emailRequest, string status, string error)
        {
            // Convert lists to strings
            string ccMail = string.Join(",", emailRequest.CCMail);
            string toMail = string.Join(",", emailRequest.EmailTo);
            string attachmentFileNames = string.Join(",", emailRequest.AttachmentFileNames);
            List<string> attachmentFileBytesBase64 = emailRequest.AttachmentFileBytes.Select(b => Convert.ToBase64String(b)).ToList();

            // Create a new MailLogModel object
            var mailLog = new MailLogModel
            {
                ProfileId = emailRequest.ProfileId,
                EmailTo = toMail,
                Subject = emailRequest.Subject,
                MailBody = emailRequest.MailBody,
                CCMail = ccMail,
                AttachmentFileBytes = string.Join(",", attachmentFileBytesBase64),
                AttachmentFileNames = attachmentFileNames,
                Status = status,
                Error = error,
                DateTime = DateTime.Now
            };

            // Add the new MailLogModel to the context and save changes
            _context.AD_MAIL_LOG.Add(mailLog);
            await _context.SaveChangesAsync();
        }



        [HttpPost("create-profile")]
        public async Task<ActionResult<ProfilesModel>> CreateProfile([FromBody] ProfilesModel profile)
        {
            try
            {
                var profileData = new ProfilesModel
                {
                    ProfileId = profile.ProfileId,
                    SMTPServer = profile.SMTPServer ?? string.Empty,
                    Port = profile.Port,
                    SSL = profile.SSL,
                    ClientId = profile.ClientId ?? string.Empty,
                    ClientSecret = profile.ClientSecret ?? string.Empty,
                    TenantId = profile.TenantId ?? string.Empty,
                    OfficeEmail = profile.OfficeEmail ?? string.Empty,
                    OfficeEmailPassword = profile.OfficeEmailPassword ?? string.Empty
                };

                _context.AD_MAIL_PROFILE.Add(profileData);
                await _context.SaveChangesAsync();
                var CreatedProfileData = await _context.AD_MAIL_PROFILE.FirstOrDefaultAsync(p => p.ProfileId == profile.ProfileId);
                return CreatedProfileData;
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to create profile: {ex.Message}");
            }
        }
    }

}
