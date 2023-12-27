
using Microsoft.EntityFrameworkCore;
using PerformanceAuditing.Contracts;
using PerformanceAuditing.Data;
using PerformanceAuditing.Services;
using PerformanceAuditing.Workers;

namespace PerformanceAuditing
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //configuring worker settings
            builder.Services.Configure<WorkerSettings>(builder.Configuration.GetSection("WorkerSettings"));
            builder.Services.AddSingleton<WorkerSettings>();

            //register the background worker
            builder.Services.AddHostedService<AuditWorker>();
            //registration of URL Management Service
            builder.Services.AddSingleton<URLManagementService>();

            builder.Services.AddDbContext<ApplicationDBContext>(
                options => {
                    options.UseSqlServer(
                    builder.Configuration["ConnectionStrings:ApplicationDB"]
                    );
                }
                );
            builder.Services.AddScoped<IAuditService,AuditResultsService>();
            //adding the service
            //int workerCount = builder.Configuration.GetValue<int>("NumberOfServiceWorkers");
            //int workerCount = builder.Configuration.GetValue<WorkerSettings>("WorkerSettings").InitialNumberOfWorkers;

            //for(int  i = 0; i < workerCount; i++)
            //{
            //    builder.Services.AddHostedService(
            //         provider => {
            //             return new AuditingWorker(provider.GetRequiredService<ILogger<AuditingWorker>>(),provider.GetRequiredService<WorkerSettings>());
            //         }

                    
            //        );

            //}
         

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}