using System;
using System.Collections.Generic;
using System.Text;

namespace AutoReadApiData.Models
{
    /// <summary>
    /// 方法信息
    /// </summary>
    public class MethodData
    {


        /// <summary>
        /// 方法名
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// 方法注释
        /// </summary>
        public string Annotation { get; set; }

        /// <summary>
        /// 方法返回类型名
        /// </summary>
        public string ResultTypeName { get; set; }

        /// <summary>
        /// 返回值注释
        /// </summary>
        public string ResultAnnotation { get; set; }

        /// <summary>
        /// 方法HTTP请求类型
        /// </summary>
        public HttpState HttpType { get; set; }

        /// <summary>
        /// 参数列表
        /// </summary>
        public List<ParaData> ParaList { get; set; }

        /// <summary>
        /// 路由信息
        /// </summary>
        public string RouteUrl { get; set; }
    }

    public enum HttpState
    {
        Get,
        Post,
        Put,
        Delete,
        Head,
        Options
    }
}
