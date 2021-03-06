﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Quickstart.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using IdSrvr4Demo.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using IdSrvr4Demo.Context;
using IdSrvr4Demo.Services;
using IdentityServer4.Validation;
using IdentityServer4.Services;
using System.Security.Cryptography.X509Certificates;
using IdSrvr4Demo.Settings;
using IdentityServer4;
using IdSrvr4Demo.Repositories;
using IdentityServer.Web.ConfigStores;

namespace IdSrvr4Demo
{
  public class Startup
  {
    public IConfiguration _configuration { get; }

    public Startup(IConfiguration Configuration)
    {
      _configuration = Configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {

      services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
      {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
      }));
       
      services.AddIdentityServer()
             .AddDeveloperSigningCredential()
              //.AddSigningCredential(GetClientCertificate("idsrv3test"))              
              .AddInMemoryPersistedGrants()
              .AddResourceStore<ResourcesStore>()
              .AddClientStore<ClientStore>()
              .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
              .AddProfileService<ProfileService>();

      // Inject services
      services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();
      services.AddTransient<IProfileService, ProfileService>();

      services.AddScoped(typeof(IUserRepository), typeof(UserRepository));

      services.AddSingleton(_configuration.GetSection("GoogleCredentials").Get<GoogleSettings>());
      services.AddSingleton(_configuration.GetSection("FacebookCredentials").Get<FacebookCredentials>());

      services.AddAuthentication()
     .AddGoogle("Google", options =>
     {
       options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

       options.ClientId = GoogleSettings.ClientId;
       options.ClientSecret =GoogleSettings.CLientSecret;
     });

      // this only works on local
      services.AddAuthentication()
      .AddFacebook(facebookOptions =>
      {
        facebookOptions.AppId = "398265667678447";
        facebookOptions.AppSecret = "7cff924ebd0ac7e718fded423e11a472";
        //  facebookOptions.SignInScheme = IdentityServerConstants.DefaultCookieAuthenticationScheme;
        // facebookOptions.CallbackPath = "/signin-facebook";
        facebookOptions.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

      });
      services.AddMvc(options =>options.EnableEndpointRouting = false );

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      app.UseCors("MyPolicy");

      var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

      app.UseDeveloperExceptionPage();
     
      app.UseStaticFiles();
      app.UseIdentityServer();
      app.UseAuthentication();
      app.UseMvcWithDefaultRoute();
    }

    private static X509Certificate2 GetClientCertificate(object findValue)
    {
      var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
      X509Certificate2 certificate = null;

      try
      {
        store.Open(OpenFlags.ReadOnly);

        X509Certificate2Collection collection =
          store.Certificates.Find(X509FindType.FindBySubjectName, findValue, false);

        if (collection.Count != 0)
        {
          certificate = collection[0];
        }
      }
      finally
      {
        store.Close();
      }

      return certificate;
    }

  }
}
