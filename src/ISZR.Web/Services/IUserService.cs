namespace ISZR.Web.Services
{
    public interface IUserService
    {
        string? GetUsername();
        void SetUsername(string? username);
    }
    public class UserService : IUserService
    {
        private string? _username;

        public string? GetUsername()
        {
            return _username == null ? "Felhasználói profil" : _username;
        }

        public void SetUsername(string? username)
        {
            _username = username;
        }
    }
}
