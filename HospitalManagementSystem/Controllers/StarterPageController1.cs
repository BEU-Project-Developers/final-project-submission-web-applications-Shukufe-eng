using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class StarterPageController1 : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
