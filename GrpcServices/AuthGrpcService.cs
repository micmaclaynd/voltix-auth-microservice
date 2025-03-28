using Grpc.Core;
using Voltix.AuthMicroservice.Protos;
using Voltix.AuthMicroservice.Services;


namespace Voltix.AuthMicroservice.GrpcServices;

public class AuthGrpcService(IJwtService jwtService) : AuthProto.AuthProtoBase {
    private readonly IJwtService _jwtService = jwtService;

    public override Task<VerifyAuthTokenResponse> VerifyAuthToken(VerifyAuthTokenRequest request, ServerCallContext context) {
        var payload = _jwtService.VerifyAuthToken(request.Token) ?? throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid token"));

        return Task.FromResult(
            new VerifyAuthTokenResponse {
                UserId = payload.UserId,
                RoleId = payload.RoleId
            }
        );
    }
}