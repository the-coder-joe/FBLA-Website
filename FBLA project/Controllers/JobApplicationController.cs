using Microsoft.AspNetCore.Mvc;

namespace FBLA_project.Controllers
{
    public class JobApplicationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Apply()
        {
            ApplicationModel model;

            model = new ApplicationModel()
            {
                Job = new Job(),
                Completed = false
            };
            return View(model);
        }

        public IActionResult SubmitApplication()
        {
            return View();
        }

        public IActionResult Login(LoginModel loginModel)
        {
            if (loginModel != null)
                if (ModelState.IsValid)
                    if (loginModel.Username == "admin")
                        if (loginModel.Password == "password")
                            return RedirectToAction();

            return View();
        }

        public IActionResult () {
    }
}
