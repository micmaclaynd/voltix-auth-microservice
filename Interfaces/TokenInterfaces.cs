namespace Voltix.AuthMicroservice.Interfaces;

public class AuthTokenPayload {
    public required int UserId { get; set; }
    public required int RoleId { get; set; }
}

public class ConfirmEmailTokenPayload {
    public required int UserId { get; set; }
}

public class RecoveryPasswordTokenPayload {
    public required int UserId { get; set; }
}
