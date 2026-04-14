using System.Text;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.IdentityModel.Tokens;
using PNDS_BackEnd_Prod.OPC_Client;
using PNDS_BackEnd_Prod.OPC_Repos;
using PNDS_BackEnd_Prod.Services;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
// Konfiguracja Seriloga
Log.Logger = new LoggerConfiguration()
#if DEBUG
    .WriteTo.Console()
#endif
    .WriteTo.File("c:/PNDS/Logs/log-.log", 
                    rollingInterval: RollingInterval.Hour,
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    retainedFileCountLimit: 7,             // Przechowuj tylko 7 ostatnich plików (tydzień)
                    fileSizeLimitBytes: 10 * 1024 * 1024, // Opcjonalnie: limit 10MB na plik
                    rollOnFileSizeLimit: true)
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.FromSource<PNDS_BackEnd_Prod.Controllers.authController>())
        .WriteTo.File("c:/PNDS/logs/auth-.log", 
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    retainedFileCountLimit: 7,             // Przechowuj tylko 7 ostatnich plików (tydzień)
                    fileSizeLimitBytes: 10 * 1024 * 1024, // Opcjonalnie: limit 10MB na plik
                    rollOnFileSizeLimit: true))
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.FromSource<PNDS_BackEnd_Prod.Services.RecaptchaService>())
        .WriteTo.File("c:/PNDS/logs/reCap-.log",
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    retainedFileCountLimit: 7,             // Przechowuj tylko 7 ostatnich plików (tydzień)
                    fileSizeLimitBytes: 10 * 1024 * 1024, // Opcjonalnie: limit 10MB na plik
                    rollOnFileSizeLimit: true))
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.FromSource<PNDS_BackEnd_Prod.OPC_Client.OPCClient>())
        .WriteTo.File("c:/PNDS/logs/OPC_Client-.log",
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    retainedFileCountLimit: 7,             // Przechowuj tylko 7 ostatnich plików (tydzień)
                    fileSizeLimitBytes: 10 * 1024 * 1024, // Opcjonalnie: limit 10MB na plik
                    rollOnFileSizeLimit: true))
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Add services to the container.

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

builder.Services.AddSingleton<J1DalbaListInterface, J1DalbaList>();
builder.Services.AddSingleton<J2DalbaListInterface, J2DalbaList>();
builder.Services.AddSingleton<J1ShipDataInterface, J1ShipDataRepo>();
builder.Services.AddSingleton<J2ShipDataInterface, J2ShipDataRepo>();
builder.Services.AddSingleton<J1WeatherDataInterface, J1WeatherDataRepo>();
builder.Services.AddSingleton<J2WeatherDataInterface, J2WeatherDataRepo>();
builder.Services.AddSingleton<J1SeaStateDataInterface, J1SeaStateDataRepo>();
builder.Services.AddSingleton<J2SeaStateDataInterface, J2SeaStateDataRepo>();
builder.Services.AddSingleton<J1BerthingInterface, J1BerthingRepo>();
builder.Services.AddSingleton<J2BerthingInterface, J2BerthingRepo>();
//builder.Services.AddSingleton<ShipDataInterface, ShipDataRepo>();
builder.Services.AddControllers().AddNewtonsoftJson();

//builder.Services.AddSingleton<IOPCClient,OPCClient>();


builder.Services.AddControllers();

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

// Configure the HTTP request pipeline.
app.UseCors("react");
//app.UseCors("AllowAll");
//app.UseCors(cors => cors.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
// Configure the HTTP request pipeline.


app.UseHttpsRedirection();
//app.UseAuthorization();

app.MapControllers();

app.Run();
