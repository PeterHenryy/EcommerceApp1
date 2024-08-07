using Azure.Storage.Blobs;
using EcommerceApp1.Data;
using EcommerceApp1.Models;
using EcommerceApp1.Models.Identity;
using EcommerceApp1.Models.Repositories;
using EcommerceApp1.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceApp1
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddIdentity<AppUser, AppRole>(options => options.SignIn.RequireConfirmedAccount = false)
                                                    .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddTransient<CompanyRepository>();
            services.AddTransient<CompanyService>();
            services.AddTransient<ProductRepository>();
            services.AddTransient<ProductService>();
            services.AddTransient<UserService>();
            services.AddTransient<TransactionRepository>();
            services.AddTransient<TransactionService>();
            services.AddTransient<CreditCardRepository>();
            services.AddTransient<CreditCardService>();
            services.AddTransient<ReviewRepository>();
            services.AddTransient<ReviewService>();
            services.AddTransient<CommentRepository>();
            services.AddTransient<CommentService>();
            services.AddTransient<RatingRepository>();
            services.AddTransient<RatingService>();
            services.AddTransient<RefundRepository>();
            services.AddTransient<RefundService>();
            services.AddTransient<CouponRepository>();
            services.AddTransient<CouponService>();
            services.AddTransient<ShoppingCartRepository>();
            services.AddTransient<ShoppingCartService> ();
            services.AddRazorPages().AddRazorRuntimeCompilation();                 
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.ConfigureApplicationCookie(x => x.LoginPath = "/AppUser/Login");
            services.AddSingleton(u => new BlobServiceClient(
        Configuration.GetValue<string>("BlobConnection")
            ));
            services.AddSingleton<IBlobService, BlobService>();
            services.AddRazorPages()
            .AddMvcOptions(options =>
            {
                options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
                    _ => "");
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Product}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
