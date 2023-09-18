using ISZR.Web.Models;
using ISZR.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ISZR.Web.Controllers
{
    /// <summary>
    /// /Application/? Controller
    /// </summary>
    [Authorize(Policy = "Administrator")]
    public class ApplicationController : Controller
    {
        private readonly DataContext _context;
        private readonly IDatabaseStatusService _databaseStatusService;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;

        public ApplicationController(DataContext context, IDatabaseStatusService databaseStatusService, IConfiguration configuration, IHostEnvironment environment)
        {
            _context = context;
            _databaseStatusService = databaseStatusService;
            _configuration = configuration;
            _environment = environment;
        }

        /// <summary>
        /// Rendszer állapotfelméréséről jelentés
        /// </summary>
        [Authorize(Policy = "Administrator")]
        public IActionResult Index()
        {
            var viewModel = new HealthChecksViewModel { };

            // Adatbázis ellenőrzése
            viewModel.DatabaseStatus = _databaseStatusService.IsDatabaseOnline();

            // Bejelentkezett felhasználók statisztikája
            viewModel.LoggedUserToday = _context.Users.Count(u => u.LastLogin.Date == DateTime.Now.Date);

            // Igénylések statisztika
            viewModel.RequestAll = _context.Requests.Count();
            viewModel.RequestAllDone = _context.Requests.Count(r => r.Status == "Végrehajtva");
            viewModel.RequestAllProgress = _context.Requests.Count(r => r.Status == "Folyamatban");
            viewModel.RequestAllDenied = viewModel.RequestAll - (viewModel.RequestAllDone + viewModel.RequestAllProgress);
            viewModel.RequestClosedToday = _context.Requests.Count(r => r.ClosedDateTime.Date == DateTime.Now.Date);
            viewModel.RequestClosedMonth = _context.Requests.Count(r => r.ClosedDateTime.Year == DateTime.Now.Year && r.ClosedDateTime.Month == DateTime.Now.Month);
            viewModel.RequestOpenToday = _context.Requests.Count(r => r.CreatedDateTime.Date == DateTime.Now.Date);
            viewModel.RequestOpenMonth = _context.Requests.Count(r => r.CreatedDateTime.Year == DateTime.Now.Year && r.CreatedDateTime.Month == DateTime.Now.Month);

            // Adat statisztika
            viewModel.ActiveUsers = _context.Users.Count(u => !u.IsArchived);
            viewModel.ActiveParkings = _context.Parkings.Count(p => !p.IsArchived);
            viewModel.ActiveGroups = _context.Groups.Count(g => !g.IsArchived);
            viewModel.ActiveWindowsPermission = _context.Permissions.Count(p => !p.IsArchived && p.Type == "Windows");
            viewModel.ActiveFonixPermission = _context.Permissions.Count(p => !p.IsArchived && p.Type == "Főnix 3");
            viewModel.ActivePhones = _context.Phones.Count(p => !p.IsArchived);
            viewModel.ActiveNotUsedPhone = _context.Phones.Count(p => !p.IsArchived && p.PhoneUserId == null);

            // Százalékok kiszámítása
            ViewData["donePercent"] = CalculatePercent(viewModel.RequestAllDone, viewModel.RequestAll);
            ViewData["inProgressPercent"] = CalculatePercent(viewModel.RequestAllProgress, viewModel.RequestAll);
            ViewData["deniedPercent"] = CalculatePercent(viewModel.RequestAllDenied, viewModel.RequestAll);

            // Felület megjelenítése
            return View(viewModel);
        }

        public IActionResult Settings()
        {
            var settings = _configuration.GetSection("ApplicationSettings").Get<ApplicationSettingsViewModel>();
            return View(settings);
        }

        [HttpPost]
        public IActionResult Settings(ApplicationSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var configFilePath = _environment.IsProduction() ? Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json") : Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Development.json");

                    // Read the existing configuration JSON or create an empty JObject if the file doesn't exist
                    JObject configObject;
                    if (System.IO.File.Exists(configFilePath))
                    {
                        var configJson = System.IO.File.ReadAllText(configFilePath);
                        configObject = JObject.Parse(configJson);
                    }
                    else
                    {
                        configObject = new JObject();
                    }

                    // Ensure the "ApplicationSettings" section exists
                    if (configObject["ApplicationSettings"] == null)
                    {
                        configObject["ApplicationSettings"] = new JObject();
                    }

                    // Get the "ApplicationSettings" section
                    var appSettingsSection = configObject["ApplicationSettings"];

                    // Update only the changed properties
                    if (appSettingsSection != null)
                    {
                        appSettingsSection["SNMP_ENABLE"] = model.SNMP_ENABLE;
                        appSettingsSection["SNMP_SERVER"] = model.SNMP_SERVER;
                        appSettingsSection["SNMP_MAIL"] = model.SNMP_MAIL;
                        appSettingsSection["SNMP_USERNAME"] = model.SNMP_USERNAME;
                        appSettingsSection["SNMP_PASSWORD"] = model.SNMP_PASSWORD;
                        appSettingsSection["LDAP_ENABLE"] = model.LDAP_ENABLE;
                        appSettingsSection["LDAP_SERVER"] = model.LDAP_SERVER;
                        appSettingsSection["LDAP_USERNAME"] = model.LDAP_USERNAME;
                        appSettingsSection["LDAP_PASSWORD"] = model.LDAP_PASSWORD;
                    }

                    // Serialize the modified JObject back to JSON with indentation
                    var updatedConfigJson = configObject.ToString(Formatting.Indented);

                    // Write the updated JSON back to the configuration file
                    System.IO.File.WriteAllText(configFilePath, updatedConfigJson);
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that may occur during the process.
                    // Log the error, display an error message, or take appropriate action.
                    return View("Error", new ErrorViewModel { Message = ex.Message });
                }

                // Felhasználó tájékoztatása
                return RedirectToAction("SettingsComplete");
            }

            return View(model);
        }

        public IActionResult SettingsComplete()
        {
            return View();
        }

        /// <summary>
        /// Százalék kiszámítása
        /// </summary>
        /// <param name="count">Megadott érték</param>
        /// <param name="max">Maximális mennyiség</param>
        private int? CalculatePercent(int count, int max)
        {
            return (int)Math.Round((double)(100 * count) / max);
        }
    }
}