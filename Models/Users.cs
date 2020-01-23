using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.NetCore.Models
{
    public class Users
    {
        // [IgnoreDataMember]
        [SwaggerExcludeAttribute]
        public ObjectId Id { get; set; }
        [SwaggerExclude]
        public int UserID { get; set; }
        [Required, BsonElement("Name")]
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime Birthdate { get; set; } = DateTime.Now;
        [Required, BsonElement("Username")]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public string Usertype { get; set; }
        private DateTime created_date = DateTime.Now;
        [SwaggerExclude]
        public DateTime Created_date
        {
            get { return created_date; }
            set { created_date = value.ToLocalTime(); }
        } 
    }
    public class LoginModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }

}
