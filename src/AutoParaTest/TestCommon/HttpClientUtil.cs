using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace TestCommon
{
    public static class HttpClientUtil
    {

        /// <summary>
        /// 获取要提交的信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static MultipartFormDataContent GetPostModelData<T>(T model)
        {
            var modelType = typeof(T);
            var formData = new MultipartFormDataContent();

            //遍历SendData的所有成员
            foreach (var item in modelType.GetProperties())
            {
                if (item.GetValue(model) == null)
                {
                    continue;

                }
                var content = new StringContent(item.GetValue(model).ToString());

                formData.Add(content, item.Name);

            }

            return formData;

        }
        /// <summary>
        /// 获取要提交的信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static MultipartFormDataContent GetPostData(Dictionary<string, string> paraList)
        {
            var formData = new MultipartFormDataContent();

            //遍历SendData的所有成员
            foreach (var item in paraList)
            {
                var content = new StringContent(item.Value);
                formData.Add(content, item.Key);

            }
            return formData;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <param name="model"></param>
        /// <param name="isJson">是否使用json的方式提交（api用了参数[FromBody]修饰后，必须用此方法方可提交成功）</param>
        /// <returns></returns>
        public static string PostData<T>(this HttpClient client, string url, T model,bool isJson=false) where T : class
        {

            client.InitClient();

            MultipartFormDataContent postModelData;
            if (typeof(T) == typeof(Dictionary<string, string>))
            {
                postModelData = GetPostData(model as Dictionary<string, string>);
            }
            else
            {
                postModelData = GetPostModelData<T>(model);
            }
            if(isJson)
            {
                HttpContent httpContent = new StringContent(model.ToJson());
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var responseJson = client.PostAsync(url, httpContent).Result;
                return responseJson.Content.ReadAsStringAsync().Result;
            }

            var response = client.PostAsync(url, postModelData).Result;
            return response.Content.ReadAsStringAsync().Result;

        }



        public static string GetData(this HttpClient client, string url, Dictionary<string, string> formData = null)
        {

            client.InitClient();
            if (formData == null || formData.Count <= 0)
            {
                var resu = client.GetAsync(url).Result;
                return resu.Content.ReadAsStringAsync().Result;
            }

            var paraData = formData.GetQueryString();
            var questionmark = "?" + paraData;


            var response = client.GetAsync(url + questionmark).Result;
            var restr = response.Content.ReadAsStringAsync().Result;
            if (string.IsNullOrEmpty(restr))
            {
                var eeee = 11111;
            }
            return restr;

        }


        public static string PutData<T>(this HttpClient client, string url, T model)
        {

            client.InitClient();

            var postModelData = HttpClientUtil.GetPostModelData<T>(model);
            var response = client.PutAsync(url, postModelData).Result;
            return response.Content.ReadAsStringAsync().Result;

        }


        public static string DeleteData(this HttpClient client, string url, Dictionary<string, string> formData)
        {

            client.InitClient();

            if (formData == null || formData.Count <= 0)
            {
                return client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
            }

            var paraData = formData.GetQueryString();

            var response = client.DeleteAsync(url + "?" + paraData).Result;
            return response.Content.ReadAsStringAsync().Result;

        }

        private static void InitClient(this HttpClient client)
        {
            client.MaxResponseContentBufferSize = int.MaxValue;
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36.0.1985.143 Safari/537.36");

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// 组装QueryString的方法
        /// 参数之间用&连接，首位没有符号，如：a=1&b=2&c=3
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        public static string GetQueryString(this Dictionary<string, string> formData)
        {
            if (formData == null || formData.Count == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();

            var i = 0;
            foreach (var kv in formData)
            {
                i++;
                sb.AppendFormat("{0}={1}", kv.Key, kv.Value);
                if (i < formData.Count)
                {
                    sb.Append("&");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 实体转键值对
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Dictionary<string, string> EntityToDictionary<T>(this T obj) where T : class
        {
            //初始化定义一个键值对，注意最后的括号
            Dictionary<string, string> dic = new Dictionary<string, string>();
            //返回当前 Type 的所有公共属性Property集合
            PropertyInfo[] props = typeof(T).GetProperties();
            foreach (PropertyInfo p in props)
            {
                var property = obj.GetType().GetProperty(p.Name);//获取property对象
                var value = p.GetValue(obj);//获取属性值
                dic.Add(p.Name, value.ToString());
            }
            return dic;
        }





    }
}
