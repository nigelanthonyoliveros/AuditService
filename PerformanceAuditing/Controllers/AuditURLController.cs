using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PerformanceAuditing.Contracts;
using PerformanceAuditing.Services;

namespace PerformanceAuditing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditURLController : ControllerBase
    {
        private readonly URLManagementService urlmanager;
        private readonly IAuditService resultsService;

        public AuditURLController(URLManagementService urlmanager, IAuditService resultsService)
        {
            this.urlmanager=urlmanager;
            this.resultsService=resultsService;
        }
        [HttpPost]
        public IActionResult PostNewURL ([FromBody] string url)
        {   
            try
            {
                urlmanager.AddURL(url);
                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(e);
            }


        }


        [HttpGet]

        public async Task<IActionResult> GetAllAuditResultsAsync ()
        {
            var results = await resultsService.GetAllResults();
            if(results.Count > 0)
            {
                return Ok(results);
            }
            else
            {
                return NotFound(new {Message =  "Records are truncated or empty."});
            }
        }

        [HttpDelete]

        public async Task<IActionResult> FlushRecords()
        {
            var result = await resultsService.FlushRecords();
            if(result)
            {
                return Ok(new { Message = "Records has been truncated successfully!" });
            }
            return NotFound(new { Message = "Error while truncating." });

        }




    }
}
