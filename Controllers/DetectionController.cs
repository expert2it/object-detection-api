using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebAPI.NetCore.Core;
using WebAPI.NetCore.Models;

namespace WebAPI.NetCore.Controllers
{
    //[Authorize]
    [Produces("application/json")]
    [Route("[controller]/[action]")]
    [EnableCors]
    public class DetectionController : Controller
    {
        // Reading "appsettings" jwt section
        private readonly Microsoft.Extensions.Options.IOptions<Jwt> _jwtSettings;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DetectionController> _logger;
        private readonly Utils _utils;
        public DetectionController(Microsoft.Extensions.Options.IOptions<Jwt> jwtSettings, IWebHostEnvironment env, ILogger<DetectionController> logger)
        {
            _jwtSettings = jwtSettings;
            _env = env;
            _logger = logger;
            _utils = new Utils();
        }

        //POST api/values
        [HttpPost]
        public async Task<IActionResult> ObjectDetection(IEnumerable<IFormFile> files, bool detect=true)
        {
            try
            {
                string url = HttpContext?.Request?.Host.ToUriComponent();
                // Better to be used by If - Else to avoid null expeption!!!
                url = HttpContext.Request.IsHttps ? "https://" + url : "http://" + url;

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
                string[] FileNames = new string[files.Count()];//Path.GetTempFileName();
                //Uri[] URLs = new Uri[FileNames.Length];
                string[] URLs = new string[FileNames.Length];

                int i = 0;
                foreach (var formFile in files)
                {
                    if (formFile.Length > 0 && _utils.GetFileExtension(Path.GetExtension(formFile.FileName)))
                    {
                        URLs[i] = url + "/Processor/uploads/" + formFile.FileName;
#if DEBUG
                        Console.WriteLine(formFile.Name + " -- " + formFile.FileName);
#endif
                        FileNames[i] = Path.Combine(_env.WebRootPath, @"Processor\uploads", formFile.FileName);
#if DEBUG
                        Console.WriteLine(Url.Content(FileNames[i]));
#endif
                        using (var stream = new FileStream(FileNames[i], FileMode.Create, FileAccess.ReadWrite))
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
                }

                return Ok(new { Count = files.Count(), URLs, Detection, Total_Size }); //, path = FileNames
            }
            catch (Exception exp)
            {
                Console.WriteLine("Exception generated when uploading file - " + exp.Message);
                _logger.LogError(exp, "ObjectDetection");
                //WriteLog("Exception generated when uploading file - " + exp.Message, true);
                string message = $"{ exp.Message } - file / upload failed!";
                return Json(message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> BytesObjectDetection(byte[] image, string filename = "sample.jpg", bool detect = true)
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
                    URLs[0] = url + "/Processor/uploads/" + filename;
#if DEBUG
                    Console.WriteLine(filename);
#endif
                    FileNames[0] = Path.Combine(_env.WebRootPath, @"Processor\uploads", filename);
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
                _logger.LogError(exp, "BytesObjectDetection");
                //WriteLog("Exception generated when uploading file - " + exp.Message, true);
                string message = $"{ exp.Message } - file / upload failed!";
                return Json(message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> UrlObjectDetection(string[] url, bool detect = true)
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
                        var nameSplit = address.Split('/');
                        string filename = nameSplit[^1]; // Index Operator

                        if (_utils.GetFileExtension(filename))
                        {
                            // Get file url
                            URLs[i] = baseUrl + "/Processor/uploads/" + filename;
                            FileNames[i] = Path.Combine(_env.WebRootPath, @"Processor\uploads", filename);
#if DEBUG
                            Console.WriteLine(Url.Content(FileNames[i]));
#endif

                            try
                            {
                                using HttpClient httpClient = new HttpClient();
                                await System.IO.File.WriteAllBytesAsync(FileNames[i], await httpClient.GetByteArrayAsync(address));
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
                _logger.LogError(exp, "UrlObjectDetection");
                //WriteLog("Exception generated when uploading file - " + exp.Message, true);
                string message = $"{ exp.Message } - file / upload failed!";
                return Json(message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> TextRecognition(ICollection<IFormFile> files, bool detect = true, double padding= 0.0)
        {
            try
            {


                string url = HttpContext?.Request?.Host.ToUriComponent();
                // Better to be used by If - Else to avoid null expeption!!!
                url = HttpContext.Request.IsHttps ? "https://" + url : "http://" + url;

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
                        URLs[i] = url + "/Processor/uploads/" + formFile.FileName;
#if DEBUG
                        Console.WriteLine(formFile.Name + " -- " + formFile.FileName);
#endif
                        FileNames[i] = Path.Combine(_env.WebRootPath, @"Processor\uploads", formFile.FileName);
#if DEBUG
                        Console.WriteLine(Url.Content(FileNames[i]));
#endif
                        using (var stream = new FileStream(FileNames[i], FileMode.Create, FileAccess.ReadWrite))
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
                    Detection = await DetectObject(FileNames, "ocr", (long)padding);
                }

                return Ok(new { files.Count, URLs, Detection, Total_Size }); //, path = FileNames
            }
            catch (Exception exp)
            {
                Console.WriteLine("Exception generated when uploading file - " + exp.Message);
                _logger.LogError(exp, "TextRecognition");
                //WriteLog("Exception generated when uploading file - " + exp.Message, true);
                string message = $"{ exp.Message } - file / upload failed!";
                return Json(message);
            }
        }
        private async Task<object> DetectObject(string[] name, string method, double padding)
        {
            try
            {
                using var client = new HttpClient();
                HttpResponseMessage result = await client.PostAsync("http://127.0.0.1:5000/" + method, new JsonContent(new { name, padding }));
                string responseContent = await result.Content.ReadAsStringAsync();
                var x = Json(responseContent);
                return JsonConvert.DeserializeObject<object>(x.Value.ToString());
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, "DetectObject");
                return Json(exp.Message);
            }
        }
        [HttpGet]
        public IActionResult GetImage(string name)
        {
            //Directory.GetCurrentDirectory() OR _env.WebRootPath
            var file = Path.Combine(_env.WebRootPath,
                                    "Processor", "uploads", name);

            return PhysicalFile(file, "image/jpeg");
        }
    }
}
