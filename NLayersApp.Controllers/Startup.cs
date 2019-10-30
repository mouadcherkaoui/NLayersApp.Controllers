using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLayersApp.Controllers.DependencyInjection;
using NLayersApp.Persistence;
using NLayersApp.CQRS.DependencyInjection;
using NLayersApp.Persistence.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using IdentityServer4.Services;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Entities;
using System.Threading.Tasks;
using System.Threading;
using NLayersApp.Extensions;
using NLayersApp.Authorisation.Models;
using NLayersApp.Authorisation;
using NLayersApp.Authorization.Extensions;

namespace NLayersApp.Controllers
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var resolver = new TypesResolver(() => new Type[] { typeof(TestModel) });

            services.AddScoped<ITypesResolver>(s => resolver);
            // services.AddScoped<TDbContext>();

            services.AddDbContext<IContext, TDbContextConfigStore>(optionsAction: (s, o) =>
            {
                o.UseInMemoryDatabase("nlayersapp-tests");
            }, ServiceLifetime.Scoped);

            services.AddAuthorizationConfig<TDbContextConfigStore, AppUser, IdentityRole, string>();
            // ConfigureAuthenticationAndAuthorisation(services);

            services.AddMediatRHandlers(resolver);

            services.AddControllers()
                .UseDynamicControllers(resolver)
                .AddControllersAsServices()
                .UseNLayersAppAccountController();
        }

        private void ConfigureAuthenticationAndAuthorisation(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "app";
                options.DefaultScheme = "app";
            })
            .AddCookie();


            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<TDbContextConfigStore>()
                .AddUserManager<UserManager<AppUser>>()
                .AddRoleManager<RoleManager<IdentityRole>>()
                .AddDefaultTokenProviders();
            //.AddClaimsPrincipalFactory();

            var builder = services.AddIdentityServer()
                .AddConfigurationStore<TDbContextConfigStore>()
                .AddAspNetIdentity<AppUser>()
                .AddInMemoryApiResources(AuthConfig.Apis)
                .AddInMemoryClients(AuthConfig.Clients)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseInMemoryDatabase("nalyersapp-tests");
                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 30; // interval in seconds
                })
                //.AddInMemoryPersistedGrants()
                .AddInMemoryIdentityResources(AuthConfig.GetIdentityResources())
                .AddInMemoryApiResources(AuthConfig.GetApiResources())
                .AddInMemoryClients(AuthConfig.GetClients());

            /* We'll play with this down the road... 
                services.AddAuthentication()
                .AddGoogle("Google", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = "<insert here>";
                    options.ClientSecret = "<insert here>";
                });*/

            services.AddTransient<IProfileService, IdentityClaimsProfileService>();

            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader())); ;

            builder.AddDeveloperSigningCredential();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                    var error = context.Features.Get<IExceptionHandlerFeature>();
                    if (error != null)
                    {
                        context.Response.AddApplicationError(error.Error.Message);
                        await context.Response.WriteAsync(error.Error.Message).ConfigureAwait(false);
                    }
                });
            });

            app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseIdentityServer().UseAuthentication();
            //app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
    public class TDbContextConfigStore : TDbContext<AppUser>, IConfigurationDbContext
    {
        public TDbContextConfigStore(DbContextOptions<TDbContextConfigStore> options, ITypesResolver typesResolver) : base(options, typesResolver)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<IdentityResource> IdentityResources { get; set; }
        public DbSet<ApiResource> ApiResources { get; set; }

        public Task<int> SaveChangesAsync()
        {
            return SaveChangesAsync(CancellationToken.None);
        }
    }
}
//services.AddAuthentication("Bearer")
//    .AddJwtBearer("Bearer", options =>
//    {
//        options.Authority = "http://localhost:5000";
//        options.RequireHttpsMetadata = false;

//        options.Audience = "api1";
//    });

//services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = "GitHub";
//})
//.AddCookie()
//.AddOAuth("GitHub", options =>
//{
//    options.ClientId = Configuration["GitHub:ClientId"];
//    options.ClientSecret = Configuration["GitHub:ClientSecret"];
//    options.CallbackPath = new PathString("/signin-github");

//    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
//    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
//    options.UserInformationEndpoint = "https://api.github.com/user";

//    options.SaveTokens = true;

//    // some code omitted for brevity

//    options.Events = new OAuthEvents
//    {
//        OnCreatingTicket = async context =>
//        {
//            // some code omitted for brevity
//        }
//    };
//});


//services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = "app";
//    options.DefaultScheme = "app";
//})
//    .AddCookie()
//    .AddOAuth("app", options =>
//    {
//        options.ClientId = "appclient";
//        options.ClientSecret = "3f97501d279b44f3bd69e8eec64cf336";

//        options.CallbackPath = new PathString("/app-signin");

//        options.Scope.Add("app_identity");
//        options.Scope.Add("profile");
//        options.Scope.Add("email");

//        options.AuthorizationEndpoint = "http://dev/services/identity/connect/authorize";
//        options.TokenEndpoint = "http://dev/services/identity/connect/token";
//        options.UserInformationEndpoint = "http://dev/services/identity/api/user-profile";

//        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
//        options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "firstName");
//        options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "lastName");
//        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

//        options.Events = new OAuthEvents
//        {
//            OnCreatingTicket = async context =>
//            {
//                var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
//                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

//                var response = await context.Backchannel.SendAsync(request,
//                                   HttpCompletionOption.ResponseHeadersRead,
//                                   context.HttpContext.RequestAborted);
//                response.EnsureSuccessStatusCode();
//                //JsonElement user = JObject.Parse(await response.Content.ReadAsStringAsync(), new JsonLoadSettings() { });
//                //context.RunClaimActions(user);
//            }
//        };
//    });