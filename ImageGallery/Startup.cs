using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GalleryDatabase.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using GalleryDatabase.Services;
using Microsoft.AspNetCore.Identity;
using System;

namespace GalleryDatabase
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
            services.AddDbContext<GalleryDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("GalleryDbContext")));

            services.AddDefaultIdentity<GalleryOwner>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;    
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 3;

            }).AddEntityFrameworkStores<GalleryDbContext>()
            .AddDefaultTokenProviders()
            .AddPasswordValidator<Services.PasswordValidator<GalleryOwner>>();

            services.AddScoped<IEmailSender, EmailSender>();
            services.AddTransient<DropdownList>();
            services.AddTransient<SortingHelper>();
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    IConfigurationSection googleAuthNSection =
                    Configuration.GetSection("Authentication:Google");
                    options.ClientId = googleAuthNSection["ClientId"];
                    options.ClientSecret = googleAuthNSection["ClientSecret"];
                    options.AccessDeniedPath = "/Identity/Account/ExternalLoginFailed";
                })
                .AddMicrosoftAccount(options =>
                 {
                     options.ClientId = Configuration["Authentication:Microsoft:ClientId"];
                     options.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
                     options.AccessDeniedPath = "/Identity/Account/ExternalLoginFailed";
                     options.AuthorizationEndpoint = "https://login.microsoftonline.com/e7bf04a2-63c8-4a47-a0bd-28d0aa88f6ec/oauth2/v2.0/authorize";
                     options.TokenEndpoint = "https://login.microsoftonline.com/e7bf04a2-63c8-4a47-a0bd-28d0aa88f6ec/oauth2/v2.0/token";
                 });

            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });

            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            scope.ServiceProvider.GetService<GalleryDbContext>().Database.Migrate();
        }
    }
}
