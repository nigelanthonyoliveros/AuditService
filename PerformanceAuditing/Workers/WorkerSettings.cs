namespace PerformanceAuditing.Workers
{
    public class WorkerSettings
    {
        //this reflects the configuration set in appsettings.json under WorkerSettings property
        public int InitialNumberOfWorkers { get; set; } = 3;
        public int CycleTime { get; set; }  // this is in milliseconds
        public string? JSONFilePath { get; set; } = @"Data/urls.json";
    }
}
