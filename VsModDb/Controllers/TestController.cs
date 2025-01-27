using Microsoft.AspNetCore.Mvc;

namespace VsModDb.Controllers;

[ApiController]
[Route("api/v1/test")]
public class TestController : ControllerBase
{
    [HttpGet]
    public string Ping() => "Pong!";
}
