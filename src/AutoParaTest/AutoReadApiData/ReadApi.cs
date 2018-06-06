using AutoReadApiData.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace AutoReadApiData
{
    public class ReadApi
    {
        public static ResultModel GetApiData<T>()
        {

            //获取传入类型所在程序集下，所有定义的类
            return GetApiData(typeof(T).Assembly.GetTypes());
        }

        public static ResultModel GetApiData(Type[] types)
        {
            var result = new ResultModel();

            //获取本程序集中所有定义的类  
            //Type[] types = Assembly.GetExecutingAssembly().GetTypes();

            //遍历类  
            for (int i = 0; i < types.Length; i++)
            {
                //找到所有接口类
                if (types[i].Name.IndexOf("Controller") < 0)
                {
                    continue;
                }


                if (ApiDataModel.ControllerList == null)
                {
                    ApiDataModel.ControllerList = new List<ControllerData>();
                }


                var oneControllerData = new ControllerData();

                oneControllerData.ControllerName = types[i].Name;


                GetMethodList(types[i], oneControllerData);

                if (oneControllerData.MethodList == null || oneControllerData.MethodList.Count <= 0)
                {
                    continue;
                }
                if (oneControllerData.MethodList.Count <= 0)
                {
                    continue;
                }
                GetNotCompileClass(types[i], oneControllerData);

                ApiDataModel.ControllerList.Add(oneControllerData);

            }


            result.Status = ResultStatus.HasResult;
            result.Message = "已成功执行完毕";
            return result;
        }

        /// <summary>
        /// 获取方法列表
        /// </summary>
        /// <param name="controllerType">当前要识别的控制器类型</param>
        /// <param name="oneControllerData">当前正在操作的控制器实体</param>
        private static void GetMethodList(Type controllerType, ControllerData oneControllerData)
        {
            //获取类下的所有方法  (排除了非本类的方法)
            MethodInfo[] methods = controllerType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly);

            if (methods.Length <= 0)
            {
                return;
            }

            oneControllerData.MethodList = new List<MethodData>();



            //遍历类下的方法  
            for (int j = 0; j < methods.Length; j++)
            {
                //获取是否为静态方法
                var isStatic = methods[j].IsStatic;

                if (isStatic)
                {
                    //接口不可能是静态方法，如果是静态方法则跳过
                    continue;
                }

                var oneMethod = new MethodData();
                oneMethod.MethodName = methods[j].Name;
                oneMethod.ResultTypeName = methods[j].ReturnType.Name;


                oneMethod.ParaList = new List<ParaData>();
                GetParaList(oneMethod, methods[j]);

                oneMethod.HttpType = HttpState.Get;


                var attributes = methods[j].GetCustomAttributes();
                if (attributes != null)
                {
                    foreach (var oneAttri in attributes)
                    {
                        if (oneAttri == null)
                        {
                            continue;
                        }


                        if (oneAttri is RouteAttribute)
                        {
                            var oneRoute = oneAttri as RouteAttribute;
                            oneMethod.RouteUrl = oneRoute.Template;
                            continue;
                        }
                        if (oneAttri is HttpGetAttribute)
                        {
                            //oneMethod.HttpType = HttpState.Get;
                            continue;
                        }
                        if (oneAttri is HttpPutAttribute)
                        {
                            oneMethod.HttpType = HttpState.Put;
                            continue;
                        }
                        if (oneAttri is HttpPostAttribute)
                        {
                            oneMethod.HttpType = HttpState.Post;
                            continue;
                        }
                        if (oneAttri is HttpDeleteAttribute)
                        {
                            oneMethod.HttpType = HttpState.Delete;
                            continue;
                        }
                        if (oneAttri is HttpHeadAttribute)
                        {
                            oneMethod.HttpType = HttpState.Head;
                            continue;
                        }
                        if (oneAttri is HttpOptionsAttribute)
                        {
                            oneMethod.HttpType = HttpState.Options;
                            continue;
                        }
                    }
                }

                oneControllerData.MethodList.Add(oneMethod);
            }
        }

        /// <summary>
        /// 获取方法中的参数
        /// </summary>
        /// <param name="oneMethod">正在操作的方法实体</param>
        /// <param name="methodInfo">方法信息封装类</param>
        private static void GetParaList(MethodData oneMethod, MethodInfo methodInfo)
        {
            //获取方法下的参数  
            ParameterInfo[] parameters = methodInfo.GetParameters();

            if (parameters.Length <= 0)
            {
                return;
            }


            //遍历方法下的参数  
            for (int k = 0; k < parameters.Length; k++)
            {
                var onePara = new ParaData();
                /* 
                 * parameters[k].ParameterType.Namespace
"TestAutoApiDoc.Controllers"
 parameters[k].ParameterType.Name
"UserModel"
*/

                var oneType = parameters[k].ParameterType;
                onePara.ParaTypeName = oneType.Name;
                if ((oneType.IsGenericType && oneType.GetGenericTypeDefinition().Equals(typeof(Nullable<>))))
                {
                    
                    oneType = oneType.GetGenericArguments()[0];
                    onePara.ParaTypeName = oneType.Name+"?";
                }

                onePara.ParaName = parameters[k].Name;
                

                //方法位置
                onePara.ParaPosition = parameters[k].Position;

                GetParaTypeInfo(oneType, onePara);

                oneMethod.ParaList.Add(onePara);
            }
        }
        #region 获取注释


        /// <summary>
        /// 获取未编译文件
        /// </summary>
        /// <param name="controllerType">当前要识别的控制器类型</param>
        /// <param name="oneControllerData">当前正在操作的控制器实体</param>
        private static void GetNotCompileClass(Type controllerType, ControllerData oneControllerData)
        {
            if (controllerType.Assembly.Location == null)
            {
                return;
            }

            var rightPath = GetRightPath(controllerType);
            if (string.IsNullOrEmpty(rightPath))
            {
                return;
            }

            List<string> strList = new List<string>();
            using (FileStream fs = new FileStream(rightPath, FileMode.Open, FileAccess.Read))
            {

                using (StreamReader m_streamReader = new StreamReader(fs))
                {
                    //使用StreamReader类来读取文件
                    m_streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                    //  从数据流中读取每一行，直到文件的最后一行，并在richTextBox1中显示出内容

                    string strLine = m_streamReader.ReadLine();

                    string oneMethodData = string.Empty;

                    while (strLine != null)
                    {
                        if (@"/// <summary>" == strLine.Trim())
                        {
                            oneMethodData = string.Empty;

                            strLine = m_streamReader.ReadLine();
                            oneMethodData += strLine;

                            while (strLine != null && (strLine.IndexOf("///") > -1) || strLine.IndexOf("[") > -1 || strLine.IndexOf("public") > -1)
                            {
                                if (strLine.IndexOf("public") > -1)
                                {
                                    GetMethodAnnotation(strLine, oneMethodData, oneControllerData);

                                    oneMethodData = string.Empty;
                                }

                                strLine = m_streamReader.ReadLine();
                                oneMethodData += strLine;
                            }

                        }

                        strLine = m_streamReader.ReadLine();
                    }
                }
            }

        }

        /// <summary>
        /// 获取正确的文件所有路径
        /// </summary>
        /// <param name="controllerType"></param>
        /// <returns></returns>
        private static string GetRightPath(Type controllerType)
        {

            /*
             一开始用controllerType.Assembly.Location，的IIS Express  下获取到的不是正确的目录，所以改为CodeBase
             */
            //		CodeBase"file:///H:/CollegeLife/CollegeLife.WebApi/bin/CollegeLife.WebApi.DLL"
            var codeBase = controllerType.Assembly.CodeBase;
            codeBase = codeBase.Replace("file:///", "");

            var binNum = codeBase.LastIndexOf("bin");
            if (binNum <= 0)
            {
                return null;
            }

            //获取未编译文件路径
            codeBase = codeBase.Substring(0, binNum);


            var classPath = controllerType.FullName.Substring(controllerType.FullName.IndexOf(".") + 1).Replace(".", "/") + ".cs";
            // = classPath + @"Controllers\" + controllerType.Name + ".cs";
            //先根据命名空间来找到这个类
            var oneFilePath = codeBase + classPath;

            if (File.Exists(oneFilePath))
            {
                return oneFilePath;
            }
            var dllName = controllerType.Module.Name.Replace(".dll", "");
            oneFilePath = oneFilePath.Replace(dllName.Replace(".", "/"), dllName);
            if (File.Exists(oneFilePath))
            {
                return oneFilePath;
            }

            //找不到，就只能靠搜索了
            DirectoryInfo dir = new DirectoryInfo(codeBase);
            foreach (FileInfo file in dir.GetFiles(controllerType.Name + ".cs", SearchOption.AllDirectories))//第二个参数表示搜索包含子目录中的文件；
            {
                /*这里可能改成每个都获取一下，然后再判断里面的命名空间与类名是否对应得上，对得上就说明是这个文件*/
                oneFilePath = file.FullName;//这里是只用搜索到的最后一个文件来使用
                return oneFilePath;
            }
            /*这里应该再加上，返回再上一层目录的操作，因为可能是跟此项目同级的Models层*/

            var lastPath = codeBase.Remove(codeBase.Length - 1).LastIndexOf("/");
            codeBase = codeBase.Remove(lastPath + 1);
            classPath = classPath.Remove(0, classPath.IndexOf("Controllers")-1);

            oneFilePath = codeBase + dllName+classPath;
            if (File.Exists(oneFilePath))
            {
                return oneFilePath;
            }
            oneFilePath = oneFilePath.Replace(dllName.Replace(".", "/"), dllName);
            if (File.Exists(oneFilePath))
            {
                return oneFilePath;
            }
            return null;
        }

        /// <summary>
        /// 获取注释信息
        /// </summary>
        /// <param name="methodNameStr">方法或类名那一行数据</param>
        /// <param name="allAnnotation">所有注释文本信息</param>
        /// <param name="oneControllerData">当前正在操作的控制器实体</param>
        private static void GetMethodAnnotation(string methodNameStr, string allAnnotation, ControllerData oneControllerData)
        {
            if (string.IsNullOrEmpty(methodNameStr))
            {
                return;
            }
            //if (methodNameStr.IndexOf("<") >= 0)
            //{
            //    //webapi没有泛型接口
            //    return;
            //}

            allAnnotation = allAnnotation.Replace(" ", "");

            var methodNameNum = methodNameStr.IndexOf("(");
            if (methodNameNum < 0)
            {
                //没有括号说明不是一个方法
                methodNameNum = methodNameStr.IndexOf("class");
                if (methodNameNum < 0)
                {
                    //还没有，说明既不是方法也不是控制器的注释
                    return;
                }
                var controllerLastNum = allAnnotation.IndexOf("///</summary>");
                var controllerAnnotation = allAnnotation.Substring(0, controllerLastNum);
                controllerAnnotation = controllerAnnotation.Replace("///", "");
                oneControllerData.ControllerAnnotation = controllerAnnotation;

                return;
            }

            var actionName = methodNameStr.Substring(0, methodNameNum).Trim();
            var oneNullNum = actionName.LastIndexOf(" ");
            actionName = actionName.Substring(oneNullNum).Trim();

            //这个方法不存在
            if (!oneControllerData.MethodList.Exists(q => q.MethodName == actionName))
            {
                return;
            }


            GetMethodByStr(methodNameStr, actionName, allAnnotation, methodNameNum, oneControllerData);

        }


        /// <summary>
        /// 在文本内容中找到对应的方法
        /// </summary>
        /// <param name="methodNameStr"></param>
        /// <param name="actionName"></param>
        /// <param name="allAnnotation"></param>
        /// <param name="methodNameNum"></param>
        /// <param name="oneControllerData"></param>
        private static void GetMethodByStr(string methodNameStr, string actionName, string allAnnotation, int methodNameNum, ControllerData oneControllerData)
        {

            //找到这个方法
            var oneMethodList = oneControllerData.MethodList.FindAll(q => q.MethodName == actionName);
            if (oneMethodList == null)
            {
                return;
            }
            if (oneMethodList.Count <= 0)
            {
                return;
            }
            /*分析这个方法的参数有几个，分别是什么*/
            var allParaStr = methodNameStr.Substring(methodNameNum + 1).Trim();
            allParaStr = allParaStr.Replace(")", "").Trim();
            if (string.IsNullOrEmpty(allParaStr))
            {
                for (int i = 0; i < oneMethodList.Count; i++)
                {
                    var oneMethod = oneMethodList[i];
                    if (oneMethod.ParaList == null || oneMethod.ParaList.Count <= 0)
                    {
                        GetParaAnnotation(allAnnotation, oneMethod);
                        return;
                    }
                }
                //执行到这里说明找不到这个方法
                return;

            }

            /*到这里说明有参数，要分析方法对应的参数数据是否正确，不正确则要另外寻找*/
            var paraArray = allParaStr.Split(',');
            for (int i = 0; i < oneMethodList.Count; i++)
            {
                var oneMethod = oneMethodList[i];
                if (oneMethod.ParaList.Count == paraArray.Length)
                {
                    int okParaNum = 0;

                    //参数数目相同，有可能是对应的方法
                    for (int b = 0; b < paraArray.Length; b++)
                    {
                        //因为参数类型与参数名是以空格分离的
                        var oneParaArray = paraArray[b].Trim().Split(' ');
                        if (oneParaArray == null)
                        {
                            continue;
                        }
                        if (oneParaArray.Length <= 1)
                        {
                            continue;
                        }

                        //类型不会因大小写而不同，所以直接都转为小写来比较
                        if (oneMethod.ParaList[b].ParaName.ToLower() == oneParaArray[1].ToLower())
                        {
                            okParaNum++;
                        }
                    }

                    if (okParaNum == paraArray.Length)
                    {
                        //说明就是你了，上吧，参数注释开始
                        GetParaAnnotation(allAnnotation, oneMethod);
                        return;
                    }

                }

            }
        }

        /// <summary>
        /// 获取参数的注释
        /// </summary>
        /// <param name="allAnnotation"></param>
        /// <param name="oneMethod"></param>
        private static void GetParaAnnotation(string allAnnotation, MethodData oneMethod)
        {


            //开始获取方法的注释
            var lastNum = allAnnotation.IndexOf("///</summary>");
            var methodAnnotation = allAnnotation.Substring(0, lastNum);
            methodAnnotation = methodAnnotation.Replace("///", "");

            oneMethod.Annotation = methodAnnotation;


            /*获取方法返回值的注释*/
            var returnIndexBegin = allAnnotation.IndexOf("<returns>");
            returnIndexBegin = returnIndexBegin + "<returns>".Length;
            if (returnIndexBegin > -1)
            {
                var returnIndexEnd = allAnnotation.IndexOf("</returns>");
                if (returnIndexEnd > -1)
                {
                    oneMethod.ResultAnnotation = allAnnotation.Substring(returnIndexBegin, returnIndexEnd - returnIndexBegin);
                }
            }

            /*获取参数的注释*/
            while (allAnnotation.IndexOf("<param") > -1)
            {

                var oneParaNameBeginIndex = allAnnotation.IndexOf("name=\"");
                oneParaNameBeginIndex = oneParaNameBeginIndex + "name=\"".Length;
                var oneParaNameEndIndex = allAnnotation.IndexOf("\">");
                var oneParaName = allAnnotation.Substring(oneParaNameBeginIndex, oneParaNameEndIndex - oneParaNameBeginIndex);


                var oneParaEndIndex = allAnnotation.IndexOf("</param>");

                //判断参数里是否有这个名字的
                if (string.IsNullOrEmpty(oneParaName))
                {
                    allAnnotation = allAnnotation.Substring(0, oneParaEndIndex);
                    continue;
                }
                if (!oneMethod.ParaList.Exists(q => q.ParaName == oneParaName))
                {
                    allAnnotation = allAnnotation.Substring(0, oneParaEndIndex);
                    continue;
                }
                var onePara = oneMethod.ParaList.Find(q => q.ParaName == oneParaName);
                if (oneMethod == null)
                {
                    allAnnotation = allAnnotation.Substring(0, oneParaEndIndex);
                    return;
                }
                var oneParaBeginIndex = allAnnotation.IndexOf("\">");
                oneParaBeginIndex = oneParaBeginIndex + "\">".Length;

                var oneParaAnnotation = allAnnotation.Substring(oneParaBeginIndex, oneParaEndIndex - oneParaBeginIndex);
                onePara.ParaAnnotation = oneParaAnnotation;

                allAnnotation = allAnnotation.Substring(oneParaEndIndex + 8);
            }

        }


        #endregion

        #region 解析实体类

        /// <summary>
        /// 获取此类型下包含的其它类型的详细信息
        /// </summary>
        /// <param name="paraType">要获取此类型下包含的其它类型的信息的类型</param>
        /// <param name="paraData">已保存当前类型信息的参数实体</param>
        private static void GetParaTypeInfo(Type paraType, ParaData paraData)
        {
            /*
             1.首先，判断参数类型是否为泛型

             */
            if (!paraType.IsGenericType)
            {
                //不是泛型
                if (!IsUserType(paraType))
                {
                    return;
                }

                /*以下为其它类型，有可能是自定义实体，有可能是第三方*/

                GetOneTypeParaInfo(paraType, paraData);
                return;

            }
            /*以下就都是泛型的处理了*/

            /*2.判断此类型是否为用户自定义类型*/

            if (IsUserType(paraType))
            {
                //是用户定义的类型
                GetTypeAllProperties(paraType, paraData);


            }

        }

        /// <summary>
        /// 获取类中所有属性
        /// </summary>
        /// <param name="paraType"></param>
        private static void GetTypeAllProperties(Type paraType, ParaData paraData)
        {
            PropertyInfo[] properties = paraType.GetProperties();
            if (properties.Length <= 0)
            {
                return;
            }
            paraData.SubseriesList = new List<ParaData>();


            //遍历方法下的参数  
            for (int k = 0; k < properties.Length; k++)
            {
                var item = properties[k];

                var onePara = new ParaData();

                //方法位置
                onePara.ParaPosition = k;
                onePara.ParaTypeName = item.PropertyType.Name;
                onePara.ParaName = item.Name;

                GetParaTypeInfo(item.PropertyType, onePara);

                paraData.SubseriesList.Add(onePara);
            }
        }


        /// <summary>
        /// 判断是否是用户定义的类型
        /// </summary>
        /// <param name="paraType"></param>
        /// <returns></returns>
        private static bool IsUserType(Type paraType)
        {
            if (paraType.IsPrimitive)//是否原生类型
            {
                /*注： String DateTime decimal 都不是原生类型*/
                return false;
            }
            if (paraType.FullName.Contains("System.") || paraType.FullName.Contains("Microsoft."))//微软系类型
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// 获取一个实体类中所有字段信息
        /// </summary>
        /// <param name="paraType">实体类类型</param>
        /// <param name="paraData"></param>
        private static void GetOneTypeParaInfo(Type paraType, ParaData paraData)
        {
            PropertyInfo[] PropertyList = paraType.GetProperties();
            if (PropertyList == null || PropertyList.Length <= 0)
            {
                return;
            }
            paraData.SubseriesList = new List<ParaData>();

            for (int i = 0; i < PropertyList.Length; i++)
            {
                var item = PropertyList[i];
                var onePara = new ParaData();

                onePara.ParaName = item.Name;
                var oneType = item.PropertyType;
                onePara.ParaTypeName = oneType.Name;
                if ((oneType.IsGenericType && oneType.GetGenericTypeDefinition().Equals(typeof(Nullable<>))))
                {

                    oneType = oneType.GetGenericArguments()[0];
                    onePara.ParaTypeName = oneType.Name + "?";
                }


                onePara.ParaPosition = i;

                GetParaTypeInfo(oneType, onePara);

                /*这里应该再加上，去那个类文件，找到对应的类注释的方法*/
                paraData.SubseriesList.Add(onePara);
            }

        }



        #endregion


    }
}
