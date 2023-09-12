using ISZR.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Text;

namespace ISZR.Web.Services
{
    public class EmailService
    {
        private readonly DataContext _context;
        private readonly string? smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER");
        private readonly int smtpPort = 25;
        private readonly string? smtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME");
        private readonly string? smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

        public EmailService(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Igénylés státusz módosítás esetén értesítés
        /// </summary>
        /// <param name="requestData">Igénylés adatai</param>
        /// <param name="status">Új státusz</param>
        public async Task SendStatusChange(Request requestData, string status)
        {
            if (smtpServer == null || smtpUsername == null || smtpPassword == null) { return; }

            // SMTP beállítása
            var smtpClient = new SmtpClient(smtpServer)
            {
                Port = smtpPort,
                EnableSsl = false,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            User? creator = await GetUserById(requestData.CreatedByUserId);
            User? createdFor = await GetUserById(requestData.CreatedForUserId);

            // Igénylést készítő értesítése
            if (!string.IsNullOrEmpty(creator.Email) && creator.UserId != createdFor.UserId)
            {
                try
                {
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("skfb.informatika@bv.gov.hu"),
                        Subject = $"ISZR #{requestData.RequestId} igénylés állapota megváltozott",
                        IsBodyHtml = true,
                        BodyEncoding = Encoding.UTF8,
                        Body = $@"<!DOCTYPE html>
<html lang=""hu"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>ISZR</title>
</head>
<body style=""background-color: #ffffff; margin: 0; padding: 0;"">
    <table cellpadding=""0"" cellspacing=""0"" width=""100%"">
        <tr>
            <td align=""center"" valign=""top"" style=""padding: 20px;"">
                <table cellpadding=""0"" cellspacing=""0"" width=""600"" style=""background-color: #ffffff; border: 1px solid #e0e0e0; border-radius: 5px; box-shadow: 0 0 10px rgba(0,0,0,0.1);"">
                    <tr>
                        <td align=""center"" style=""padding: 40px;"">
                            <h1 style=""color: #333;"">Tisztelt  {creator.DisplayName}!</h1>
                            <p style=""font-size: 16px; color: #555;"">Az általad létrehozott #{requestData.RequestId} azonosítójú igénylés státusza megváltozott.</p>
                            <p style=""font-size: 14px; color: #555; margin-top: 30px;""><strong>Felhasználó:</strong> {createdFor.DisplayName} bv.{createdFor.Rank.ToLower()}</p>
                            <p style=""font-size: 14px; color: #555;""><strong>Típus:</strong> {requestData.Type}</p>
                            <p style=""font-size: 14px; color: #555;""><strong>Állapot:</strong> {status}</p>
                            <a href=""http://skfb-iszr/Requests/Details/{requestData.RequestId}"" style=""display: inline-block; background-color: #17a2b8; color: #fff; text-decoration: none; padding: 10px 20px; font-size: 18px; border-radius: 5px; margin-top: 40px;"">Igénylés megtekintése</a>
                            <p style=""font-size: 12px; color: #777; margin-top: 30px;"">Ez egy automatikus értesítő. Kérjük, ne válaszoljon erre az e-mailre.</p>
                        </td>
                    </tr>
                    <tr>
                        <td align=""center"" style=""padding: 10px; background-color: #17a2b8;"">
                            <p style=""font-size: 14px; color: #fff;"">&copy; 2021-{DateTime.Now.Year} ISZR</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>
"
                    };



                    mailMessage.To.Add(creator.Email);

                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            // Felhasználó értesítése
            if (!string.IsNullOrEmpty(createdFor.Email))
            {
                try
                {
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("skfb.informatika@bv.gov.hu"),
                        Subject = $"ISZR #{requestData.RequestId} igénylés állapota megváltozott",
                        IsBodyHtml = true,
                        BodyEncoding = Encoding.UTF8,
                        Body = $@"<!DOCTYPE html>
<html lang=""hu"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>ISZR</title>
</head>
<body style=""background-color: #ffffff; margin: 0; padding: 0;"">
    <table cellpadding=""0"" cellspacing=""0"" width=""100%"">
        <tr>
            <td align=""center"" valign=""top"" style=""padding: 20px;"">
                <table cellpadding=""0"" cellspacing=""0"" width=""600"" style=""background-color: #ffffff; border: 1px solid #e0e0e0; border-radius: 5px; box-shadow: 0 0 10px rgba(0,0,0,0.1);"">
                    <tr>
                        <td align=""center"" style=""padding: 40px;"">
                            <h1 style=""color: #333;"">Tisztelt  {createdFor.DisplayName}!</h1>
                            <p style=""font-size: 16px; color: #555;"">Az számodra létrehozott #{requestData.RequestId} azonosítójú igénylés státusza megváltozott.</p>
                            <p style=""font-size: 14px; color: #555; margin-top: 30px;""><strong>Típus:</strong> {requestData.Type}</p>
                            <p style=""font-size: 14px; color: #555;""><strong>Állapot:</strong> {status}</p>
                            <a href=""http://skfb-iszr/Requests/Details/{requestData.RequestId}"" style=""display: inline-block; background-color: #17a2b8; color: #fff; text-decoration: none; padding: 10px 20px; font-size: 18px; border-radius: 5px; margin-top: 40px;"">Igénylés megtekintése</a>
                            <p style=""font-size: 12px; color: #777; margin-top: 30px;"">Ez egy automatikus értesítő. Kérjük, ne válaszoljon erre az e-mailre.</p>
                        </td>
                    </tr>
                    <tr>
                        <td align=""center"" style=""padding: 10px; background-color: #17a2b8;"">
                            <p style=""font-size: 14px; color: #fff;"">&copy; 2021-{DateTime.Now.Year} ISZR</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>
"
                    };



                    mailMessage.To.Add(createdFor.Email);

                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Igénylés státusz módosítás esetén értesítés
        /// </summary>
        /// <param name="requestData">Igénylés adatai</param>
        /// <param name="status">Új státusz</param>
        public async Task SendNewRequestNotification(Request requestData)
        {
            if (smtpServer == null || smtpUsername == null || smtpPassword == null) { return; }

            // SMTP beállítása
            var smtpClient = new SmtpClient(smtpServer)
            {
                Port = smtpPort,
                EnableSsl = false,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            User? creator = await GetUserById(requestData.CreatedByUserId);
            User? createdFor = await GetUserById(requestData.CreatedForUserId);

            // Felhasználó értesítése
            if (!string.IsNullOrEmpty(createdFor.Email) && creator.UserId != createdFor.UserId)
            {
                try
                {
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("skfb.informatika@bv.gov.hu"),
                        Subject = $"ISZR #{requestData.RequestId} új igénylés érkezett",
                        IsBodyHtml = true,
                        BodyEncoding = Encoding.UTF8,
                        Body = $@"<!DOCTYPE html>
<html lang=""hu"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>ISZR</title>
</head>
<body style=""background-color: #ffffff; margin: 0; padding: 0;"">
    <table cellpadding=""0"" cellspacing=""0"" width=""100%"">
        <tr>
            <td align=""center"" valign=""top"" style=""padding: 20px;"">
                <table cellpadding=""0"" cellspacing=""0"" width=""600"" style=""background-color: #ffffff; border: 1px solid #e0e0e0; border-radius: 5px; box-shadow: 0 0 10px rgba(0,0,0,0.1);"">
                    <tr>
                        <td align=""center"" style=""padding: 40px;"">
                            <h1 style=""color: #333;"">Tisztelt  {createdFor.DisplayName}!</h1>
                            <p style=""font-size: 16px; color: #555;"">Új igénylés lett számodra kérve amely #{requestData.RequestId} azonosítót kapta.</p>
                            <p style=""font-size: 14px; color: #555; margin-top: 30px;""><strong>Ügyintéző:</strong> {creator.DisplayName} bv.{creator.Rank.ToLower()}</p>
                            <p style=""font-size: 14px; color: #555;""><strong>Típus:</strong> {requestData.Type}</p>
                            <a href=""http://skfb-iszr/Requests/Details/{requestData.RequestId}"" style=""display: inline-block; background-color: #17a2b8; color: #fff; text-decoration: none; padding: 10px 20px; font-size: 18px; border-radius: 5px; margin-top: 40px;"">Igénylés megtekintése</a>
                            <p style=""font-size: 12px; color: #777; margin-top: 30px;"">Ez egy automatikus értesítő. Kérjük, ne válaszoljon erre az e-mailre.</p>
                        </td>
                    </tr>
                    <tr>
                        <td align=""center"" style=""padding: 10px; background-color: #17a2b8;"">
                            <p style=""font-size: 14px; color: #fff;"">&copy; 2021-{DateTime.Now.Year} ISZR</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>
"
                    };



                    mailMessage.To.Add(createdFor.Email);

                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Felhasználó megkeresése a rendszerben id által
        /// </summary>
        /// <param name="id">Felhasználó azonosítója</param>
        /// <returns>Felhasználó</returns>
        private async Task<User?> GetUserById(int? id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        }
    }
}
