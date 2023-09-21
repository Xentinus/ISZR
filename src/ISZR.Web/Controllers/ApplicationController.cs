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
        private readonly LdapService _ldapService;

        private readonly bool smtpEnable;
        private readonly bool ldapEnable;

        public ApplicationController(DataContext context, IDatabaseStatusService databaseStatusService, IConfiguration configuration, IHostEnvironment environment, LdapService ldapService)
        {
            _context = context;
            _databaseStatusService = databaseStatusService;
            _configuration = configuration;
            _environment = environment;
            _ldapService = ldapService;

            smtpEnable = configuration.GetValue<bool>("ApplicationSettings:SNMP_ENABLE");
            ldapEnable = configuration.GetValue<bool>("ApplicationSettings:LDAP_ENABLE");
        }

        /// <summary>
        /// Rendszer állapotfelméréséről jelentés
        /// </summary>
        [Authorize(Policy = "Administrator")]
        public IActionResult Index()
        {
            var viewModel = new HealthChecksViewModel { };

            // Az elmúlt 24 órában bejelentkezett felhasználók részére
            var setUserUptime = DateTime.Now.AddDays(-1);

            // Az elmúlt 1 hónap igényléseinek részére
            var setRequestTime = DateTime.Now.AddMonths(-1);

            // Adatbázis ellenőrzése
            viewModel.DatabaseStatus = _databaseStatusService.IsDatabaseOnline();

            // Bejelentkezett felhasználók statisztikája
            viewModel.LoggedUserToday = _context.Users.Count(u => u.LastLogin >= setUserUptime);

            // Szolgáltatások állapotai
            viewModel.EmailServiceStatus = smtpEnable;
            viewModel.LDAPServiceStatus = ldapEnable;
            viewModel.LDAPConnectionStatus = _ldapService.IsLdapConnectionSuccessful();

            // Igénylések statisztika
            viewModel.RequestAll = _context.Requests.Count(r => r.CreatedDateTime >= setRequestTime);
            viewModel.RequestAllDone = _context.Requests.Where(r => r.CreatedDateTime >= setRequestTime).Count(r => r.Status == "Végrehajtva");
            viewModel.RequestAllProgress = _context.Requests.Where(r => r.CreatedDateTime >= setRequestTime).Count(r => r.Status == "Folyamatban");
            viewModel.RequestAllDenied = viewModel.RequestAll - (viewModel.RequestAllDone + viewModel.RequestAllProgress);

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

        /// <summary>
        /// Appsettings értékeinek betöltése
        /// </summary>
        public IActionResult Settings()
        {
            // Értékek beolvasása a JSON fájlból
            var settings = _configuration.GetSection("ApplicationSettings").Get<ApplicationSettingsViewModel>();
            return View(settings);
        }


        /// <summary>
        /// Appsettings fájl értékeinek átírása
        /// </summary>
        /// <param name="model">appsettings értékek</param>
        [HttpPost]
        public IActionResult Settings(ApplicationSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Fájl elérési útvonalának beállítása environment típus alapján
                    var configFilePath = _environment.IsProduction() ? Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json") : Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Development.json");

                    // Fájl beolvasása
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

                    // Amennyiben nem létezik, annak létrehozása
                    if (configObject["ApplicationSettings"] == null)
                    {
                        configObject["ApplicationSettings"] = new JObject();
                    }

                    // Beolvasott fájl érintett szekciójának betöltése
                    var appSettingsSection = configObject["ApplicationSettings"];

                    // Értékek frissítése
                    if (appSettingsSection != null)
                    {
                        appSettingsSection["SNMP_ENABLE"] = model.SNMP_ENABLE;
                        appSettingsSection["SNMP_SERVER"] = model.SNMP_SERVER;
                        appSettingsSection["SNMP_MAIL"] = model.SNMP_MAIL;
                        appSettingsSection["SNMP_USERNAME"] = model.SNMP_USERNAME;
                        appSettingsSection["SNMP_PASSWORD"] = model.SNMP_PASSWORD;
                        appSettingsSection["LDAP_ENABLE"] = model.LDAP_ENABLE;
                        appSettingsSection["LDAP_SERVER"] = model.LDAP_SERVER;
                        appSettingsSection["LDAP_BIND_USER"] = model.LDAP_BIND_USER;
                        appSettingsSection["LDAP_BIND_PASSWORD"] = model.LDAP_BIND_PASSWORD;
                        appSettingsSection["LDAP_BASE"] = model.LDAP_BASE;
                    }

                    // Visszaalakítás JSON formává
                    var updatedConfigJson = configObject.ToString(Formatting.Indented);

                    // Elkészült JSON elmentése
                    System.IO.File.WriteAllText(configFilePath, updatedConfigJson);
                }
                catch (Exception ex)
                {
                    // Amennyiben hiba van arról tájékoztatni
                    return View("Error", new ErrorViewModel { Message = ex.Message });
                }

                // Felhasználó tájékoztatása
                return RedirectToAction("SettingsComplete");
            }

            // Hibás model esetén újrabetöltés
            return View(model);
        }

        /// <summary>
        /// Sikeres tájékoztatás
        /// </summary>
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