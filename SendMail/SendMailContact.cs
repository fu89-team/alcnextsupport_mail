using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendMail.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Http;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Linq;

namespace SendMail
{
    public class SendMailContact
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;
        public SendMailContact(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        [FunctionName("SendMailContact")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var formData = await req.ReadFormAsync();
            var data = new Dictionary<string, string>();
            foreach (var Keys in formData.Keys)
            {
                data.Add(Keys, formData[Keys]);
            }
            var json = JsonConvert.SerializeObject(data);
            var requestBodySendMailContact = JsonConvert.DeserializeObject<RequestBodySendMailContact>(json);
            try
            {
                //string rootPath = _hostingEnvironment.WebRootPath + "\\";

                //log.LogInformation(rootPath);
                //string path = @"Contact.txt";
                //string path = $@"{_hostingEnvironment.WebRootPath}\Template\Contact.txt";
                //var contentMail = requestBodySendMailContact.EmailAddress;
                var contentMail = "------------ \r\n" +
"問合せ日時\r\n" +
"#DATE# \r\n" +
"------------ \r\n" +
"氏名\r\n" +
"#USER_NAME#\r\n" +
"-----------\r\n" +
"所属の団体(大学・学部名、勤め先など)\r\n" +
"#USER_ORGANIZATION#\r\n" +
"-----------\r\n" +
"返信用メールアドレス\r\n" +
"#MAIL_ADDRESS#\r\n" +
"-----------\r\n" +
"ログインID（アカウント / User ID）\r\n" +
"#LOGIN_ID#\r\n" +
"-----------\r\n" +
"お問い合わせの種別\r\n" +
"#INQUIRY_TYPE#\r\n" +
"-----------\r\n" +
"コースについて\r\n" +
"#COURSE_NAME#\r\n" +
"-----------\r\n" +
"端末について\r\n" +
"#DEVICE_TYPE#\r\n" +
"-----------\r\n" +
"ブラウザについて\r\n" +
"#BROWSER#\r\n" +
"-----------\r\n" +
"端末のOSについて\r\n" +
"#OS#\r\n" +
"-----------\r\n" +
"お問い合わせ内容\r\n\r\n" +
"#CONTENT#\r\n" +
"-----------\r\n" +
"個人情報保護規約\r\n" +
"下記、プライバシー保護方針を確認の上同意します\r\n" +
"------------\r\n" +
"■このメールにお心あたりのない方は下記にご連絡お願い致します。\r\n" +
"問合せ先　ALC NetAcademy NEXT サポート係 nextsupport@alc.co.jp\r\n";

                contentMail = contentMail.Replace("#DATE#", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                contentMail = contentMail.Replace("#USER_NAME#", requestBodySendMailContact.UserName);
                contentMail = contentMail.Replace("#USER_ORGANIZATION#", requestBodySendMailContact.UserOrganization);
                contentMail = contentMail.Replace("#LOGIN_ID#", requestBodySendMailContact.LoginId);
                contentMail = contentMail.Replace("#MAIL_ADDRESS#", requestBodySendMailContact.EmailAddress);
                contentMail = contentMail.Replace("#INQUIRY_TYPE#", requestBodySendMailContact.InquiryType);
                contentMail = contentMail.Replace("#COURSE_NAME#", string.Join(", ", requestBodySendMailContact.CourseName));
                contentMail = contentMail.Replace("#DEVICE_TYPE#", string.Join(", ", requestBodySendMailContact.DeviceType));
                contentMail = contentMail.Replace("#BROWSER#", string.Join(", ", requestBodySendMailContact.Brower));
                contentMail = contentMail.Replace("#OS#", string.Join(", ", requestBodySendMailContact.Os));
                contentMail = contentMail.Replace("#CONTENT#", requestBodySendMailContact.Content);

                var subject = "ALC NetAcademy お問合せ：" + requestBodySendMailContact.InquiryType;
                await SendMail(subject, contentMail, new EmailAddress(Environment.GetEnvironmentVariable("ADMIN_EMAIL"), Environment.GetEnvironmentVariable("ADMIN_NAME")), log);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new Result()
                    {
                        message = "Complete",
                        status = StatusCode.OK
                    }), Encoding.UTF8, "application/json")
                };
            }
            catch (Exception ex)
            {
                log.LogInformation("Error message: " + ex.Message);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new Result()
                    {
                        message = ex.Message,
                        status = StatusCode.InternalServerError
                    }), Encoding.UTF8, "application/json")
                };
            }
        }

        public async Task SendMail(string subject, string plainTextContent, EmailAddress to, ILogger log)
        {
            try
            {
                var from = new EmailAddress(Environment.GetEnvironmentVariable("FROM_EMAIL"), Environment.GetEnvironmentVariable("FROM_NAME"));
                var client = new SendGridClient(Environment.GetEnvironmentVariable("SENDGRID_API_KEY"));
                var msg = new SendGridMessage()
                {
                    From = from,
                    Subject = subject,
                    PlainTextContent = plainTextContent
                };
                msg.AddTo(to);
                await client.SendEmailAsync(msg);
                log.LogInformation("send mail success");
            }
            catch (Exception ex)
            {
                log.LogInformation("Error message: " + ex.Message);
            }
            
        }
    }
}
