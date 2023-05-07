using System.Runtime.Serialization;

[DataContract]
public class DurableTask {
    [DataMember]
    public string id { get; set; }
    [DataMember]
    public string statusQueryGetUri { get; set; }
    [DataMember]
    public string sendEventPostUri { get; set; }
    [DataMember]
    public string terminatePostUri { get; set; }
    [DataMember]
    public string purgeHistoryDeleteUri { get; set; }
    [DataMember]
    public string restartPostUri { get; set; }
    [DataMember]
    public string suspendPostUri { get; set; }
    [DataMember]
    public string resumePostUri { get; set; }
}