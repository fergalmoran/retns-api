using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using retns.api.Data;
using retns.api.Data.Models;
using retns.api.Data.Settings;
using retns.api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace retns.homework.api {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.Configure<StorageSettings>(Configuration.GetSection("StorageSettings"));

            services.AddScoped<HomeworkFileParser>();
            services.AddScoped<AzureHelper>();
            services.AddScoped<HomeworkService>();
            var storageSettings = Configuration.GetSection(nameof(StorageSettings)).Get<StorageSettings>();
            services.AddHttpClient("AzureClient", client => {
                client.BaseAddress = new Uri(storageSettings.CdnUrl);
            });
            services.AddCors(options => {
                options.AddPolicy(
                    "AllowAllOrigins",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyHeader());

            });

            services.AddMvc(options => {
                options.OutputFormatters.OfType<StringOutputFormatter>().Single().SupportedMediaTypes.Add("text/html");
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseCors(builder => {
                builder.AllowAnyOrigin().AllowAnyHeader();
            });

            app.UseMvc();
        }
    }
}
