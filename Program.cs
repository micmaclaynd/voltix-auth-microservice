using Voltix.AuthMicroservice.GrpcServices;
using Voltix.AuthMicroservice.Interfaces.Options;
using Voltix.AuthMicroservice.Services;
using Voltix.NotificationMicroservice.Protos;
using Voltix.Shared.Extensions;
using Voltix.UserMicroservice.Protos;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<IJwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<ICookiesOptions>(builder.Configuration.GetSection("Cookies"));

builder.Services.AddGrpc().AddJsonTranscoding();
builder.Services.AddGrpcSwagger();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddGrpcClient<UserProto.UserProtoClient>(options => {
    options.Address = new Uri("https://voltix-user-microservice");
});

builder.Services.AddGrpcClient<NotificationProto.NotificationProtoClient>(options => {
    options.Address = new Uri("https://voltix-notification-microservice");
});

var app = builder.Build();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGrpcService<AuthGrpcService>();

app.Run();