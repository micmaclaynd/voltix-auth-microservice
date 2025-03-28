namespace Voltix.AuthMicroservice.Interfaces.Http;

public class ILoginRequest {
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class IRegisterRequest {
    public required string Email { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public required string Patronymic { get; set; }
    public required string Password { get; set; }
}

public class IConfirmEmailRequest {
    public required string Token { get; set; }
}

public class IRequestRecoveryPasswordRequest {
    public required string Email { get; set; }
}

public class IConfirmRecoveryPasswordRequest {
    public required string Password { get; set; }
    public required string Token { get; set; }
}