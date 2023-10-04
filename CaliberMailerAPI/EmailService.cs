using CaliberMailerAPI.Controllers;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Security;
using Microsoft.IdentityModel.Tokens;

namespace CaliberMailerAPI
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="to">Recipient's email address.</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="body">Email body content (HTML or plain text).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendOfficeEmailAsync(EmailRequest emailRequest)
        {
            try
            {
                var pcaOptions = new PublicClientApplicationOptions
                {
                    ClientId = emailRequest.ClientId,
                    TenantId = emailRequest.TenantId,

                    RedirectUri = "https://login.microsoftonline.com/common/oauth2/nativeclient"
                };
                var pca = PublicClientApplicationBuilder
                    .CreateWithApplicationOptions(pcaOptions)
                    .WithAuthority(AzureCloudInstance.AzurePublic, emailRequest.TenantId).Build();

                var ewsScope = new string[] { "https://graph.microsoft.com/.default" };
                var securePasssword = new SecureString();
                foreach (char c in emailRequest.OfficeEmailPassword)
                    securePasssword.AppendChar(c);
                var authResult = await pca.AcquireTokenByUsernamePassword(ewsScope, emailRequest.OfficeEmail, securePasssword).ExecuteAsync();

                GraphServiceClient graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
                {
                    requestMessage.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                }));
                List<Recipient> toMailList = new List<Recipient>();
                char[] separator = { ';' };
                foreach (string ToUser in emailRequest.EmailTo)
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
                if (emailRequest.CCMail.Count == 0)
                {
                    foreach (string CCUser in emailRequest.CCMail)
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
                var message = new Microsoft.Graph.Message
                {
                    Subject = emailRequest.Subject,
                    Body = new ItemBody
                    {
                        ContentType = BodyType.Html,
                        Content = emailRequest.MailBody
                    },

                    ToRecipients = toMailList,
                    CcRecipients = CCMailList,
                    Attachments = new MessageAttachmentsCollectionPage(),
                };
                try
                {
                    if (emailRequest.AttachmentFileBytes.Count != 0)
                    {
                        for (int i = 0; i < emailRequest.AttachmentFileBytes.Count; i++)
                        {
                            byte[] fileBytes = emailRequest.AttachmentFileBytes[i];
                            string fileName = emailRequest.AttachmentFileNames[i];

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
                catch { }
                try
                {
                    await graphServiceClient.Me.SendMail(message, true).Request().PostAsync();
                }
                catch { }
                graphServiceClient = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw;
            }
        }


        public async Task SendSmtpEmailAsync(EmailRequest emailRequest)
        {
            try
            {
                using (var client = new SmtpClient(emailRequest.SMTPServer))
                {
                    client.Port = emailRequest.Port;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(emailRequest.OfficeEmail, emailRequest.OfficeEmailPassword);
                    client.EnableSsl = emailRequest.SSL;

                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(emailRequest.OfficeEmail);

                        foreach (string toEmail in emailRequest.EmailTo)
                        {
                            message.To.Add(toEmail);
                        }

                        foreach (string ccEmail in emailRequest.CCMail)
                        {
                            if (!string.IsNullOrEmpty(ccEmail))
                            {
                                message.CC.Add(ccEmail);
                            }
                        }


                        message.Subject = emailRequest.Subject;
                        message.Body = emailRequest.MailBody;
                        message.IsBodyHtml = true;

                        foreach (var attachmentBytes in emailRequest.AttachmentFileBytes)
                        {
                            string fileName = emailRequest.AttachmentFileNames[emailRequest.AttachmentFileBytes.IndexOf(attachmentBytes)];
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
