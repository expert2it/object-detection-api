using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using WebAPI.NetCore.Interfaces;
using WebAPI.NetCore.Models;

namespace WebAPI.NetCore.Controllers
{
    //[Produces("application/json")]
    [Route("[controller]/[action]")]
    public class TokenController : Controller
    {
        // ALternative method To access appsettings Jwt Section
        private readonly IConfiguration _config;
        private readonly ILogger<TokenController> _logger;
        private readonly IUserService _userService;
        //IUser iuser = null;
        public TokenController(IConfiguration config, ILogger<TokenController> logger, IUserService userService)
        {
            _config = config;
            _logger = logger;
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Produces("application/json", Type = typeof(string))]
        public IActionResult CreateToken(LoginModel login)
        {
            if (ModelState.IsValid)
            {
                var user = Authenticate(login);
                //var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (user.Result != null) // result.Succeeded
                {
                    return Ok(new
                    {
                        user.Result.Name,
                        user.Result.Email,
                        user.Result.Username,
                        user.Result.Birthdate,
                        registered = user.Result.Created_date.ToString("dd/MM/yyyy hh:mm"),
                        token = BuildToken(user.Result)
                    });
                }
                return Unauthorized();
            }
            return BadRequest(ModelState);
        }

        private string BuildToken(Users user)
        {
            #region Factory Pattern
            // One way to use
            // Factory Pattern to create User object and do Validation
            ///// iuser = Factory<IUser>.Create(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(user.Usertype.ToLower()));
            ///// var validation = iuser.Validate();
            // TODO: do the logic
            #endregion
            // Claims can be used as an Authorisation measure
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.Name),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Birthdate, user.Birthdate.ToString("yyyy-MM-dd")),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddHours(12),
              signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<Users> Authenticate(LoginModel login)
        {
            try
            {
                var client = MongoDBClient.Connect();
                var database = client.GetDatabase("netCoreAPI");
                var collection = database.GetCollection<Users>("users");
                var document = await collection.Find(u=> u.Username.Equals(login.Username) && u.Password.Equals(login.Password)).FirstOrDefaultAsync();
                //Console.WriteLine(document.ToString());
                return document;
            }
            catch (Exception exc) { _logger.LogError(exc, "Authenticate"); return null; }
            #region Manual Authentication
            //Users user = null;
            //// Full Access Admin Example
            //if (login.Username == "admin" && login.Password == "admin")
            //{
            //    user = new Users { name = "Admin M", email = "mohsen@domain.com", birthdate = DateTime.Parse("Jan 1, 1981") };
            //}
            //if (login.Username == "mohsen" && login.Password == "secret")
            //{
            //    //user = await _userManager.FindByEmailAsync(model.Email);
            //    user = new Users { name = "Mohsen M", email = "mohsen@domain.com" };
            //}
            #endregion
        }
        [AllowAnonymous]
        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> CheckUserAsync([FromBody]LoginModel login)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var client = MongoDBClient.Connect();
                    var database = client.GetDatabase("netCoreAPI");
                    var collection = database.GetCollection<BsonDocument>("users");
                    var count = collection.CountDocuments(new BsonDocument());
                    // we don’t need all the data contained in a document. 
                    var projection = Builders<BsonDocument>.Projection.Exclude("_id");
                    var document = await collection.Find(new BsonDocument()).Project(projection).FirstOrDefaultAsync();
                    //Console.WriteLine(document.ToString());
                    _logger.LogInformation("CheckUserAsync \n" + document.ToString());
                    //var doc = document.ToJson(typeof(Users));
                    var json = Json(document.ToString());
                   // var j = Json(doc);
                    if (document != null) // result.Succeeded
                    {
                        //var user = Json(document);
                        //var jsonWriterSettings = new MongoDB.Bson.IO.JsonWriterSettings { OutputMode = MongoDB.Bson.IO.JsonOutputMode.Strict };
                        //var user1 = document.ToJson(jsonWriterSettings);         
                        object val = BsonTypeMapper.MapToDotNetValue(document);
                        string jsonString = JsonConvert.SerializeObject(val);
                        var user = JsonConvert.DeserializeObject<Users>(jsonString);
                        return Ok(new { user });
                    }
                    return Unauthorized();
                }
                catch (Exception exc)
                {
                    _logger.LogError(exc, "CheckUserAsync");
                    return Json(exc.Message);
                }
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> CreateUserAsync([FromBody]Users user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //user.Created_date = DateTime.Now;

                    if (await _userService.Get(user.Username, null) == null)
                    {
                        await _userService.CreateAsync(user);
                        _logger.LogInformation("CreateUserAsync " + user.Username);
                        return Ok(new { user });
                    }
                    else
                        return Conflict("User already exists in Database!");

                }
                catch (Exception exc)
                {
                    _logger.LogError(exc, "CreateUserAsync");
                    return Conflict(exc.Message);
                }
            }
            return BadRequest(ModelState);
        }
        [HttpGet]
        public ActionResult<IEnumerable<Users>> Get()
        {
            return _userService.Get();
        }
        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var user = _userService.Get(null, id);

            if (user.Result == null)
            {
                return NotFound();
            }

            _userService.Remove(user.Result);

            return Ok("User deleted successfully!");
        }
        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, [FromBody]Users user)
        {
            if (_userService.Get(null, id).Result == null)
            {
                return NotFound();
            }

            _userService.Update(id, user);

            return Ok("User updated successfully!");
        }
    }
}