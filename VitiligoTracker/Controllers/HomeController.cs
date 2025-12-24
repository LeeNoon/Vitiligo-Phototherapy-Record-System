using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VitiligoTracker.Models;

namespace VitiligoTracker.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
