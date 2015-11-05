using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Diagnostics;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Configuration;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Internal;
using System.Data.Entity;
using org.buraktamturk.web.Models;
using MarkdownDeep;
using Microsoft.Dnx.Runtime;

namespace org.buraktamturk.web
{
    public class Startup
    {
        public static Markdown md { get; set; }

        public static IConfiguration config;

        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            config = new ConfigurationBuilder(appEnv.ApplicationBasePath)
                .AddJsonFile("./config.json").Build();

            md = new Markdown()
            {
                ExtraMode = true,
                
            };
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors().AddMvc(settings => {
              settings.ModelBinders.Add(new AuthorModelBinder());
            });

            var connStr = Startup.config.GetSection("db").Value;

            services.AddScoped<DatabaseContext>(a => new DatabaseContext(connStr));
        }

        public void Configure(IApplicationBuilder app)
        {
            app
                .UseErrorPage()
                .Use(async (a, b) => {
                  if(a.Request.Host.Value == "www.buraktamturk.org") {
                    a.Response.StatusCode = 301;
              	    a.Response.Headers.Set("Location", a.Request.Scheme + "://buraktamturk.org" + a.Request.Path);
                  } else {
                    await b();
                  }
                })
                .UseCors(policy => policy.AllowAnyMethod().AllowAnyOrigin().WithHeaders("Token"))
               .UseStaticFiles()
               .UseMvc();
        }
    }
}
