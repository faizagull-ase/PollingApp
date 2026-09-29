using Microsoft.AspNetCore.Mvc;

namespace PulsePoll.Mvc.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
