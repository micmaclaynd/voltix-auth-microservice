namespace Voltix.AuthMicroservice.Interfaces.Options;

public class IJwtOptions {
    public class IAuthTokenOptions {
        public class IFieldsOptions {
            public required string UserId { get; set; }
            public required string RoleId { get; set; }
        }

        public required double Expires { get; set; }
        public required string SecurityKey { get; set; }
        public required IFieldsOptions Fields { get; set; }
    }

    public class IConfirmEmailTokenOptions {
        public class IFieldsOptions {
            public required string UserId { get; set; }
        }

        public required double Expires { get; set; }
        public required string SecurityKey { get; set; }
        public required IFieldsOptions Fields { get; set; }
    }

    public class IRecoveryPasswordTokenOptions {
        public class IFieldsOptions {
            public required string UserId { get; set; }
        }

        public required double Expires { get; set; }
        public required string SecurityKey { get; set; }
        public required IFieldsOptions Fields { get; set; }
    }

    public required IAuthTokenOptions AuthToken { get; set; }
    public required IConfirmEmailTokenOptions ConfirmEmailToken { get; set; }
    public required IRecoveryPasswordTokenOptions RecoveryPasswordToken { get; set; }
    public required string EncryptionAlgorithm { get; set; }
}
