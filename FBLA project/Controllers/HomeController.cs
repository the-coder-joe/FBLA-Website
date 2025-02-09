using FBLA_project.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Extensions.FileProviders;
using System.Diagnostics;
using System.Security.Permissions;
using System.Text.Json;

namespace FBLA_project
{
    public class HomeController : Controller
    {
        private const string jobDirectory = @".\JobFolder";

        public IActionResult Index()
        {
            User? user = UserService.GetUserFromHttpContext(HttpContext);

            if (user is null)
            {
                return View();
            }

            return View(new BaseModel { UnprotectedData = user.UnprotectedInfo });
        }

        #region AdminLogin

        //distributes actual login page
        //public ActionResult Login()
        //{
        //    User? user = UserService.GetUserFromHttpContext(HttpContext);
        //    if (user is null)
        //    { return View(); }

        //    return RedirectToAction("Account", "Home");
        //}

        //public IActionResult Account()
        //{
        //    User? user = UserService.GetUserFromHttpContext(HttpContext);
        //    if (user is null)
        //    { return RedirectToAction("Home", "Login"); }

        //    if(user.ProtectedInfo.IsAdmin)
        //    {
        //        return RedirectToAction("AdminView");
        //    }

        //    return View(new AccountModel { UnprotectedData = user.UnprotectedInfo });
        //}

        //handles form submission
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Login(LoginModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        User? user = null;
        //        try
        //        {
        //            user = UserService.AuthenticateUser(model.Username, model.Password);
        //        }
        //        catch (Exception ex)
        //        {
        //            if (ex.Message == "User does not exist")
        //            {
        //                model.Message = "This user does not exist, please create an account.";
        //            }
        //        }
        //        if (user is null)
        //        {
        //            model.Message = "Your password is incorrect";
        //            return View(model);
        //        }

        //        var token = UserService.GenerateSessionToken(user);
        //        HttpContext.Session.SetString("SessionToken", token);

        //        if (user.ProtectedInfo.IsAdmin)
        //        {
        //            return RedirectToAction("AdminView", "Home");
        //        }
        //        return RedirectToAction("Account", "Home");
        //    }
        //    return View();
        //}

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        public IActionResult AdminView()
        {
            User? user = UserService.GetUserFromHttpContext(HttpContext);
            if (user is null)
            { return RedirectToAction("Index", "Home"); }

            if (user.ProtectedInfo.IsAdmin)
            {
                List<ProcessedApplication> apps = [];

                //read the applications from file
                using (StreamReader jsonStream = new(Path.Combine(jobDirectory, "Applications.json")))
                {
                    string jsonString = jsonStream.ReadToEnd();
                    apps = JsonSerializer.Deserialize<List<ProcessedApplication>>(jsonString) ?? [];
                }

                //create the model for the admin view and return it
                AdminViewModel model = new() { Applications = apps };
                return View(model);
            }
            return RedirectToAction("Index", "Home");
        }

        #endregion AdminLogin

        public IActionResult MyGarage()
        {
            User? user = UserService.GetUserFromHttpContext(HttpContext);
            if (user is null)
            { return View(new MyGarageModel() ); }

            HttpContext.Response.Headers.Append("CACHE-CONTROL", "NO-CACHE");
            return View(new MyGarageModel { UnprotectedData = user.UnprotectedInfo });
        }

        public IActionResult CreateAccount()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAccount(AccountCreationModel model)
        {
            if (ModelState.IsValid)
            {
                //verify password is valid
                if (model.Password == model.ConfirmPassword)
                {
                    UnprotectedData userInfo = model.UnprotectedInfo;

                    UserService.CreateNewUser(model.Password, userInfo);
                    model.Message = "Account has successfully been created";
                    Response.Headers.Append("REFRESH", "1;URL=/Home/Login");

                    return View(model);
                }


            }
            model.Message = "Something Went Wrong";
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Sources()
        {
            return View();
        }
    }
}