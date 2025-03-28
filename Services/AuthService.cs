using Grpc.Core;
using Voltix.AuthMicroservice.Interfaces;
using Voltix.NotificationMicroservice.Protos;
using Voltix.UserMicroservice.Protos;


namespace Voltix.AuthMicroservice.Services;

public interface IAuthService {
    public Task<GetUserResponse?> LoginAsync(string email, string password);
    public Task<bool> RegisterAsync(string email, string name, string surname, string patronymic, string password);

    public Task<bool> ConfirmEmailAsync(string token);
    public Task<bool> RequestRecoveryPasswordAsync(string email);
    public Task<bool> ConfirmRecoveryPasswordAsync(string token, string password);
}

public class AuthService(
    IJwtService jwtService,
    IPasswordService passwordService,
    UserProto.UserProtoClient userClient,
    NotificationProto.NotificationProtoClient notificationClient
) : IAuthService {
    private readonly IJwtService _jwtService = jwtService;
    private readonly IPasswordService _passwordService = passwordService;
    private readonly UserProto.UserProtoClient _userClient = userClient;
    private readonly NotificationProto.NotificationProtoClient _notificationClient = notificationClient;


    public async Task<GetUserResponse?> LoginAsync(string email, string password) {
        try {
            var user = await _userClient.GetUserByEmailAsync(
                new GetUserByEmailRequest {
                    Email = email
                }
            );

            if (!_passwordService.VerifyPassword(password, user.PasswordHash)) {
                return null;
            }

            return user;
        } catch (RpcException error) when (error.StatusCode == StatusCode.NotFound) {
            return null;
        }
    }

    public async Task<bool> RegisterAsync(string email, string name, string surname, string patronymic, string password) {
        try {
            var user = await _userClient.CreateUserAsync(
                new CreateUserRequest {
                    Email = email,
                    Name = name,
                    Surname = surname,
                    Patronymic = patronymic,
                    PasswordHash = _passwordService.HashPassword(password)
                }
            );

            await _notificationClient.SendConfirmEmailNotificationAsync(
                new SendConfirmEmailNotificationRequest {
                    Email = email,
                    Token = _jwtService.GenerateConfirmEmailToken(
                        new ConfirmEmailTokenPayload {
                            UserId = user.Id
                        }
                    )
                }
            );

            return true;
        } catch (RpcException error) when (error.StatusCode == StatusCode.AlreadyExists) {
            return false;
        }
    }

    public async Task<bool> ConfirmEmailAsync(string token) {
        var payload = _jwtService.VerifyConfirmEmailToken(token);

        if (payload == null) {
            return false;
        }

        await _userClient.ConfirmUserEmailAsync(
            new ConfirmUserEmailRequest {
                Id = payload.UserId
            }
        );

        return true;
    }

    public async Task<bool> RequestRecoveryPasswordAsync(string email) {
        try {
            var user = await _userClient.GetUserByEmailAsync(
                new GetUserByEmailRequest {
                    Email = email
                }
            );

            await _notificationClient.SendRecoveryPasswordNotificationAsync(
                new SendRecoveryPasswordNotificationRequest {
                    TelegramId = user.TelegramId,
                    Email = email,
                    Token = _jwtService.GenerateRecoveryPasswordToken(
                        new RecoveryPasswordTokenPayload {
                            UserId = user.Id
                        }
                    )
                }
            );

            return true;
        } catch (RpcException error) when (error.StatusCode == StatusCode.NotFound) {
            return false;
        }
    }

    public async Task<bool> ConfirmRecoveryPasswordAsync(string token, string password) {
        var payload = _jwtService.VerifyRecoveryPasswordToken(token);

        if (payload == null) {
            return false;
        }

        await _userClient.ChangeUserPasswordAsync(
            new ChangeUserPasswordRequest {
                Id = payload.UserId,
                PasswordHash = _passwordService.HashPassword(password)
            }
        );

        return true;
    }
}
