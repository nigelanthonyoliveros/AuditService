
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

            //registration of URL Management Service & Results Service
            builder.Services.AddSingleton<URLManagementService>();
            builder.Services.AddScoped<IAuditService, AuditResultsService>();


            //! DB Context 
            builder.Services.AddDbContext<ApplicationDBContext>(
                options => {
                    options.UseSqlServer(
                    builder.Configuration["ConnectionStrings:ApplicationDB"]
                    );
                    
                }
                );



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