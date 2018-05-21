using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace AutoPara
{
    public class TestProgram
    {
        private static TestServer _server;
        
        public TestProgram()
        {
            _server = new TestServer(WebHost.CreateDefaultBuilder()
                .UseUrls("http://+:80")
                .UseStartup<Startup>());


        }

        /// <summary>
        /// 获取httpClient以进行请求操作
        /// </summary>
        /// <returns></returns>
        public static HttpClient GetClient()
        {
            return _server.CreateClient();

        }

    }
}
