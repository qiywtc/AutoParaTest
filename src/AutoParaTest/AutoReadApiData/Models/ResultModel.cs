using System;
using System.Collections.Generic;
using System.Text;

namespace AutoReadApiData.Models
{
    /// <summary>
    /// 返回结果状态，-1：失败出错，0：没有结果，1：有结果,-2：入参格式错误
    /// </summary>
    public enum ResultStatus
    {

        /// <summary>
        /// 参数错误
        /// </summary>
        TypeError = -2,
        /// <summary>
        /// 失败出错
        /// </summary>
        Error = -1,

        /// <summary>
        /// 没有结果
        /// </summary>
        NoResult = 0,

        /// <summary>
        /// 有结果
        /// </summary>
        HasResult = 1,


    }

    public class ResultModel
    {


        /// <summary>
		/// 响应结果状态，-1：失败出错，0：没有结果，1：有结果,-2：格式错误
		/// </summary>
		public ResultStatus Status { get; set; }

        /// <summary>
        /// 原因
        /// </summary>
        public string Message { get; set; }
        
    }


    /// <summary>
    /// WebApi响应状态与结果
    /// </summary>
    /// <typeparam name="T">数据T</typeparam>
    public class ResultModel<T> : ResultModel
    {
        /// <summary>
        /// 数据结果
        /// </summary>
        public T Result { get; set; }

    }
}
