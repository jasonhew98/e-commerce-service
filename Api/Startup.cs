using Api.Infrastructure;
using Api.Infrastructure.AutofacModules;
using Api.Infrastructure.Services;
using Api.Seedwork.AesEncryption;
using Autofac;
using FluentValidation.AspNetCore;
using Infrastructure.Seedwork;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;

namespace Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddOptions().Configure<ServiceConfiguration>(Configuration);
            services.AddOptions().Configure<CoreRepositoryOptions>(Configuration.GetSection("CoreRepository"));
            services.AddOptions().Configure<AesConfigurationOptions>(Configuration.GetSection("AesConfigurations"));
            services.AddOptions().Configure<JwtAuthorizationConfigurationOptions>(Configuration.GetSection("JwtAuthorizationConfigurations"));
            services.AddOptions().Configure<DirectoryPathConfigurationOptions>(Configuration.GetSection("DirectoryPathConfigurations"));
            services.AddOptions().Configure<BaseAddressConfigurationOptions>(Configuration.GetSection("BaseAddressConfigurations"));
            services.AddOptions().Configure<GoogleAuthServiceConfigurationOptions>(Configuration.GetSection("GoogleAuthServiceConfigurations"));
            services.AddOptions().Configure<MicrosoftAuthServiceConfigurationOptions>(Configuration.GetSection("MicrosoftAuthServiceConfigurations"));
            services.AddOptions().Configure<MicrosoftGraphServiceConfigurationOptions>(Configuration.GetSection("MicrosoftGraphServiceConfigurations"));

            services.AddCors();
            services.AddHttpContextAccessor();

            services.AddMvc(opt => opt.EnableEndpointRouting = false)
                .AddNewtonsoftJson()
                .AddFluentValidation();

            services.AddControllers().AddNewtonsoftJson();
            services.AddControllersWithViews().AddNewtonsoftJson();
            services.AddRazorPages().AddNewtonsoftJson();

            services.AddApiVersioning(opt =>
            {
                opt.ReportApiVersions = true;
                opt.DefaultApiVersion = ApiVersion.Parse("1.0");
                opt.AssumeDefaultVersionWhenUnspecified = true;
            });

            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
                .AddCookie()
                .AddJwtBearer();

            services.AddHttpClient<GoogleAuthService, ApiGoogleAuthService>((serviceProvider, client) =>
            {
                var url = serviceProvider.GetService<IOptions<GoogleAuthServiceConfigurationOptions>>().Value.ServiceUrl;
                client.BaseAddress = new Uri(url);
            });

            services.AddHttpClient<MicrosoftAuthService, ApiMicrosoftAuthService>((serviceProvider, client) =>
            {
                var url = serviceProvider.GetService<IOptions<MicrosoftAuthServiceConfigurationOptions>>().Value.ServiceUrl;
                client.BaseAddress = new Uri(url);
            });

            services.AddHttpClient<OneDriveService, ApiOneDriveService>((serviceProvider, client) =>
            {
                var url = serviceProvider.GetService<IOptions<MicrosoftGraphServiceConfigurationOptions>>().Value.ServiceUrl;
                client.BaseAddress = new Uri(url);
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            var container = builder;

            container.RegisterModule(new MediatorModule());
            container.RegisterModule(new ApplicationModule(Configuration.Get<ServiceConfiguration>()));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors(b =>
                {
                    b.AllowAnyHeader();
                    b.AllowAnyMethod();
                    b.AllowAnyOrigin();
                });
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc();

            MongoDbConfiguration.RegisterDefault();
        }
    }
}
