using WebCar.Models;
using WebCar.Repository;
using WebCar.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebCar.DbContext;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Minio;
using Microsoft.Extensions.Options;
using System.Net;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DB
builder.Services.AddDbContext<myDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("local");
    options.UseNpgsql(connectionString);
});

// Add Identity
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<myDbContext>()
    .AddDefaultTokenProviders();


// Config Identity
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 3;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.SignIn.RequireConfirmedEmail = false;
});


// Add Authentication and JwtBearer
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
        };
    });




// Inject app Dependencies (Dependency Injection)
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter your token with this format: ''Bearer YOUR_TOKEN''",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Name = "Bearer",
                In = ParameterLocation.Header,
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});


builder.Services.AddScoped<ICarCompanyService, carCompanyService>();
builder.Services.AddScoped<ICarService, carService>();
builder.Services.AddSingleton<IRedisCache, RedisCacheService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Add Redis Cache
builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = builder.Configuration["RedisCacheUrl"]; });

builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddScoped<KafkaProducerService>();
builder.Services.AddSingleton<KafkaConsumerService>();




configureLogging();
builder.Host.UseSerilog();

builder.Services.Configure<MinIOSettings>(builder.Configuration.GetSection("MinIO"));

// Register MinioClient
builder.Services.AddSingleton(sp =>
{
    var minioSettings = sp.GetRequiredService<IOptions<MinIOSettings>>().Value;
    return new MinioClient()
        .WithEndpoint(minioSettings.Host)
        .WithCredentials(minioSettings.AccessKey, minioSettings.SecretKey)
        .WithSSL(minioSettings.SSL)
        .Build();
});

builder.Services.AddScoped<MinIOService>();


// Register your MinIOService
//builder.Services.AddTransient<MinIOService>();

var app = builder.Build();
app.UseCors(x => x
           .AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var kafkaConsumerService = app.Services.GetRequiredService<KafkaConsumerService>();
kafkaConsumerService.StartAsync(CancellationToken.None);
app.Run();

void configureLogging()
{
    //var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    //var configuration = new ConfigurationBuilder()
    //    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    //    .AddJsonFile(
    //        $"appsettings.{environment}.json", optional: false
    //       ).Build();
    //Log.Logger = new LoggerConfiguration()
    //    .Enrich.FromLogContext()
    //    .Enrich.WithExceptionDetails()
    //    .WriteTo.Debug()
    //    .WriteTo.Console()
    //    .WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment))
    //    .Enrich.WithProperty("Environment", environment)
    //    .ReadFrom.Configuration(configuration)
    //    .WriteTo.File("logs/webCar-.txt", rollingInterval: RollingInterval.Day)
    //    .CreateLogger();
    //var config = new ConfigurationBuilder()
    //        .AddJsonFile("appsettings.json")
    //        .Build();
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile(
            $"appsettings.{environment}.json", optional: false
           ).Build();
    Log.Logger = new LoggerConfiguration()
        //.ReadFrom.Configuration(config)
        .ReadFrom.Configuration(configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment))
        //.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(config["ElasticsearchUrl"]))
        //{
        //    AutoRegisterTemplate = true,
        //    IndexFormat = "your-app-logs-{0:yyyy.MM.dd}"
        //})
        .WriteTo.File("logs/webCar-.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();
}
ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment)
{
    return new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Url"]))
    {
        AutoRegisterTemplate = true,
        IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{environment.ToLower()}-{DateTime.UtcNow:yyyy-MM}",
        NumberOfReplicas = 1,
        NumberOfShards = 2,

    };
}