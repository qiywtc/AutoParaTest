

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace TestCommon
{
    public class TestProgram
    {


        private readonly TestServer _server;
        private HttpClient _client;


        public TestProgram(IWebHostBuilder builder)
        {
            _server = new TestServer(builder);
            _client = _server.CreateClient();

        }

        public HttpClient Client
        {
            get { return _client; }
        }



        /// <summary>
        /// 刷新Client信息
        /// </summary>
        /// <param name="userId"></param>
        public void RefreshClient(string userId = null)
        {

            _client = _server.CreateClient();
            
        }

    }
}
