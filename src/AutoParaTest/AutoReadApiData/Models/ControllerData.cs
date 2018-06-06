using System;
using System.Collections.Generic;
using System.Text;

namespace AutoReadApiData.Models
{
    /// <summary>
    /// 控制器信息
    /// </summary>
    public class ControllerData
    {

        /// <summary>
        /// 控制器名
        /// </summary>
        public string ControllerName { get; set; }

        /// <summary>
        /// 控制器注释
        /// </summary>
        public string ControllerAnnotation { get; set; }


        /// <summary>
        /// 方法列表
        /// </summary>
        public List<MethodData> MethodList { get; set; }

    }
}
