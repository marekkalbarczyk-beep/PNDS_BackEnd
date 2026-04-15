using System.Text;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.IdentityModel.Tokens;
using PNDS_BackEnd_Dev.Models;
using PNDS_BackEnd_Dev.OPC_Client;
using PNDS_BackEnd_Dev.Services;
using Serilog;
using Serilog.Events;
using Serilog.Filters;

// Konfiguracja Seriloga
Log.Logger = new LoggerConfiguration()
#if DEBUG
    .WriteTo.Console()
#endif
    .WriteTo.File("c:/PNDS/DevLogs/log-.log",
                    rollingInterval: RollingInterval.Hour,
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    retainedFileCountLimit: 7,             // Przechowuj tylko 7 ostatnich plików (tydzień)
                    fileSizeLimitBytes: 10 * 1024 * 1024, // Opcjonalnie: limit 10MB na plik
                    rollOnFileSizeLimit: true)
    //.WriteTo.Logger(lc => lc
    //    .Filter.ByIncludingOnly(Matching.FromSource<PNDS_BackEnd_Prod.Controllers.authController>())
    //    .WriteTo.File("c:/PNDS/logs/auth-.log",
    //                rollingInterval: RollingInterval.Day,
    //                restrictedToMinimumLevel: LogEventLevel.Information,
    //                retainedFileCountLimit: 7,             // Przechowuj tylko 7 ostatnich plików (tydzień)
    //                fileSizeLimitBytes: 10 * 1024 * 1024, // Opcjonalnie: limit 10MB na plik
    //                rollOnFileSizeLimit: true))
    //.WriteTo.Logger(lc => lc
    //    .Filter.ByIncludingOnly(Matching.FromSource<PNDS_BackEnd_Prod.Services.RecaptchaService>())
    //    .WriteTo.File("c:/PNDS/logs/reCap-.log",
    //                rollingInterval: RollingInterval.Day,
    //                restrictedToMinimumLevel: LogEventLevel.Information,
    //                retainedFileCountLimit: 7,             // Przechowuj tylko 7 ostatnich plików (tydzień)
    //                fileSizeLimitBytes: 10 * 1024 * 1024, // Opcjonalnie: limit 10MB na plik
    //                rollOnFileSizeLimit: true))
    //.WriteTo.Logger(lc => lc
    //    .Filter.ByIncludingOnly(Matching.FromSource<PNDS_BackEnd_Prod.OPC_Client.OPCClient>())
    //    .WriteTo.File("c:/PNDS/logs/OPC_Client-.log",
    //                rollingInterval: RollingInterval.Day,
    //                restrictedToMinimumLevel: LogEventLevel.Information,
    //                retainedFileCountLimit: 7,             // Przechowuj tylko 7 ostatnich plików (tydzień)
    //                fileSizeLimitBytes: 10 * 1024 * 1024, // Opcjonalnie: limit 10MB na plik
    //                rollOnFileSizeLimit: true))
    .CreateLogger();



var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();


var jwtKey = builder.Configuration["BearerJWT:Key"] ??= "Default_T43_M0st_Complicated_Protected_K3Y_1n_Th3_Univers3";

var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
})
.AddJwtBearer("JwtBearer", options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
#if DEBUG
    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
#endif


builder.Services.AddSingleton<IOPCClient, OPCClient>();
builder.Services.AddSingleton<IJ1ShipService, J1ShipService>();
builder.Services.AddSingleton<IJ2ShipService, J2ShipService>();
builder.Services.AddSingleton<IJ1BerthingService, J1BerthingService>();
builder.Services.AddSingleton<IJ2BerthingService, J2BerthingService>();
builder.Services.AddSingleton<IJ1SeaStateService, J1SeaStateService>();
builder.Services.AddSingleton<IJ2SeaStateService, J2SeaStateService>();

builder.Services.AddHttpClient<RecaptchaService>();
builder.Services.AddScoped<ShipService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("react",
        p => p.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin());
});

var app = builder.Build();

app.UseCors("react");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseSwaggerUI(options => // UseSwaggerUI is called only in Development.
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
