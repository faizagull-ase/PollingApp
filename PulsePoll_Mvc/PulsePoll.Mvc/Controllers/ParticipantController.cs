using Microsoft.AspNetCore.Mvc;

namespace PulsePoll.Mvc.Controllers;

public class ParticipantController : Controller
{
    public IActionResult Join() => View();
}
