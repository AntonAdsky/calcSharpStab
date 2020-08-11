using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Net;

namespace CalculatorEmul.Controllers
{
    [Route("[controller]")]
    public class CalculatorController : Controller
    {
        private readonly IConfiguration Configuration;
        private bool _logginEnabled;

        public CalculatorController(IConfiguration configuration)
        {
            Configuration = configuration;
            _logginEnabled = bool.Parse(Configuration["LogIncomming"]);
        }

        [HttpPost]
        public async void Addition()
        {
            DateTime startEmul = DateTime.Now;
            string id = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString();

            string xmlData = "";
            if (Request.Body != null)
            {
                var streamReader = new StreamReader(Request.Body);
                xmlData = await streamReader.ReadToEndAsync();
            }
            if (bool.Parse(Configuration["LogIncomming"]))
            {
                string path = @"C:\log\Calculator";
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }

                using (FileStream stream = new FileStream($"{path}\\" + id + "_request.xml", FileMode.OpenOrCreate))
                {
                    byte[] array = Encoding.Default.GetBytes(xmlData);
                    stream.Write(array, 0, array.Length);
                }
            }

            string emulAction = xmlData.Substring(xmlData.IndexOf("<tem:") + 5, xmlData.IndexOf("<tem:requestModel>") - xmlData.IndexOf("<tem:") - 17);
            
            string response = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + 
                                "<s:Envelope xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">" + 
                                    "<s:Body>" + 
                                        "<" + emulAction + "Response xmlns=\"http://tempuri.org/\">" + 
                                            "<" + emulAction + "Result xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" + 
                                                "<Result>10</Result>" + 
                                            "</" + emulAction + "Result>" + 
                                        "</" + emulAction + "Response>" + 
                                    "</s:Body>" + 
                                "</s:Envelope>";
            
            if (emulAction == "Addition")
            {
                Task<HttpWebRequest> task = Task.Run<HttpWebRequest>(async () => await ResponceAsync(xmlData));
            }

            if (bool.Parse(Configuration["LogIncomming"]))
            {
                string path = @"C:\log\Calculator";
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }

                using (FileStream stream = new FileStream($"{path}\\" + id + "_response.xml", FileMode.OpenOrCreate))
                {
                    byte[] array = Encoding.Default.GetBytes(response);
                    stream.Write(array, 0, array.Length);
                }
            }

            //Response.StatusCode = 202;
            Response.ContentType = "text/xml";
            System.Threading.Thread.Sleep(int.Parse(Configuration["TimeoutAddition"]));

            if (bool.Parse(Configuration["LogIncomming"]))
            {
                DateTime dateTime = DateTime.Now;
                using (StreamWriter writer = System.IO.File.AppendText(@"C:\log\Calculator\Timeout.csv"))
                {
                    writer.WriteLine(dateTime.ToString("yyyy.MM.dd HH:mm:ss,fff") + ";" + dateTime.Subtract(startEmul).TotalMilliseconds.ToString());
                }
            }

            await Response.WriteAsync(response);
        }

        public async Task<HttpWebRequest> ResponceAsync(string xmlData)
        {
            DateTime startEmul = DateTime.Now;
            string id = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString();

            if (bool.Parse(Configuration["LogIncomming"]))
            {
                string path = @"C:\log\Calculator";
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }

                using (FileStream stream = new FileStream($"{path}\\" + id + "_request.xml", FileMode.OpenOrCreate))
                {
                    byte[] array = Encoding.Default.GetBytes(xmlData);
                    stream.Write(array, 0, array.Length);
                }
            }

            // Эндпоинт ответа
            string uri = "https://185.233.2.14/Calculator.asmx";

            string response = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" + 
                                "<soapenv:Header/>" + 
                                "<soapenv:Body>" + 
                                    "<tem:Subtraction>" + 
                                        "<tem:requestModel>" + 
                                            "<tem:intA>2</tem:intA>" + 
                                            "<tem:intB>1</tem:intB>" + 
                                        "</tem:requestModel>" + 
                                    "</tem:Subtraction>" + 
                                "</soapenv:Body>" + 
                                "</soapenv:Envelope>";


            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = "text/xml;charset=utf-8";
            request.Headers["SOAPAction"] = "\"http://tempuri.org/ICalculatorService/Subtraction\"";
            byte[] reqContent = Encoding.UTF8.GetBytes(response);
            request.ContentLength = reqContent.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(reqContent, 0, reqContent.Length);
            }

            if (bool.Parse(Configuration["LogIncomming"]))
            {
                string path = @"C:\log\Calculator";
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }

                using (FileStream stream = new FileStream($"{path}\\" + id + "_response.xml", FileMode.OpenOrCreate))
                {
                    byte[] array = Encoding.Default.GetBytes(response);
                    stream.Write(array, 0, array.Length);
                }
            }

            System.Threading.Thread.Sleep(int.Parse(Configuration["TimeoutAddition"]));

            if (bool.Parse(Configuration["LogIncomming"]))
            {
                DateTime dateTime = DateTime.Now;
                using (StreamWriter writer = System.IO.File.AppendText(@"C:\log\Calculator\Timeout.csv"))
                {
                    writer.WriteLine(dateTime.ToString("yyyy.MM.dd HH:mm:ss,fff") + ";" + dateTime.Subtract(startEmul).TotalMilliseconds.ToString());
                }
            }

            return await Task.FromResult(request);
        }
    }
}