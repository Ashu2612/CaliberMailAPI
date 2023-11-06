using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Net.Mail;
using System.Net;
using CaliberMailerAPI.Models;
using CaliberMailerAPI.Data;

namespace CaliberMailerAPI
{
    public class EmailService
    {
        private readonly DataContext _DCcontext;

        private readonly IConfiguration _config;

        public EmailService(IConfiguration config, DataContext dCcontext)
        {
            _config = config;
            _DCcontext = dCcontext;
        }

        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="to">Recipient's email address.</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="body">Email body content (HTML or plain text).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendOfficeEmailAsync(MailsModel mailsModel, ProfilesModel profilesModel)
        {
            var clientId = profilesModel.ClientId;
            var clientSecret = profilesModel.ClientSecret;
            var tenantId = profilesModel.TenantId;

            var clientApplication = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
                .Build();

            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var authenticationResult = await clientApplication.AcquireTokenForClient(scopes).ExecuteAsync();

            var graphServiceClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(requestMessage =>
                {
                    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
                    return Task.FromResult(0);
                }));
            List<Recipient> toMailList = new List<Recipient>();
            char[] separator = { ';' };
            foreach (string ToUser in mailsModel.EmailTo)
            {
                string[] strlist = ToUser.Split(separator);
                for (int inti = 0; inti < strlist.Count(); inti++)
                {
                    Recipient toMailadd = new Recipient();
                    EmailAddress toMailaddress = new EmailAddress();

                    toMailaddress.Address = strlist[inti];
                    toMailadd.EmailAddress = toMailaddress;
                    toMailList.Add(toMailadd);
                }
            }

            List<Recipient> CCMailList = new List<Recipient>();
            if (mailsModel.CCMail.Count != 0)
            {
                foreach (string CCUser in mailsModel.CCMail)
                {
                    if (!string.IsNullOrEmpty(CCUser))
                    {
                        string[] CClist = CCUser.Split(separator);
                        for (int inti = 0; inti < CClist.Count(); inti++)
                        {
                            Recipient CCMailadd = new Recipient();
                            EmailAddress CCMailaddress = new EmailAddress();

                            CCMailaddress.Address = CClist[inti];
                            CCMailadd.EmailAddress = CCMailaddress;
                            CCMailList.Add(CCMailadd);
                        }
                    }
                }
            }
            var message = new Microsoft.Graph.Message
            {
                Subject = mailsModel.Subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = mailsModel.MailBody
                },

                ToRecipients = toMailList,
                CcRecipients = CCMailList,
                Attachments = new MessageAttachmentsCollectionPage(),
            };
            try
            {
                if (mailsModel.AttachmentFileBytes.Count != 0)
                {
                    for (int i = 0; i < mailsModel.AttachmentFileBytes.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(mailsModel.AttachmentFileNames[i]))
                        {
                            byte[] fileBytes = mailsModel.AttachmentFileBytes[i];
                            string fileName = mailsModel.AttachmentFileNames[i];

                            string contentType = GetContentType(fileName);

                            var attachment = new FileAttachment
                            {
                                Name = fileName,
                                ContentBytes = fileBytes,
                                ContentType = contentType,
                            };

                            message.Attachments.Add(attachment);
                        }
                    }
                }

            }
            catch { }

            await graphServiceClient.Users[profilesModel.OfficeEmail].SendMail(message, true).Request().PostAsync();

        }
        public async Task SendSmtpEmailAsync(MailsModel mailsModel, ProfilesModel profilesModel)
        {
            try
            {
                using (var client = new SmtpClient(profilesModel.SMTPServer))
                {
                    client.Port = profilesModel.Port;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(profilesModel.OfficeEmail, profilesModel.OfficeEmailPassword);
                    client.EnableSsl = profilesModel.SSL;

                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(profilesModel.OfficeEmail);

                        foreach (string toEmail in mailsModel.EmailTo)
                        {
                            message.To.Add(toEmail);
                        }

                        foreach (string ccEmail in mailsModel.CCMail)
                        {
                            if (!string.IsNullOrEmpty(ccEmail))
                            {
                                message.CC.Add(ccEmail);
                            }
                        }


                        message.Subject = mailsModel.Subject;
                        message.Body = mailsModel.MailBody;
                        message.IsBodyHtml = true;

                        foreach (var attachmentBytes in mailsModel.AttachmentFileBytes)
                        {
                            string fileName = mailsModel.AttachmentFileNames[mailsModel.AttachmentFileBytes.IndexOf(attachmentBytes)];
                            string contentType = GetContentType(fileName);

                            var attachment = new System.Net.Mail.Attachment(new MemoryStream(attachmentBytes), fileName, contentType);
                            message.Attachments.Add(attachment);
                        }

                        await client.SendMailAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending SMTP email: {ex.Message}");
                throw;
            }
        }


        private string GetContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName);

            switch (extension.ToLower())
            {
                case ".pdf":
                    return "application/pdf";
                case ".html":
                case ".htm":
                    return "text/html";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                default:
                    return "application/octet-stream";
            }
        }
    }
}
