using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TestCommon;

namespace TestCommon
{
    public class TestStartup<T>
    {
        public IConfiguration Configuration { get; }

        private T startup;

        public TestStartup(IConfiguration configuration)
        {
            Configuration = configuration;


            startup = (T)System.Activator.CreateInstance(typeof(T), Configuration);
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.TryAddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, HttpContextAccessor>();



            MethodInfo doWorkMethod = typeof(T).GetMethod("ConfigureServices");
            doWorkMethod.Invoke(startup, new object[] { services });


        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IServiceProvider svp)
        {
            MyHttpContext.ServiceProvider = svp;



            MethodInfo doWorkMethod = typeof(T).GetMethod("Configure");
            var paras = doWorkMethod.GetParameters();
            object[] objPara = null;
            if (paras.Length > 0)
            {
                var list = new List<object>();
                foreach (var item in paras)
                {
                    switch (item.ParameterType.Name)
                    {
                        case "IApplicationBuilder":
                            list.Add(app);
                            break;
                        case "IHostingEnvironment":
                            list.Add(env);
                            break;
                        case "ILoggerFactory":
                            list.Add(loggerFactory);
                            break;
                        case "IServiceProvider":
                            list.Add(svp);
                            break;
                    }
                }
                objPara = list.ToArray();

            }
            var paraList = new List<object>();


            doWorkMethod.Invoke(startup, objPara);
        }

    }
}
