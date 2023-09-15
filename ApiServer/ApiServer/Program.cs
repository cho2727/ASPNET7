using ApiServer.Helper;
using ApiServer.Shared.Behaviors;
using ApiServer.Shared.Injectables;
using ApiServer.Shared.Models;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using Infrastructure.Extensions;
using ApiServer.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;

var configurationBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.Development.json", optional: true)
    .Build();

Environment.SetEnvironmentVariable("BASEDIR", AppDomain.CurrentDomain.BaseDirectory);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configurationBuilder)
    .CreateBootstrapLogger();

try
{

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    // builder.Services.AddSwaggerGen();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiServer API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Name = "Bearer",
            BearerFormat = "JWT",
            Description = "Please enter authorization key",
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme()
                    {
                         Reference = new OpenApiReference()
                         {
                             Type = ReferenceType.SecurityScheme,
                             Id = "Bearer"
                         }
                    },
                    Enumerable.Empty<string>().ToList()
                }
            });
        c.CustomSchemaIds(x => x.FullName?.Replace("+", "."));
    });
    builder.Services.Configure<SwaggerGeneratorOptions>(opts =>
    {
        opts.InferSecuritySchemes = true;
    });

    builder.Host.UseSerilog((context, configuration) => configuration
                                                        .ReadFrom
                                                        .Configuration(context.Configuration));

    builder.Services.AddMediatR(AssemblyHelper.GetAllAssemblies().ToArray());
    //builder.Services.AddSingleton<ITokenGenerator, TokenGenerator>();
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MediatRAutoLogger<,>)); // 자동로거

    builder.Services.Scan(scan => scan
    .FromAssemblies(AssemblyHelper.GetAllAssemblies(SearchOption.AllDirectories))
    .AddClasses(classes => classes.AssignableTo<ITransientService>())
    .AsSelfWithInterfaces()
    .WithTransientLifetime()
    .AddClasses(classes => classes.AssignableTo<IScopedService>())
    .AsSelfWithInterfaces()
    .WithScopedLifetime()
    .AddClasses(classes => classes.AssignableTo<ISingletonService>())
    .AsSelfWithInterfaces()
    .WithSingletonLifetime());

    builder.Services.Configure<TokenManagement>(builder.Configuration.GetSection("TokenManagement"));
    var token = builder.Configuration.GetSection("TokenManagement").Get<TokenManagement>();
    if (token == null)
        throw new ArgumentException(nameof(token));
    var secret = Encoding.ASCII.GetBytes(token.Secret);

    builder.Services.AddAuthentication().AddJwtBearer(x =>
    {
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(token.Secret)),
            ValidIssuer = token.Issuer,
            ValidAudience = token.Audience,
            ValidateIssuer = false,
            ValidateAudience = false
            //ClockSkew = isDevelopment ? TimeSpan.Zero : TimeSpan.FromSeconds(30)
        };
    });
    // builder.Services.AddAuthorization();
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("CustomAuthorizationPolicy", policy => {
            policy.Requirements.Add(new CustomAuthorizationPolicyRequirement());
        });
    });

    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    builder.Services.AddScoped<IAuthorizationHandler, CustomAuthorizationPolicyHandler>();

    builder.Services.AddCors(o => o.AddPolicy("ApiServerPolicy", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    }));

    string connectionString = builder.Configuration.GetConnectionString("Server") ?? string.Empty;
    builder.Services.AddServerAccessServices(connectionString);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();
    //app.UseMiddleware<AutoLogger>();

    app.MapControllers();

    app.Run();

}
catch (Exception ex)
{
    string type = ex.GetType().Name;
    if (type.Equals("StopTheHostException", StringComparison.Ordinal))
        throw;
    if (type.Equals("HostAbortedException", StringComparison.Ordinal))
        throw;
    Log.Fatal(ex, $"{type} Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
