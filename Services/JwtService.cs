using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Voltix.AuthMicroservice.Interfaces;
using Voltix.AuthMicroservice.Interfaces.Options;


namespace Voltix.AuthMicroservice.Services;

public interface IJwtService {
    public string GenerateAuthToken(AuthTokenPayload tokenPayload);
    public AuthTokenPayload? VerifyAuthToken(string token);

    public string GenerateConfirmEmailToken(ConfirmEmailTokenPayload tokenPayload);
    public ConfirmEmailTokenPayload? VerifyConfirmEmailToken(string token);

    public string GenerateRecoveryPasswordToken(RecoveryPasswordTokenPayload tokenPayload);
    public RecoveryPasswordTokenPayload? VerifyRecoveryPasswordToken(string token);
}

public class JwtService(IOptions<IJwtOptions> jwtOptions) : IJwtService {
    private readonly IJwtOptions _jwtOptions = jwtOptions.Value;

    public string GenerateAuthToken(AuthTokenPayload tokenPayload) {
        return GenerateToken(
            _jwtOptions.AuthToken.SecurityKey,
            new JwtPayload {
                { _jwtOptions.AuthToken.Fields.UserId, tokenPayload.UserId },
                { _jwtOptions.AuthToken.Fields.RoleId, tokenPayload.RoleId }
            },
            _jwtOptions.AuthToken.Expires
        );
    }

    public AuthTokenPayload? VerifyAuthToken(string token) {
        var tokenPayload = VerifyToken(_jwtOptions.AuthToken.SecurityKey, token);

        return tokenPayload == null ? null : new AuthTokenPayload {
            UserId = Convert.ToInt32(tokenPayload.Payload[_jwtOptions.AuthToken.Fields.UserId]),
            RoleId = Convert.ToInt32(tokenPayload.Payload[_jwtOptions.AuthToken.Fields.RoleId])
        };
    }

    public string GenerateConfirmEmailToken(ConfirmEmailTokenPayload tokenPayload) {
        return GenerateToken(
            _jwtOptions.ConfirmEmailToken.SecurityKey,
            new JwtPayload {
                { _jwtOptions.ConfirmEmailToken.Fields.UserId, tokenPayload.UserId }
            },
            _jwtOptions.ConfirmEmailToken.Expires
        );
    }

    public ConfirmEmailTokenPayload? VerifyConfirmEmailToken(string token) {
        var tokenPayload = VerifyToken(_jwtOptions.ConfirmEmailToken.SecurityKey, token);

        return tokenPayload == null ? null : new ConfirmEmailTokenPayload {
            UserId = Convert.ToInt32(tokenPayload.Payload[_jwtOptions.ConfirmEmailToken.Fields.UserId])
        };
    }

    public string GenerateRecoveryPasswordToken(RecoveryPasswordTokenPayload tokenPayload) {
        return GenerateToken(
            _jwtOptions.RecoveryPasswordToken.SecurityKey,
            new JwtPayload {
                { _jwtOptions.RecoveryPasswordToken.Fields.UserId, tokenPayload.UserId }
            },
            _jwtOptions.RecoveryPasswordToken.Expires
        );
    }

    public RecoveryPasswordTokenPayload? VerifyRecoveryPasswordToken(string token) {
        var tokenPayload = VerifyToken(_jwtOptions.RecoveryPasswordToken.SecurityKey, token);

        return tokenPayload == null ? null : new RecoveryPasswordTokenPayload {
            UserId = Convert.ToInt32(tokenPayload.Payload[_jwtOptions.ConfirmEmailToken.Fields.UserId])
        };
    }

    private string GenerateToken(string securityKey, JwtPayload tokenPayload, double expires) {
        var jwtHandler = new JwtSecurityTokenHandler();

        var symmetricSecurityKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(securityKey)
        );

        var credentials = new SigningCredentials(symmetricSecurityKey, _jwtOptions.EncryptionAlgorithm);
        var jwtHeader = new JwtHeader(credentials);

        tokenPayload["exp"] = new DateTimeOffset(
            DateTime.UtcNow.AddSeconds(expires)
        ).ToUnixTimeSeconds();

        var jwtToken = new JwtSecurityToken(
            jwtHeader,
            tokenPayload
        );

        return jwtHandler.WriteToken(jwtToken);
    }

    private JwtSecurityToken? VerifyToken(string securityKey, string token) {
        try {
            var jwtHandler = new JwtSecurityTokenHandler();

            var symmetricSecurityKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(securityKey)
            );

            var credentials = new SigningCredentials(symmetricSecurityKey, _jwtOptions.EncryptionAlgorithm);
            var jwtHeader = new JwtHeader(credentials);

            jwtHandler.ValidateToken(
                token,
                new TokenValidationParameters {
                    IssuerSigningKey = symmetricSecurityKey,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                },
                out SecurityToken securityToken
            );

            return securityToken as JwtSecurityToken;
        } catch {
            return null;
        }
    }
}
