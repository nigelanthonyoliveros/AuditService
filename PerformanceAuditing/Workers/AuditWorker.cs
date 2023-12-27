using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Extensions;
using PerformanceAuditing.Contracts;
using PerformanceAuditing.Data;
using PerformanceAuditing.Services;
using System;

namespace PerformanceAuditing.Workers
{
    public class AuditWorker : BackgroundService
    {
        private readonly ILogger<AuditWorker> _logger;
        private readonly IOptions<WorkerSettings> settings;
        private readonly URLManagementService urlservice;
        private readonly IServiceProvider provider;
       

        public AuditWorker(ILogger<AuditWorker> logger, IOptions<WorkerSettings> settings, URLManagementService urlservice , IServiceProvider provider)
        {
            this._logger=logger;
            this.settings=settings;
            this.urlservice=urlservice;
            this.provider=provider;
           
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {   
            
            #region Log
            _logger.LogWarning("Service Started....");
            _logger.LogInformation($"-- Config -- \n" +
                $"\n Initial Number of Workers: {this.settings.Value.InitialNumberOfWorkers}" +
                $"\n Cycle Time : {this.settings.Value.CycleTime}" +
                $"\n JSon Path: {this.settings.Value.JSONFilePath}");
            #endregion
            urlservice.SeedFromJson();

            //urlservice.AddURL("http://resreq.in");

            // using an interval timer, that accepts timespan as a parameter, 
            //getting that paramater value from the configuration that we have in appsettings.json
            PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMilliseconds(settings.Value.CycleTime));

            while(await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
            {
                string? url = urlservice.NextUrl();

                if(!string.IsNullOrEmpty(url))
                {
                    await AuditURL(url);
                }
               
                _logger.LogInformation($"Running service at {DateTime.Now.ToString("h:m:s tt")}");
            }
            
        }

        protected async Task AuditURL (string url)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri (url);
            DateTime startTime = DateTime.Now;
            try
            {
                HttpResponseMessage responseMessage = await client.GetAsync(url);
                if (responseMessage.IsSuccessStatusCode)
                {
                    TimeSpan responseTime = DateTime.Now - startTime;
                    _logger.LogInformation("Succefully reached the site {site} with \n Status Code : {code} ({stat})  \n Response Time: {time}", 
                        url.Length > 25 ? string.Concat(url.Substring(0,25),"...") : url , 
                        responseMessage.StatusCode.GetDisplayName(),
                        (int)responseMessage.StatusCode, 
                        responseTime );
                    await PersistRecordToDB(new AuditResults()
                                                {
                                                    AccessTime = startTime,
                                                    Reachable = (bool)responseMessage.IsSuccessStatusCode,
                                                    ResponseTime = responseTime,
                                                    URL = url
                                                }
                    );
                    //await _auditService.SaveResult(new AuditResults() { AccessTime = startTime, Reachable = responseMessage.IsSuccessStatusCode, ResponseTime = responseTime, 
                    //    URL = url });
                }
                else
                {   
                    TimeSpan failedResponseTime = DateTime.Now - startTime;
                    await PersistRecordToDB(new AuditResults() { AccessTime = startTime , ResponseTime =  failedResponseTime , Reachable = false, URL = url});
                    _logger.LogCritical("Unsuccessful ping to {site} with status code : {code}", url, responseMessage.StatusCode);
                }
            }
            catch (Exception e)
            {
                TimeSpan failedResponseTime = DateTime.Now - startTime;
                await PersistRecordToDB(new AuditResults() { AccessTime = startTime, ResponseTime =  failedResponseTime, Reachable = false, URL = url });
                _logger.LogError("Unable to make the HTTP call , due to {error}", e.Message);
                return;
            }
            finally
            {
                client.Dispose();
            }
         

        }
        protected async Task PersistRecordToDB (AuditResults result)
        {
            // was able to solve the conflict of using a scoped services inside this worker class
            // resources : https://learn.microsoft.com/en-us/dotnet/core/extensions/scoped-service
            using (IServiceScope scope = provider.CreateScope())
            {
                IAuditService scopedAuditService = scope.ServiceProvider.GetRequiredService<IAuditService>();
                await scopedAuditService.SaveResult(result);
            }
        }
    }
}
