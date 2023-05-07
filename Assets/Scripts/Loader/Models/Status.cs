using System;
using System.Runtime.Serialization;


namespace Backend.Models {
    [DataContract]
    public class Status {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string instanceId { get; set; }
        [DataMember]
        public string runtimeStatus { get; set; }
        [DataMember]
        public string input { get; set; }
        [DataMember]
        public string customStatus { get; set; }
        [DataMember]
        public string output { get; set; }
        [DataMember]
        public string createdTime { get; set; }
        [DataMember]
        public string lastUpdatedTime { get; set; }
    }
}
