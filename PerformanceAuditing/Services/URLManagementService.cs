using Microsoft.Extensions.Options;
using PerformanceAuditing.Data;
using PerformanceAuditing.Workers;
using System.Text;
using System.Text.Json;


namespace PerformanceAuditing.Services
{
    public class URLManagementService
    {
        public readonly Queue<string> _urls = new Queue<string>(); 
        private readonly IOptions<WorkerSettings> _workerSettings;
        private readonly ILogger<URLManagementService> logger;
        string jsonContent = string.Empty;
        string defaultJSON = "Data/urls.json";
        public URLManagementService(IOptions<WorkerSettings> workerSettings,ILogger<URLManagementService> logger) { 
            this._workerSettings = workerSettings;
            this.logger=logger;
        }
        public void SeedFromJson()
        {
            try
            {   
                //check if the file exists
                if(!File.Exists(_workerSettings.Value.JSONFilePath)) {
                    logger.LogCritical("JSON file was not found.");
                    throw new FileNotFoundException("JSON file was not found.");
                }
                FileWatcher();
                using (var reader = new StreamReader(_workerSettings.Value.JSONFilePath ?? defaultJSON, Encoding.UTF8))
                {
                    jsonContent = reader.ReadToEnd();
                   if(string.IsNullOrEmpty(jsonContent))
                    {
                        logger.LogCritical("Json file was not loaded properly or not read.");
                        throw new Exception("JSON file was not loaded/read properly");
                       
                    }

                    URLRecord? record = JsonSerializer.Deserialize<URLRecord>(jsonContent);
                   
                    if (record != null)
                    {
                      
                        foreach (var item in record.URLs)
                        {

                            this.AddURL(item); //utilize the method, to check duplicates to the current collection
                         
                        }
                    }
                   

                }

            }
            catch (Exception e)
            {

                logger.LogError("There was an error with seeding json data. \n Error Message: {message}", e.Message);
            }
          



        }

        public void FileWatcher()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = @"Data/";
            watcher.Filter = "urls.json";
            watcher.Changed += OnFileChanged;
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if(e.ChangeType == WatcherChangeTypes.Changed)
            {
                logger.LogInformation("Something has changed");
            }
        }

        public void AddURL (string url)
        {
            if(!_urls.Contains(url))
                _urls.Enqueue (url);
        }
        public string? NextUrl()
        {
            if(_urls.Count > 0)
            {
               return _urls.Dequeue ();
            }
            else
            {
                return string.Empty; //return empty string if there are no more items in the queue,  anyway this is handled in the service implementor
            }
        }
    }
}
