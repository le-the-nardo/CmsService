using CmsService.Api.Auth;
using CmsService.Api.Auth.Options;
using CmsService.Api.Security;
using CmsService.Domain.Services;
using CmsService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add Swagger/OpenAPI services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<WriteDbContext>(options =>
{
    options.UseSqlite("Data Source=cms.db");
});

builder.Services.AddDbContext<ReadDbContext>(options =>
{
    options.UseSqlite("Data Source=cms.db");
});

builder.Services.AddScoped<EntityDomainService>();
builder.Services.AddScoped<IUserContextAccessor, UserContextAccessor>();

builder.Services
    .AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, UserBasicAuthenticationHandler>(
        "UserBasic", null)
    .AddScheme<AuthenticationSchemeOptions, CmsBasicAuthHandler>(
        "CmsBasic", null);

builder.Services.Configure<UserBasicAuthOptions>(
    builder.Configuration.GetSection("Authentication:Api"));

builder.Services.Configure<CmsBasicAuthOptions>(
    builder.Configuration.GetSection("Authentication:Cms"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
