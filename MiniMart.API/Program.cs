using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MiniMart.API.Data;
using MiniMart.API.Options;
using MiniMart.API.Security;

var builder = WebApplication.CreateBuilder(args);

// 1. Thêm Controllers
builder.Services.AddControllers();

// 2. Cấu hình Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Đăng ký AppDbContext
// Lựa chọn "LocalDb" hoặc "TailscaleDb" tùy vào môi trường phát triển hiện tại
var connectionString = builder.Configuration.GetConnectionString("LocalDb");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 4. Cấu hình xác thực JWT cho chức năng đăng nhập
var jwtSection = builder.Configuration.GetSection(JwtSettings.SectionName);
var jwtSettings = jwtSection.Get<JwtSettings>() ?? new JwtSettings();
jwtSettings.Validate();

builder.Services.Configure<JwtSettings>(jwtSection);
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Cấu hình HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Tạo tài khoản Admin mặc định nếu database chưa có, để có thể đăng nhập ngay sau khi apply migration.
await DbSeeder.SeedDefaultAdminAsync(app.Services, app.Logger);

app.Run();