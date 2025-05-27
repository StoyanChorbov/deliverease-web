using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[ApiController]
[Route("/")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("Success!");
}