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
                .AddFacebook(options =>
                {
                    IConfigurationSection FBAuthNSection =
                    Configuration.GetSection("Authentication:Facebook");
                    options.AppId = FBAuthNSection["AppId"];
                    options.AppSecret = FBAuthNSection["AppSecret"];
                    options.AccessDeniedPath = "/Identity/Account/ExternalLoginFailed";
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
