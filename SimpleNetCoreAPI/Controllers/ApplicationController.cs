using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleNetCoreAPI.DataTransferObjects;
using SimpleNetCoreAPI.Models;
using SimpleNetCoreAPI.Server.Models;
using SimpleNetCoreAPI.WebSockets;

namespace SimpleNetCoreAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApplicationController: ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ApplicationWebSocketHandler _webSocketHandler;
        public ApplicationController(ApplicationDbContext context, ApplicationWebSocketHandler _handler)
        {
            _context= context;
            _webSocketHandler = _handler;
        }
        [HttpGet(Name = "GetApplications")]
        [Authorize]
        public ActionResult<IEnumerable<Application>> GetApplications()
        {
            Console.WriteLine("Getting applications");
            return Ok(_context.Applications.OrderBy(a=>a.Date).ToList());
        }
        [HttpPost(Name = "CreateApplication")]
        [Authorize]
        public async Task<ActionResult<Application>> CreateApplication([FromBody] ApplicationDTO applicationDto)
        {
            var application = new Application
            {
                Type = (Enums.ApplicationType)(applicationDto.type),
                Message = applicationDto.message,
                Date=DateTime.UtcNow
                // Set other properties as needed, or let them be auto-filled
            };

            _context.Applications.Add(application);
            _context.SaveChanges();
            await _webSocketHandler.NotifyClientsOfDataChangeAsync("Applications updated");
            return CreatedAtAction(nameof(GetApplications), new { id = application.Guid }, application);
        }
    }
}
