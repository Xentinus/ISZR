using ISZR.Web.Models;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace ISZR.Web.Services
{
    public class EmailService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly bool smtpEnable;
        private readonly string? smtpServer;
        private readonly int smtpPort = 25;
        private readonly string? smtpMail;
        private readonly string? smtpUsername;
        private readonly string? smtpPassword;

        public EmailService(DataContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;

            smtpEnable = configuration.GetValue<bool>("ApplicationSettings:SNMP_ENABLE");
            smtpServer = configuration.GetValue<string>("ApplicationSettings:SNMP_SERVER");
            smtpMail = configuration.GetValue<string>("ApplicationSettings:SNMP_MAIL");
            smtpUsername = configuration.GetValue<string>("ApplicationSettings:SNMP_USERNAME");
            smtpPassword = configuration.GetValue<string>("ApplicationSettings:SNMP_PASSWORD");
        }

        /// <summary>
        /// Igénylés státusz módosítás esetén értesítés
        /// </summary>
        /// <param name="requestData">Igénylés adatai</param>
        /// <param name="status">Új státusz</param>
        public async Task SendStatusChange(Request requestData, string status)
        {
            // E-mail szolgáltatás bekapcsolásának ellenőrzése
            if (!smtpEnable) return;

            // SMTP beállítások ellenőrzése
            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(smtpMail)) { return; }

            // SMTP szerver beállítása
            var smtpClient = new SmtpClient(smtpServer)
            {
                Port = smtpPort,
                EnableSsl = false,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            // Igényléssel kapcsolatos felhasználók adatainak lekérdezése
            User? creator = await GetUserById(requestData.CreatedByUserId);
            User? createdFor = await GetUserById(requestData.CreatedForUserId);

            // Dinamikus elérési útvonal legenerálása
            var httpRequest = _httpContextAccessor.HttpContext.Request;
            var requestUrl = $"{httpRequest.Scheme}://{httpRequest.Host}/Requests/Details/{requestData.RequestId}";

            // Igénylést készítő értesítése
            if (!string.IsNullOrEmpty(creator.Email) && creator.UserId != createdFor.UserId)
            {
                try
                {
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(smtpMail),
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
                            <a href=""{requestUrl}"" style=""display: inline-block; background-color: #17a2b8; color: #fff; text-decoration: none; padding: 10px 20px; font-size: 18px; border-radius: 5px; margin-top: 40px;"">Igénylés megtekintése</a>
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
                        From = new MailAddress(smtpMail),
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
                            <a href=""{requestUrl}"" style=""display: inline-block; background-color: #17a2b8; color: #fff; text-decoration: none; padding: 10px 20px; font-size: 18px; border-radius: 5px; margin-top: 40px;"">Igénylés megtekintése</a>
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
            // E-mail szolgáltatás bekapcsolásának ellenőrzése
            if (!smtpEnable) return;

            // SMTP beállítások ellenőrzése
            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(smtpMail)) { return; }

            // SMTP szerver beállítása
            var smtpClient = new SmtpClient(smtpServer)
            {
                Port = smtpPort,
                EnableSsl = false,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            // Igényléssel kapcsolatos felhasználók adatainak lekérdezése
            User? creator = await GetUserById(requestData.CreatedByUserId);
            User? createdFor = await GetUserById(requestData.CreatedForUserId);

            // Dinamikus elérési útvonal legenerálása
            var httpRequest = _httpContextAccessor.HttpContext.Request;
            var requestUrl = $"{httpRequest.Scheme}://{httpRequest.Host}/Requests/Details/{requestData.RequestId}";

            // Felhasználó értesítése
            if (!string.IsNullOrEmpty(createdFor.Email) && creator.UserId != createdFor.UserId)
            {
                try
                {
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(smtpMail),
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
                            <a href=""{requestUrl}"" style=""display: inline-block; background-color: #17a2b8; color: #fff; text-decoration: none; padding: 10px 20px; font-size: 18px; border-radius: 5px; margin-top: 40px;"">Igénylés megtekintése</a>
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