using ASP.NET_Core_Webapp.Configurations;
using ASP.NET_Core_Webapp.Data;
using ASP.NET_Core_Webapp.Helpers;
using ASP.NET_Core_Webapp.SeedData;
using ASP.NET_Core_Webapp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using ASP.NET_Core_Webapp.Mocks;

namespace ASP.NET_Core_Webapp
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IHostingEnvironment env;

        public Startup(IHostingEnvironment environment, IConfiguration config)
        {
            this.env = environment;
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true).AddUserSecrets<Startup>()
                .AddEnvironmentVariables();
            this.configuration = builder.Build();

            this.env = environment;
            this.configuration = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var allvariables = Environment.GetEnvironmentVariables();

            services.AddCors();
            services.AddMvc().AddJsonOptions(options =>
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

            if (env.IsDevelopment())

            {
                services.AddDbContext<ApplicationContext>(builder =>
                        builder.UseInMemoryDatabase("InMemoryDatabase"));
            }
            if (env.IsProduction())
            {
                services.AddDbContext<ApplicationContext>(builder =>
                        builder.UseSqlServer(configuration.GetConnectionString("environmentString"))
                        .EnableSensitiveDataLogging(true));
            }

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser().Build());
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateIssuerSigningKey = true,
                            ValidateLifetime = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Authentication:Jwt:Secret"])),
                            ClockSkew = TimeSpan.Zero
                        };
                        options.Events = new JwtBearerEvents()
                        {
                            OnAuthenticationFailed = c =>
                            {
                                c.NoResult();
                                c.Response.StatusCode = 401;
                                c.Response.ContentType = "application/json";
                                c.Response.WriteAsync(JsonConvert.SerializeObject(new CustomErrorMessage("Unauthorized"))).Wait();
                                return Task.CompletedTask;
                            },
                            OnChallenge = c =>
                            {
                                c.HandleResponse();
                                return Task.CompletedTask;
                            }
                        };
                    });

            services.AddScoped<IHelloService, HelloService>();

            services.AddScoped<ISlackService, SlackService>();
            services.AddHttpClient();
            services.AddScoped<HttpClient>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IGoogleSheetService, GoogleSheetService>();
            services.AddScoped<IPitchService, PitchService>();            
            services.AddHttpClient<GoogleSheetService>();
            services.AddHttpClient<AuthService>();
        }

        public void ConfigureTestingServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationContext>(builder =>
                builder.UseInMemoryDatabase("InMemoryDatabase"));

            services.AddCors();
            services.AddMvc().AddJsonOptions(options =>
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser().Build());
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
            }).AddTestAuth(o => { });

            services.AddScoped<IHelloService, HelloService>();
            services.AddScoped<IAuthService, MockAuthService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IGoogleSheetService, MockGoogleSpreadSheetService>();
            services.AddScoped<ISlackService, MockSlackService>();
            services.AddScoped<IPitchService, PitchService>();
            services.AddScoped<HttpClient>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ApplicationContext applicationContext)
        {
            if (env.IsDevelopment() || env.EnvironmentName == "Testing")
            {
                app.UseDeveloperExceptionPage();
                SeedDatabaseHandler seedDataFromObject = new SeedDatabaseHandler(applicationContext, configuration);
                seedDataFromObject.FillDatabaseFromObject();
            }

            if (env.IsProduction())
            {
                SeedDatabaseHandler seedDataFromObject = new SeedDatabaseHandler(applicationContext, configuration);

                seedDataFromObject.FillDatabaseFromObject();
            }
            app.UseMiddleware<ErrorHandlerMiddleware>();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Auth}/{action=Login}");
            });


            app.UseAuthentication();
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
        }
    }
}