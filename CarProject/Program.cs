using System.Text;
using System.Text.Json.Serialization;
using CarProject.Data;
using CarProject.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var cloudinary = new Cloudinary(new Account("cloud", "key", "secret"));
var uploadParams = new ImageUploadParams
{
    File = new FileDescription(@"C:\path\to\file.jpg")
};
var result = cloudinary.Upload(uploadParams);

var builder = WebApplication.CreateBuilder(args);


// =========================
// Controllers + JSON
// =========================

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;

        options.JsonSerializerOptions.WriteIndented = true;
    });



// =========================
// Database
// =========================

builder.Services.AddDbContext<Base>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});



// =========================
// CORS Angular
// =========================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});



// =========================
// File Upload Size
// =========================

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit =
        30 * 1024 * 1024; // 30 MB
});



// =========================
// JWT
// =========================

var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? throw new Exception("Jwt Key missing");


builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {

                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,


                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],


                ValidAudience =
                    builder.Configuration["Jwt:Audience"],


                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    )
            };
    });



builder.Services.AddAuthorization();



// =========================
// Services
// =========================

builder.Services.AddScoped<ITokenService, TokenService>();



// =========================
// Swagger
// =========================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();




var app = builder.Build();



app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

app.UseCors("AllowAngular");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapGet("/health", () => Results.Ok("OK"));

app.MapControllers();

app.Run();