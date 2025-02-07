using Microsoft.EntityFrameworkCore;
using ShoeGrabCommonModels.Contexts;
using ShoeGrabMonolith.Extensions;
using ShoeGrabOrderManagement.Database.Mappers;
using ShoeGrabUserManagement.Grpc;
using ShoeGrabUserManagement.Services;

var builder = WebApplication.CreateBuilder(args);

//Controllers
builder.Services.AddControllers();

builder.SetupKestrel();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddGrpc();
builder.Services.AddAutoMapper(typeof(GrpcMappingProfile));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();

//Contexts
builder.Services.AddDbContextPool<UserContext>(opt =>
  opt.UseNpgsql(
    builder.Configuration.GetConnectionString("PostgreSQL"),
    o => o
      .SetPostgresVersion(17, 0)));

//Services
builder.Services.AddScoped<IPasswordManagement, PasswordManagement>();
builder.Services.AddScoped<ITokenService, JWTAuthenticationService>();

//Security
builder.Services.AddAuthorization();
builder.AddJWTAuthenticationAndAuthorization();

////APP PART////
var app = builder.Build();

//Migrations
app.ApplyMigrations();

//Security
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAllOrigins");
}

app.MapGrpcService<UserManagementService>();

app.MapControllers();

app.Run();