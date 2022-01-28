using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SendMail.Models
{
    public class RequestBodySendMailContact
    {
        public string UserName { get; set; }
        public string UserOrganization { get; set; }
        public string LoginId { get; set; }
        public string EmailAddress { get; set; }
        public string InquiryType { get; set; }
        [JsonProperty("CourseName[]")]
        public string CourseName { get; set; }
        [JsonProperty("DeviceType[]")]
        public string DeviceType { get; set; }
        [JsonProperty("Brower[]")]
        public string Brower { get; set; }
        [JsonProperty("Os[]")]
        public string Os { get; set; }
        public string Content { get; set; }
    }
}
