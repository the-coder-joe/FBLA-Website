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
    }
}
