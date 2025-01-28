using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VsModDb.Data.Entities;

namespace VsModDb.Controllers;

#if DEBUG

[ApiController]
[Route("api/v1/test")]
public class TestController(SignInManager<User> signInManager, UserManager<User> userManager) : ControllerBase
{
    [HttpGet]
    public string Ping() => "Pong!";

    [HttpPost("login")]
    public async Task<Results<Ok, BadRequest>> Login([FromQuery] string username)
    {
        var user = await userManager.FindByNameAsync(username);

        if (user is null)
        {
            return TypedResults.BadRequest();
        }

        await signInManager.SignInAsync(new User
        {
            UserName = username
        }, isPersistent: true);

        return TypedResults.Ok();
    }

    [HttpPost("create-user")]
    public async Task CreateUser([FromBody] CreateTestUserRequest request)
    {
        var user = new User()
        {
            Email = request.Username,
            UserName = request.Username,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(user);

        await signInManager.SignInAsync(user, true);
    }
}

public class CreateTestUserRequest
{
    public string Username { get; set; }
}

#endif