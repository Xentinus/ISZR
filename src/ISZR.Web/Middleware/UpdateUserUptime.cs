namespace ISZR.Web.Middleware
{
    /// <summary>
    /// Felhasználó utolsó megtekintési idejének módosítása az adatbázisban
    /// </summary>
    public class UpdateUserUptime : IMiddleware
    {
        private readonly DataContext _context;
        private readonly IUserService _userService;

        public UpdateUserUptime(DataContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        /// <summary>
        /// Felhasználó megtekintési idejének módosítása az adatbázisban
        /// </summary>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Felhasználó megkeresése az adatbázisban
            var user = await _context.Users.FirstOrDefaultAsync(m => m.Username == context.User.Identity.Name);

            if (user != null)
            {
                try
                {
                    // Felhasználó idejének frissítése az aktuális idővel
                    user.LastLogin = DateTime.Now;

                    // Felhasználó értékeinek frissítése az adatbázisban
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (UserExists(user.UserId))
                    {
                        throw;
                    }
                }
            }

            // Felhasználó mentése
            _userService.SetUsername(user?.DisplayName);

            // A rendszer továbbléptetése
            await next(context);
        }

        /// <summary>
        /// Felhasználói azonosító megkeresése a rendszerben
        /// </summary>
        /// <param name="id">Felhasználói azonosító</param>
        /// <returns>Létezik e a felhasználói azonosító (igaz/hamis)</returns>
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
