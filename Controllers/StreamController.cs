using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebAPI.NetCore.Models;
//using Microsoft.AspNetCore.SignalR;

namespace WebAPI.NetCore.Controllers
{
    [Route("[controller]/[action]")]
    [EnableCors]
    public class StreamController : Controller
    {
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<Core.ClientHub> _hub;
        private readonly Microsoft.Extensions.Options.IOptions<Jwt> _jwtSettings;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<StreamController> _logger;
        public StreamController(Microsoft.Extensions.Options.IOptions<Jwt> jwtSettings, IWebHostEnvironment env, ILogger<StreamController> logger, Microsoft.AspNetCore.SignalR.IHubContext<Core.ClientHub> hub)
        {
            _hub = hub;
            _jwtSettings = jwtSettings;
            _env = env;
            _logger = logger;
        }
        // GET api/values
        //[HttpGet]
        //public async Task Get()
        //{
        //    var context = ControllerContext.HttpContext;
        //    var isSocketRequest = context.WebSockets.IsWebSocketRequest;

        //    if (isSocketRequest)
        //    {
        //        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
        //        await GetMessages(context, webSocket);
        //    }
        //    else
        //    {
        //        context.Response.StatusCode = 400;
        //    }
        //}

        //private async Task GetMessages(HttpContext context, WebSocket webSocket)
        //{
        //    var messages = new[]
        //    {
        //    "Message1",
        //    "Message2",
        //    "Message3",
        //    "Message4",
        //    "Message5"
        //    };

        //    foreach (var message in messages)
        //    {
        //        var bytes = Encoding.ASCII.GetBytes(message);
        //        var arraySegment = new ArraySegment<byte>(bytes);
        //        await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        //        Thread.Sleep(2000); //sleeping so that we can see several messages are sent
        //    }

        //    await webSocket.SendAsync(new ArraySegment<byte>(null), WebSocketMessageType.Binary, false, CancellationToken.None);
        //}
        [HttpGet]
        public async Task<IActionResult> SendMessage(string message)
        {
            //private IHubContext<Core.ClientHub> _hub;
            //var hub = GlobalHost.ConnectionManager.GetHubContext("clientHub");
            //hubContext.Clients.All.send("name", "message");
            _hub.Clients.All.SendCoreAsync("ReceiveMessage", new object[] { "Message: ", message });
            return Ok("Message sent!");
        }
    }
}