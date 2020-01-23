using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CiliAPI.NetCore.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using WebAPI.NetCore.Core;
using WebAPI.NetCore.Models;

namespace WebAPI.NetCore.Controllers
{
    //[Authorize]
    [Produces("application/json")]
    [Route("[controller]/[action]")]
    [EnableCors]
    //[EnableCors] // default policy
    public class CiliController : Controller
    {
        // TODO Reading "appsettings" jwt section
        //private readonly Microsoft.Extensions.Options.IOptions<Jwt> _jwtSettings;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CiliController> _logger;
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<Core.ClientHub> _hub;
        private readonly Utils _utils;
        public CiliController(IWebHostEnvironment env, ILogger<CiliController> logger, Microsoft.AspNetCore.SignalR.IHubContext<Core.ClientHub> hub)
        {
            //_jwtSettings = jwtSettings;
            _env = env;
            _logger = logger;
            _hub = hub;
            _utils = new Utils();
        }

        //POST api/values
        [HttpPost]
        public async Task<IActionResult> CiliDetection(ICollection<IFormFile> files, bool detect=true)
        {
            try
            {
                string url = HttpContext?.Request?.Host.ToUriComponent();
                // Better to be used by If - Else to avoid null expeption!!!
                url = HttpContext?.Request != null && HttpContext.Request.IsHttps ? "https://" + url : "http://" + url;

                // TODO: This method "GetEnvironmentVariables" could be very useful in the future
                // var e = Environment.GetEnvironmentVariables();
#if DEBUG
                Console.WriteLine("You received the call!");
#endif
                //WriteLog("PostFiles call received!", true);
                //We would always copy the attachments to the folder specified above but for now dump it wherver....
                long Total_Size = files.Sum(f => f.Length);

                // full path to file in temp location
                var filePath = Path.GetTempFileName();
                string[] FileNames = new string[files.Count];//Path.GetTempFileName();
                //Uri[] URLs = new Uri[FileNames.Length];
                string[] URLs = new string[FileNames.Length];

                int i = 0;
                foreach (var formFile in files)
                {
                    if (formFile.Length > 0 && _utils.GetFileExtension(Path.GetExtension(formFile.FileName)))
                    {
                        URLs[i] = url + "/Processor/cilidetection/" + formFile.FileName;
#if DEBUG
                        Console.WriteLine(formFile.Name + " -- " + formFile.FileName);
#endif
                        FileNames[i] = Path.Combine(_env.WebRootPath, @"Processor\cilidetection", formFile.FileName);
#if DEBUG
                        Console.WriteLine(Url.Content(FileNames[i]));
#endif
                        await using (var stream = new FileStream(FileNames[i], FileMode.Create, FileAccess.ReadWrite))
                        {
                            await formFile.CopyToAsync(stream);
                            //formFile.CopyToAsync(stream);
                        }
                        i++;
                    }
                }

                // process uploaded files
                // Don't rely on or trust the FileName property without validation.
                //Displaying File Name for verification purposes for now -Rohit
                dynamic Detection = null;
                if (detect && FileNames.Length > 0)
                {
                    Detection = await DetectObject(FileNames, "detect", 0);
                    // TODO: If detected, Start a background Task to update MySQL & Elasticsearch!
                    Task.Run(() => SetRawData(JsonConvert.SerializeObject(new { files.Count, URLs, Detection })));
                }

                return Ok(new { files.Count, URLs, Detection, Total_Size }); //, path = FileNames
            }
            catch (Exception exp)
            {
                Console.WriteLine("Exception generated when uploading file - " + exp.Message);
                _logger.LogError(exp, "CiliDetection");
                //WriteLog("Exception generated when uploading file - " + exp.Message, true);
                string message = $"{ exp.Message } - file / upload failed!";
                return Json(message);
            }
        }

        private IActionResult SetRawData(string detection)
        {
            var det = new DetectionResult(detection);

            int leafCount = 0, ciliCount = 0;
            foreach (var obj in det.Detection)
            {
                Debug.Print("File Name: " + obj.Key);
                foreach (string s in obj.Value)
                {
                    Debug.Print("Property: " + s);
                    if (s.ToLower().Contains("leaf"))
                        leafCount++;
                    else if (s.ToLower().Contains("chilli"))
                        ciliCount++;
                }
            }

            if (leafCount <= 0 && ciliCount <= 0)
                return Json("Nothing detected!");

            ChiliModel data = new ChiliModel
            {
                CheckedBy = "Robot Tank",
                Datetime = DateTime.Now.ToString("dd/MM/yyyy"),
                Image = JsonConvert.SerializeObject(det.URLs),
                LastUpdate = DateTime.Now,
                NumberOfFruits = ciliCount,
                NumberOfLeaves = leafCount,
                LeafColor = leafCount > 0 ? "Green" : "-", 
                PlantId = "R", // TODO: add plant ID
                TreeHeight = "40", // TODO
                LeafLength = leafCount > 0 ? "10" : "0", // TODO
                LeafWidth = leafCount > 0 ? "4.84" : "0", // TODO
                FruitLength = ciliCount > 0 ? "12" : "-", // TODO
                FruitWidth = ciliCount > 0 ? "1.9" : "-", // TODO
                FruitColor = ciliCount > 0 ? "Green" : "-" // TODO
            };

            var client = new RestClient("https://api.cilibot.com/DataCollect/SetRawData");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Host", "api.cilibot.com");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("data", JsonConvert.SerializeObject(data), ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            return Json(response.Content);
        }

        [HttpPost]
        public async Task<IActionResult> BytesCiliDetection(byte[] image, string filename = "sample.jpg", bool detect = true)
        {
            try
            {
                if (!_utils.GetFileExtension(filename))
                    throw new FileNotFoundException();
                string url = HttpContext?.Request?.Host.ToUriComponent();
                // Better to be used by If - Else to avoid null expeption!!!
                url = HttpContext.Request.IsHttps ? "https://" + url : "http://" + url;
                
                //WriteLog("PostFiles call received!", true);
                //We would always copy the attachments to the folder specified above but for now dump it wherver....
                long Total_Size = image.LongLength;

                // full path to file in temp location
                var filePath = Path.GetTempFileName();
                string[] FileNames = new string[1];//Path.GetTempFileName();
                //Uri[] URLs = new Uri[FileNames.Length];
                string[] URLs = new string[FileNames.Length];

                dynamic Detection = null;
                if (Total_Size > 0)
                {
                    URLs[0] = url + "/Processor/cilidetection/" + filename;
#if DEBUG
                    Console.WriteLine(filename);
#endif
                    FileNames[0] = Path.Combine(_env.WebRootPath, @"Processor\cilidetection", filename);
#if DEBUG
                    Console.WriteLine(Url.Content(FileNames[0]));
#endif

                    // Save the stream to file
                    await System.IO.File.WriteAllBytesAsync(FileNames[0], image);

                    if (detect && FileNames.Length > 0)
                    {
                        Detection = await DetectObject(FileNames, "detect", 0);
                    }
                }
                return Ok(new { FileNames.Length, URLs, Detection, Total_Size }); //, path = FileNames
            }
            catch (Exception exp)
            {
                Console.WriteLine("Exception generated when uploading file - " + exp.Message);
                _logger.LogError(exp, "BytesCiliDetection");
                //WriteLog("Exception generated when uploading file - " + exp.Message, true);
                string message = $"{ exp.Message } - file / upload failed!";
                return Json(message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> UrlCiliDetection(string[] url, bool detect = true)
        {
            try
            {
                string baseUrl = HttpContext?.Request?.Host.ToUriComponent();
                // Better to be used by If - Else to avoid null expeption!!!
                baseUrl = HttpContext.Request.IsHttps ? "https://" + baseUrl : "http://" + baseUrl;
                      
                // Loading image from URL
                //List<byte[]> images = new List<byte[]>();
                dynamic Detection = null;
                string[] FileNames = new string[url.Length];
                string[] URLs = new string[FileNames.Length];
                int i = 0;
                foreach (string address in url)
                {
                    if (!string.IsNullOrEmpty(address))
                    {
                        // Get Filename
                        string[] nameSplit = address.Split('/');
                        // Introducing C# 8.0 Index Operator feature for slicing array 
                        // ---> instead of "nameSplit.Length - 1" we used below code:
                        string filename = nameSplit[^1];

                        if (_utils.GetFileExtension(filename))
                        {
                            // Get file url
                            URLs[i] = baseUrl + "/Processor/cilidetection/" + filename;
                            FileNames[i] = Path.Combine(_env.WebRootPath, @"Processor\cilidetection", filename);
#if DEBUG
                            Console.WriteLine(Url.Content(FileNames[i]));
#endif

                            try
                            {
                                using var client = new HttpClient();
                                await System.IO.File.WriteAllBytesAsync(FileNames[i], await client.GetByteArrayAsync(address));
                            }
                            catch (Exception exp) { URLs[i] = exp.Message; }
                            i++;
                        }
                    }
                }
                if (detect && FileNames.Length > 0)
                {
                    Detection = await DetectObject(FileNames, "detect", 0);
                }
                return Ok(new { FileNames.Length, URLs, Detection });
            }
            catch (Exception exp)
            {
                Console.WriteLine("Exception generated when uploading file - " + exp.Message);
                _logger.LogError(exp, "UrlCiliDetection");
                //WriteLog("Exception generated when uploading file - " + exp.Message, true);
                string message = $"{ exp.Message } - file / upload failed!";
                return Json(message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> CameraStreamDetection(string url)
        {
            try
            {
                dynamic Detection = null;
                if (!string.IsNullOrEmpty(url))
                {
                    // Start the Socket listening
                    SingletonDetection._mode = true;
                    if (SingletonDetection.socket == null)
                    {
                        Task.Run(() => SingletonDetection.Instance.StartListening(_env, _hub, true));
                    }
                    DetectObject(new string[] { url }, "camera", 0);
                    Detection = "Camera detection is running, please follow these steps: \n" +
                        "1- See real-time result at: https://ai.cilibot.com/signalr.html \n" +
                        "2- Shutdown the TCP Socket when you are done: https://ai.cilibot.com/Cili/CloseServerSocket";
                }
                return Ok(new { Detection });
            }
            catch (Exception exp)
            {
                Console.WriteLine("Exception generated when uploading file - " + exp.Message);
                _logger.LogError(exp, "CameraStreamDetection");
                //WriteLog("Exception generated when uploading file - " + exp.Message, true);
                string message = $"{ exp.Message } - CameraStreamDetection failed!";
                return Json(message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> MovementStreamDetection(string url)
        {
            try
            {
                dynamic Detection = null;
                if (!string.IsNullOrEmpty(url))
                {
                    // Start the Socket listening
                    SingletonMovement._mode = true;
                    if (SingletonMovement.socket == null)
                    {
                        Task.Run(() => SingletonMovement.Instance.StartListening(_env, _hub, true));
                    }
                    DetectObject(new string[] { url }, "ocr", 0);
                    Detection = "Movement detection is running, please follow these steps: \n" +
                        "1- See real-time result at: https://ai.cilibot.com/movement.html \n" +
                        "2- Shutdown the TCP Socket when you are done: https://ai.cilibot.com/Cili/CloseMovementSocket";
                }
                return Ok(new { Detection });
            }
            catch (Exception exp)
            {
                Console.WriteLine("Exception generated when uploading file - " + exp.Message);
                _logger.LogError(exp, "CameraStreamDetection");
                //WriteLog("Exception generated when uploading file - " + exp.Message, true);
                string message = $"{ exp.Message } - CameraStreamDetection failed!";
                return Json(message);
            }
        }
        [HttpGet]
        public IActionResult CloseServerSocket(string mode = "off")
        {
            if (mode.ToLower() == "on")
            {
                SingletonDetection._mode = true;
                if (SingletonDetection.socket == null)
                {
                    Task.Run(() => SingletonDetection.Instance.StartListening(_env, _hub, mode.ToLower().Equals("on")));
                }
                return Ok("TCP Socket running on 127.0.0.1:11111");
            }
            else
            {
                SingletonDetection._mode = false;
                return Ok("TCP Socket closed.");
            }
        }
        [HttpGet]
        public IActionResult CloseMovementSocket(string mode = "off")
        {
            if (mode.ToLower() == "on")
            {
                SingletonMovement._mode = true;
                if (SingletonMovement.socket == null)
                {
                    Task.Run(() => SingletonMovement.Instance.StartListening(_env, _hub, mode.ToLower().Equals("on")));
                }
                return Ok("TCP Socket running on 127.0.0.1:11112");
            }
            else
            {
                SingletonMovement._mode = false;
                return Ok("TCP Socket closed.");
            }
        }
        private async Task<object> DetectObject(string[] name, string method, double padding)
        {
            try
            {
                using var client = new HttpClient();
                HttpResponseMessage result = await client.PostAsync("http://127.0.0.1:5100/" + method, new JsonContent(new { name, padding }));
                string responseContent = await result.Content.ReadAsStringAsync();
                var x = Json(responseContent);
                return JsonConvert.DeserializeObject<object>(x.Value.ToString());
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, "Cili --> DetectObject");
                return Json(exp.Message);
            }
        }
        [HttpGet]
        public IActionResult GetImage(string name)
        {
            //Directory.GetCurrentDirectory() OR _env.WebRootPath
            var file = Path.Combine(_env.WebRootPath,
                                    "Processor", "cilidetection", name);

            return PhysicalFile(file, "image/jpeg");
        }       
    }
}
