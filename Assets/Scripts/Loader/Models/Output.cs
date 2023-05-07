using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Models {

    [DataContract]
    public class Output {
        [DataMember]
        public string state { get; set; }

        [DataMember]
        public string location { get; set; }

        [DataMember]
        public Content data { get; set; }
    }

    [DataContract]
    public class Content {
        [DataMember]
        public string id { get; set; }

        [DataMember]
        public string department { get; set; }

        [DataMember]
        public string school { get; set; }

        [DataMember]
        public string[] subjects { get; set; }

        [DataMember]
        public string[] chapters { get; set; }

        [DataMember]
        public string[] content { get; set; }
    }

}
