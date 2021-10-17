using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weasel.Postgresql;

namespace MartenProjectionListenerRepro
{
    public class Startup
    {
        readonly IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = _config.GetConnectionString("Data");

            var lf = LoggerFactory.Create(lf => lf.AddConsole());
            
            services.AddMarten(opts => {
                opts.AutoCreateSchemaObjects = AutoCreate.All;

                opts.Connection(() => connectionString);

                opts.Listeners.Add(new TestListener(lf.CreateLogger<TestListener>()));
                opts.Projections.AsyncMode = DaemonMode.HotCold;
                opts.Projections.Add<TestProjection>(ProjectionLifecycle.Async);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");

                    var store = context.RequestServices.GetService<IDocumentStore>();
                    using var session = store.OpenSession();
                    session.Events.StartStream<TestEntity>(Guid.NewGuid(), new [] {new TestEvent()});
                    await session.SaveChangesAsync();
                });
            });
        }
    }
}
