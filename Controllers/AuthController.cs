using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Voltix.AuthMicroservice.Interfaces;
using Voltix.AuthMicroservice.Interfaces.Http;
using Voltix.AuthMicroservice.Interfaces.Options;
using Voltix.AuthMicroservice.Services;
using Voltix.NotificationMicroservice.Protos;
using Voltix.Shared.Interfaces.Http;


namespace Voltix.AuthMicroservice.Controllers;

[Route("auth")]
[ApiController]
public class AuthController(
    IAuthService authService, 
    IOptions<ICookiesOptions> cookiesOptions, 
    NotificationProto.NotificationProtoClient notificationClient, 
    IJwtService jwtService
) : ControllerBase {
    private readonly IAuthService _authService = authService;
    private readonly ICookiesOptions _cookiesOptions = cookiesOptions.Value;
    private readonly NotificationProto.NotificationProtoClient _notificationClient = notificationClient;
    private readonly IJwtService _jwtService = jwtService;

    [HttpPost("login")]
    public async Task<ActionResult> LoginAsync([FromBody] ILoginRequest request) {
        var user = await _authService.LoginAsync(request.Email, request.Password);
        if (user == null) {
            return BadRequest(
                new IError {
                    Message = "Incorrect email or password"
                }
            );
        }

        if (user.IsBanned) {
            return BadRequest(new IError {
                Message = "User is banned"
            });
        }

        if (!user.IsEmailConfirmed) {
            await _notificationClient.SendConfirmEmailNotificationAsync(
                new SendConfirmEmailNotificationRequest {
                    Email = user.Email,
                    Token = _jwtService.GenerateConfirmEmailToken(
                        new ConfirmEmailTokenPayload {
                            UserId = user.Id
                        }
                    )
                }
            );

            return BadRequest(new IError {
                Message = "Mail not confirmed"
            });
        }

        HttpContext.Response.Cookies.Append(
            _cookiesOptions.Token.Name,
            _jwtService.GenerateAuthToken(
                new AuthTokenPayload {
                    UserId = user.Id,
                    RoleId = user.RoleId
                }
            ),
            new CookieOptions {
                Expires = DateTimeOffset.UtcNow.AddSeconds(_cookiesOptions.Token.Expires),
                Domain = _cookiesOptions.Token.Domain,
                HttpOnly = true,
                Secure = true
            }
        );

        return Ok();
    }


    [HttpPost("register")]
    public async Task<ActionResult> RegisterAsync([FromBody] IRegisterRequest request) {
        if (await _authService.RegisterAsync(request.Email, request.Name, request.Surname, request.Patronymic, request.Password) == false) {
            return BadRequest(
                new IError {
                    Message = "User with this email already exists"
                }
            );
        }

        return Ok();
    }


    [HttpPost("email/confirm")]
    public async Task<ActionResult> ConfirmEmailAsync([FromBody] IConfirmEmailRequest request) {
        if (await _authService.ConfirmEmailAsync(request.Token) == false) {
            return BadRequest(
                new IError {
                    Message = "Invalid token"
                }
            );
        }

        return Ok();
    }


    [HttpPost("password/recovery/request")]
    public async Task<ActionResult> RequestRecoveryPasswordAsync([FromBody] IRequestRecoveryPasswordRequest request) {
        if (await _authService.RequestRecoveryPasswordAsync(request.Email) == false) {
            return BadRequest(
                new IError {
                    Message = "User with such email does not exist"
                }
            );
        }

        return Ok();
    }


    [HttpPost("password/recovery/confirm")]
    public async Task<ActionResult> ConfirmRecoveryPasswordAsync([FromBody] IConfirmRecoveryPasswordRequest request) {
        if (await _authService.ConfirmRecoveryPasswordAsync(request.Token, request.Password) == false) {
            return BadRequest(
                new IError {
                    Message = "Invalid token"
                }
            );
        }

        return Ok();
    }
}
