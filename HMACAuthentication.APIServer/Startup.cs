using HMACAuthentication.APIServer.Configurations;
using HMACAuthentication.APIServer.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;

namespace HMACAuthentication.APIServer
{
    /// <summary>
    /// The Startup
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public IConfiguration Configuration { get; }
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddMemoryCache();

            ConfigureContainer(services);
            ConfigureAppSettings(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHMACMiddleware();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
        /// <summary>
        /// Configures the container.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureContainer(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Configuration);
        }
        /// <summary>
        /// Configures the application settings.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureAppSettings(IServiceCollection services)
        {
            services.Configure<HMACConfigurations>(Configuration.GetSection("HMACConfigurations"));
            services.Configure<Logging>(Configuration.GetSection("Logging"));
        }
    }
}
