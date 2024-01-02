using Microsoft.Extensions.Options;
using PerformanceAuditing.Data;
using PerformanceAuditing.Workers;
using System.Text;
using System.Text.Json;


namespace PerformanceAuditing.Services
{
    public class URLManagementService
    {
        private readonly Queue<string> _urls = new Queue<string>(); 
        private readonly IOptions<WorkerSettings> _workerSettings;
        private readonly ILogger<URLManagementService> logger;
        string jsonContent = string.Empty;
        private URLRecord previousRecord = null!;
        string defaultJSON = "Data/urls.json";
        FileSystemWatcher watcher;
        public URLManagementService(IOptions<WorkerSettings> workerSettings,ILogger<URLManagementService> logger) { 
            this._workerSettings = workerSettings;
            this.logger=logger;

            //Setup the FileSystemWatcher instance
            var splittedpath = (workerSettings.Value.JSONFilePath??defaultJSON).Split('/');
            string directory = Path.Join(Environment.CurrentDirectory, splittedpath[0]);
            string file = splittedpath[1];
            watcher = new FileSystemWatcher(directory, file);


        }
        //! this method simply loads data from the json file
        public void SeedFromJson()
        {

            try
            {   
                //check if the file exists
                if(!File.Exists(_workerSettings.Value.JSONFilePath)) {
                    logger.LogCritical("JSON file was not found.");
                    throw new FileNotFoundException("JSON file was not found.");
                }
            
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
                            if(record != previousRecord && previousRecord != null)
                            {
                            var newUrls = record.URLs.Except(previousRecord.URLs);
                                    foreach (var item in newUrls )
                                    {
                                     this.AddURL(item);
                                    }
                                    //! Collate all existing and new records into one collection and assign it into our previous record data holder
                                    previousRecord.URLs =  previousRecord.URLs.Union(newUrls);
                            }
                            else if(previousRecord is null) {


                            #region -- new set of data (startup data based on the json)--
                            foreach (var item in record.URLs)
                            {
                                //if(!_urls.Contains(item)) // Add if the url doesn't exist in the previous queue
                                this.AddURL(item); //utilize the method, to check duplicates to the current collection

                            } 
                            previousRecord = record;
                            #endregion

                              }
                            else
                            {
                            logger.LogWarning("Attempting to duplicate the records.."); //do nothing
                            }
                           
                        }

                     
                  

                    
                   

                }

            }
            catch (Exception e)
            {

                logger.LogError("There was an error with seeding json data. \n Error Message: {message}", e.Message);
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
                return _urls.Dequeue();
            }
            else
            {
                return string.Empty; //return empty string if there are no more items in the queue,  anyway this is handled in the service implementor
            }
        }

      
        #region -- Change Detector --
        public void Monitor()
        {
            logger.LogInformation("Watching for changes on {file}", watcher.Filter);
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
            watcher.Changed += onChange;
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;

        }

        private void onChange(object sender, FileSystemEventArgs e)
        {
            logger.LogInformation("Change detected on the json file , was detected. Reseeding data...");
            SeedFromJson();
        }
        #endregion
    }
}
