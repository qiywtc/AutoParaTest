using System;
using System.Collections.Generic;
using System.Text;

namespace AutoReadApiData.Models
{
    /// <summary>
    /// 参数信息
    /// </summary>
    public class ParaData
    {

        /// <summary>
        /// 参数名
        /// </summary>
        public string ParaName { get; set; }

        /// <summary>
        /// 参数注释
        /// </summary>
        public string ParaAnnotation { get; set; }


        /// <summary>
        /// 参数类型名
        /// </summary>
        public string ParaTypeName { get; set; }


        /// <summary>
        /// 这是第几个参数（从0开始）
        /// </summary>
        public int ParaPosition { get; set; }


        #region 管理字段

        /// <summary>
        /// 参数子集（如List中的string等类型）
        /// </summary>
        public List<ParaData> SubseriesList { get; set; }

        #endregion
    }
}
