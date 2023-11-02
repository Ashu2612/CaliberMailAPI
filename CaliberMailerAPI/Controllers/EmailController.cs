﻿using CaliberMailerAPI.Data;
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
                var mailLog = new MailLogModel
                {
                    ProfileId = emailRequest.ProfileId,
                    CCMail = emailRequest.CCMail,
                    MailBody = emailRequest.MailBody,
                    Subject = emailRequest.Subject,
                    EmailTo = emailRequest.EmailTo,
                    AttachmentFileBytes = emailRequest.AttachmentFileBytes,
                    AttachmentFileNames = emailRequest.AttachmentFileNames,
                    Status = "Success",
                    DateTime = DateTime.Now
                };

                AddMailLog(mailLog);

                var profile = await _context.AD_MAIL_PROFILE.FirstOrDefaultAsync(p => p.ProfileId == emailRequest.ProfileId);


                if (profile == null)
                {
                    return BadRequest("Profile not found.");
                }

                if (!string.IsNullOrEmpty(profile.ClientId))
                {
                    await _emailService.SendOfficeEmailAsync(emailRequest, profile);
                }
                else
                {
                    await _emailService.SendSmtpEmailAsync(emailRequest, profile);
                }

                return Ok("Email sent successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to send email: {ex.Message}");
            }
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
                return CreatedAtAction("GetProfile", new { id = profileData.ProfileId }, profileData);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to create profile: {ex.Message}");
            }
        }



        void AddMailLog(MailLogModel mailLogModel)
        {
            using (var db = new DataContext())
            {
                db.AD_MAIL_LOG.Add(mailLogModel);
                db.SaveChanges();
            }
        }
    }

}
