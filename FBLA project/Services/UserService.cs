using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FBLA_project
{
    public static class UserService
    {

        private static readonly string _path = @".\Private\Users.json";
        private static readonly IDataProtectionProvider _dataProtectionProvider;
        private static readonly string _sessionTokenKey = "Session Token Key";
        private static readonly string _userStoragekey = "User Storage Key";


        private static readonly IDataProtector _userProtector;

        private static readonly Dictionary<string, string> _activeSessionTokens = new();
        static UserService()
        {
            _dataProtectionProvider = DataProtectionProvider.Create("ApplicationName");
            _userProtector = _dataProtectionProvider.CreateProtector(_userStoragekey);
        }
        //provides user authentication
        public static User? AuthenticateUser(string username, string password)
        {
            //gets the users
            List<User>? userList = getUsers();
            //if there are no users, return null
            if (userList is null) { return null; }
            //iterates through the list of users
            foreach (User user in userList)
            {
                //authenticats the password
                if (user.UnprotectedInfo.Username == username && !(AuthenticatePassword(password, user.ProtectedInfo.Salt, user.ProtectedInfo.PasswordHash)))
                {
                    throw new Exception("User does not exist");
                }
                if (user.UnprotectedInfo.Username == username && AuthenticatePassword(password, user.ProtectedInfo.Salt, user.ProtectedInfo.PasswordHash))
                {
                    return user;
                }
            }
            return null;
        }

        //generates a session token and maps it to the list of valid session tokens
        public static string GenerateSessionToken(User user)
        {
            string id = user.Id.ToString();
            string token = GenerateRandomBytes(10);

            _activeSessionTokens.Add(token, id);
            return token;
        }

        //takes in the context avaailable and provides a user from the info
        public static User? GetUserFromHttpContext(HttpContext httpContext)
        {
            string? token = httpContext.Session.GetString("SessionToken");

            if (token is null)
            {
                return null;
            }
            string? userid;
            if (_activeSessionTokens.TryGetValue(token, out userid))
            {
                if (userid is not null)
                {
                    User? user = GetUserById(userid);
                    return user;
                }
            }
            return null;
        }
        //creates a new user and constructs a user object to be appended onto the list of users
        public static void CreateNewUser(string password, UnprotectedData unprotected)
        {
            //generate the password hash
            string salt = GenerateRandomBytes(4);
            string hash = HashPasword(password, salt);

            ProtectedData protectedData = new ProtectedData { IsAdmin = false, PasswordHash = hash, Salt = salt };

            //generates the user object
            User user = new()
            {
                ProtectedInfo = protectedData,
                UnprotectedInfo = unprotected,
                Id = GenerateUserId()
            };


            List<User> users = getUsers() ?? new List<User>();
            users.Add(user);
            setUsers(users);
        }

        public static void ModifyUser(string userId, User newUser)
        {
            //gets the users
            List<User>? users = getUsers();
            if (users is null)
            {
                return;
            }
            for (int i = 0; i < users.Count; i++)
            {
                User user = users[i];
                if (user.Id == userId)
                {
                    users[i] = newUser;
                    setUsers(users);
                    return;
                }
            }
        }

        public static string? GetUserIdByUsername(string username)
        {
            List<User>? users = getUsers();
            if (users == null) return null;
            foreach (User user in users)
            {
                if (user.UnprotectedInfo.Username == username) { return user.Id; }
            }
            return null;
        }

        public static User? GetUserById(string id)
        {
            List<User>? users = getUsers();
            if (users is null) { return null; }

            foreach (User user in users)
            {
                if (user.Id == id)
                {
                    return user;
                }
            }
            return null;
        }
        private static List<User>? getUsers()
        {
            string encrptedUserData = File.ReadAllText(_path);
            if (string.IsNullOrEmpty(encrptedUserData)) { return null; }
            //    string rawUserData = _userProtector.Unprotect(encrptedUserData);
            string rawUserData = encrptedUserData;
            List<User>? users = JsonSerializer.Deserialize<List<User>>(rawUserData);

            return users;
        }

        private static void setUsers(List<User> users)
        {
            string rawUserData = JsonSerializer.Serialize(users, new JsonSerializerOptions() { WriteIndented = true });
            // string encriptedUserData = _userProtector.Protect(rawUserData);
            string encriptedUserData = rawUserData;
            File.WriteAllText(_path, encriptedUserData);

        }
        private static string GenerateUserId()
        {
            byte[] randomBytes = new byte[sizeof(Int16)];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            return BitConverter.ToInt16(randomBytes).ToString();

        }

        private static bool AuthenticatePassword(string password, string salt, string hash)
        {
            var checkHash = HashPasword(password, salt);
            return hash == checkHash;
        }

        private static string GenerateRandomBytes(int numBytes)
        {
            byte[] randomBytes = new byte[numBytes];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            return Convert.ToBase64String(randomBytes);
        }


        private static string HashPasword(string password, string saltString)
        {
            const int iterations = 350000;
            byte[] salt = Encoding.ASCII.GetBytes(saltString);


            HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.UTF8.GetBytes(password),
                    salt,
                    iterations,
                    hashAlgorithm,
                    40);

            return Convert.ToHexString(hash);
        }
    }
}

