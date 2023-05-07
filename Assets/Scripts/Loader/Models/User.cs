using System.Runtime.Serialization;

namespace Backend.Models {
    [DataContract]
    public class User {

        [DataMember(Name = "password")]
        public string Password { get; set; }

        [DataMember(Name = "username")]
        public string Username { get; set; }
    }
}
