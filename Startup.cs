using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Diagnostics;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.AspNet.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Internal;
using System.Data.Entity;
using org.buraktamturk.web.Models;

namespace org.buraktamturk.web
{
    
    public class ScopedTest : IDisposable {
        
        public ScopedTest() {
            Console.WriteLine("Created!");
        }
        
        public void Dispose() {
            Console.WriteLine("Disposed!");
        }
    }
    
    public class Startup
    {
        public static IConfiguration config;
        
        public Startup(IHostingEnvironment env)
        {
            config = new Configuration(".")
                .AddJsonFile("config.json", true);
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors().AddMvc().ConfigureMvc(settings => {
    	       settings.ModelBinders.Add(new AuthorModelBinder());
            });
            
            services.AddScoped<DatabaseContext>();
            services.AddScoped<ScopedTest>();
        }

        public void ConfigureDevelopment(IApplicationBuilder app) {
           // app.UseBrowserLink();
            app.UseErrorPage(ErrorPageOptions.ShowAll);
            Configure(app);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCors(policy => policy.AllowAnyMethod().AllowAnyOrigin().WithHeaders("Token"))
               .UseStaticFiles()
               .UseMvc();
        }
    }
}