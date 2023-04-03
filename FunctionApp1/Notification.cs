using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FunctionApp1
{

    public class Notifications
    {
        [JsonProperty(PropertyName = "value")]
        public Notification[] Items { get; set; }

        [JsonProperty(PropertyName = "validationTokens")]
        public string[] ValidationTokens { get; set; }
    }
    public class Notification
    {
        [JsonProperty(PropertyName = "changeType")]
        public string ChangeType { get; set; }

        [JsonProperty(PropertyName = "clientState")]
        public string ClientState { get; set; }

        [JsonProperty(PropertyName = "resource")]
        public string Resource { get; set; }

        [JsonProperty(PropertyName = "subscriptionExpirationDateTime")]
        public DateTimeOffset SubscriptionExpirationDateTime { get; set; }

        [JsonProperty(PropertyName = "subscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty(PropertyName = "resourceData")]
        public ResourceData ResourceData { get; set; }

        [JsonProperty(PropertyName = "encryptedContent")]
        public EncryptedContent encryptedContent { get; set; }
    }

    public class ResourceData
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "@odata.type")]
        public string ODataEType { get; set; }

        [JsonProperty(PropertyName = "@odata.id")]
        public string ODataId { get; set; }
    }

    public class EncryptedContent
    {
        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }
        
        [JsonProperty(PropertyName = "dataSignature")]
        public string DataSignature { get; set; }
        
        [JsonProperty(PropertyName = "dataKey")]
        public string DataKey { get; set; }
        
        [JsonProperty(PropertyName = "encryptionCertificateId")]
        public string EncryptionCertificateId { get; set; }

        [JsonProperty(PropertyName = "encryptionCertificateThumbprint")]
        public string EncryptionCertificateThumbprint { get; set; }
    }
}
