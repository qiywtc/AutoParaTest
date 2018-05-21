using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace AutoPara
{
    public class ParaUtil
    {
        /*
         目的：
         自动生成相应类型的随机数，以便用于测试接口。
         方法：
         1.根据单个类型生成随机值。（OK，进一步思考：如果写个万能的类型值随机生成器？）
         2.根据实体类生成对应属性类型的随机值。（半OK，进一步思考：如果根据需要生成某些有效的数据以进行下一步的测试）

             
             */


        #region 随机获取常用类型值


        static Random rd = new Random();

        /// <summary>
        /// 随机int
        /// </summary>
        /// <returns></returns>
        public static int GetInt(int max)
        {
            return rd.Next(0, max);
        }

        /// <summary>
        /// 随机int
        /// </summary>
        /// <returns></returns>
        public static int GetInt(int min = int.MinValue, int max = int.MaxValue)
        {
            return rd.Next(min, max);
        }

        /// <summary>
        /// 随机long
        /// </summary>
        /// <returns></returns>
        public static long GetLong(long min = long.MinValue, long max = long.MaxValue)
        {
            if (long.TryParse(GetDouble((double)min, (double)max).ToString(), out long longValue))
            {
                ///成功
                return longValue;
            }
            //失败
            return GetInt();

        }

        /// <summary>
        /// 随机float
        /// </summary>
        /// <returns></returns>
        public static float GeFloat(float min = float.MinValue, float max = float.MaxValue)
        {
            if (float.TryParse(GetDouble((double)min, (double)max).ToString(), out float outValue))
            {
                ///成功
                return outValue;
            }
            //失败
            return GetInt();

        }



        /// <summary>
        /// 随机Double
        /// </summary>
        /// <returns></returns>
        public static double GetDouble(double min = double.MinValue, double max = double.MaxValue)
        {
            if (rd != null)
            {
                return rd.NextDouble() * (max - min) + min;
            }
            else
            {
                return 0.0d;
            }
        }

        /// <summary>
        /// 随机string
        /// </summary>
        /// <returns></returns>
        public static string GetString()
        {
            return GetString(GetInt(0, 1000), GetBoolean(), GetInt());
        }
        /// <summary>
        /// 随机string
        /// </summary>
        /// <returns></returns>
        public static string GetString(int maxSize)
        {
            var size = GetInt(0, maxSize);
            return GetString(size, GetBoolean(), GetInt());
        }
        /// <summary>
        /// 随机string
        /// </summary>
        /// <returns></returns>
        public static string GetString(int size, bool isNum = false, int isLower = -1)
        {
            StringBuilder builder = new StringBuilder();
            char ch = '0';

            for (int i = 0; i < size; i++)
            {
                if (isNum)
                {
                    ch = Convert.ToChar(Convert.ToInt32(9 * rd.NextDouble() + 48));
                }
                else
                {
                    if (isLower < 0)
                    {
                        int index = Convert.ToInt32(size * rd.NextDouble()) % 2 == 0 ? 65 : 97;
                        ch = Convert.ToChar(Convert.ToInt32(25 * rd.NextDouble() + index));
                    }
                    else if (isLower == 0)
                    {
                        ch = Convert.ToChar(Convert.ToInt32(25 * rd.NextDouble() + 65));
                    }
                    else if (isLower > 0)
                    {
                        ch = Convert.ToChar(Convert.ToInt32(25 * rd.NextDouble() + 97));
                    }
                }
                builder.Append(ch);
            }

            return builder.ToString();
        }

        /// <summary>
        /// 随机datetime
        /// </summary>
        /// <returns></returns>
        public static DateTime GetDateTime()
        {
            return GetDate(DateTime.MinValue, DateTime.MaxValue);
        }
        /// <summary>
        /// 随机datetime
        /// </summary>
        /// <returns></returns>
        public static DateTime GetDate(DateTime minDate, DateTime maxDate)
        {
            int totalDays = (int)((TimeSpan)maxDate.Subtract(minDate)).TotalDays;
            int randomDays = rd.Next(0, totalDays);
            return minDate.AddDays(randomDays);
        }

        /// <summary>
        /// 随机bool
        /// </summary>
        /// <returns></returns>
        public static bool GetBoolean()
        {
            return (GetInt(0, 2) == 0);
        }

        /// <summary>
        /// 随机char
        /// </summary>
        /// <returns></returns>
        public static char GetChar()
        {
            return Convert.ToChar(Convert.ToInt32(26 * rd.NextDouble() + 64));
        }

        /// <summary>
        /// 随机byte
        /// </summary>
        /// <returns></returns>
        public static byte GetByte()
        {
            return GetByte(0, byte.MaxValue);
        }
        /// <summary>
        /// 随机byte
        /// </summary>
        /// <returns></returns>
        public static byte GetByte(byte min, byte max)
        {
            return (byte)GetInt((int)min, (int)max);
        }

        /// <summary>
        /// 随机shrot
        /// </summary>
        /// <returns></returns>
        public static short GetShort()
        {
            return GetShort(0, short.MaxValue);
        }
        /// <summary>
        /// 随机short
        /// </summary>
        /// <returns></returns>
        public static short GetShort(short min, short max)
        {
            return (short)GetInt((int)min, (int)max);
        }


        #endregion


        #region 如果是实体类型

        public static T GetModelData<T>(bool isGetOkData = false) where T : new()
        {
            var model = new T();
            var t = typeof(T);


            PropertyInfo[] properties = t.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                Type oneType = property.PropertyType;

                if (IsNullableType(oneType))
                {
                    if (!GetBoolean()&& !isGetOkData)
                    {
                        //对于可空类型，随机给空值
                        continue;
                    }
                    oneType = property.PropertyType.GetGenericArguments()[0];
                }
                if (isGetOkData)
                {//要求要有效的数据

                    var paraValue = GetValueByAttribute(property);
                    if (paraValue != null)
                    {

                        property.SetValue(model, paraValue);
                        continue;
                    }
                }
                if (!oneType.IsValueType&&!isGetOkData)
                {
                    if (!GetBoolean())
                    {
                        //如果是引用类型，又不要求一定要合理的值，则可以为空
                        continue;
                    }
                }
                property.SetValue(model, GetValueByType(oneType, 0));

            }
            return model;
        }


        /// <summary>
        /// 根据特性来生成对应的值
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private static object GetValueByAttribute(PropertyInfo property)
        {
            var allAttribute = property.GetCustomAttributes(true);
            if (allAttribute == null)
            {
                return null;
            }
            //必需的
            var isRequir = false;
            //长度限制特性
            StringLengthAttribute stringLengthAttribute = null;

            foreach (var item in allAttribute)
            {
                if (item is StringLengthAttribute)
                {
                    var slAttribute = item as StringLengthAttribute;
                    stringLengthAttribute = slAttribute;

                }
                if (item is RequiredAttribute)
                {
                    isRequir = true;
                }
            }

            bool isok = false;
            object paraVal = null;
            if (stringLengthAttribute == null)
            {
                if (isRequir)
                {//没有限定最大长度，且不能为空
                    var type = property.PropertyType;
                    if (IsNullableType(type))
                    {
                        type = property.PropertyType.GetGenericArguments()[0];
                    }
                    while (!isok)
                    {
                        paraVal = GetValueByType(type);
                        isok = paraVal != null;
                    }
                    return paraVal;
                }

                return GetValueByType(property.PropertyType);
            }

            while (!isok)
            {
                string paraValStr = GetString(stringLengthAttribute.MaximumLength);
                isok = ValidValue(stringLengthAttribute, paraValStr as string);
                if (isRequir)
                {
                    if (string.IsNullOrEmpty(paraValStr))
                    {
                        isok = false;
                    }
                }
                if (isok)
                {
                    paraVal = paraValStr;
                }
            }
            return paraVal;
        }

        /// <summary>
        /// 验证将要给的值是否合格
        /// </summary>
        /// <param name="slAttribute"></param>
        /// <param name="paraVal"></param>
        /// <returns></returns>
        private static bool ValidValue(StringLengthAttribute slAttribute, string paraVal)
        {
            if (slAttribute.IsValid(paraVal))
            {
                return true;

            }
            return false;
        }

        private static bool IsNullableType(Type theType)
        {
            return (theType.IsGenericType && theType.
              GetGenericTypeDefinition().Equals
              (typeof(Nullable<>)));
        }

        private static object GetValueByType(Type paraInType, int maxSize = 0)
        {
            Type paraType = paraInType;
            if (IsNullableType(paraInType))
            {
                if (GetBoolean())
                {
                    return null;
                }
                paraType = paraInType.GetGenericArguments()[0];
            }

            if (paraType == typeof(string))
            {
                if (maxSize > 0)
                {
                    return GetString(maxSize);
                }
                return GetString();
            }
            if (paraType == typeof(Int16))
            {
                if (maxSize > 0)
                {
                    return GetInt(maxSize);
                }
                return GetInt(Int16.MinValue, Int16.MaxValue);
            }
            if (paraType == typeof(int))
            {
                return GetInt();
            }
            if (paraType == typeof(Int32))
            {
                return GetInt();
            }
            if (paraType == typeof(Int64))
            {
                return GetLong();
            }
            if (paraType == typeof(long))
            {
                return GetLong();
            }
            if (paraType == typeof(float))
            {
                return GeFloat();
            }
            if (paraType == typeof(double))
            {
                return GetDouble();
            }
            if (paraType == typeof(bool))
            {
                return GetBoolean();
            }
            if (paraType == typeof(DateTime))
            {
                return GetDateTime();
            }

            return null;
        }



        /// <summary>
        /// 验证实体中的StringLength特性能否够
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns>false为不通过，true为通过</returns>
        public static bool IsValidate<T>(T model)
        {
            var t = typeof(T);
            PropertyInfo[] properties = t.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                //object[] Attribute2 = p.GetCustomAttributes(typeof(MarkAttribute), false);
                var allAttribute = property.GetCustomAttributes(true);
                if (allAttribute == null)
                {
                    continue;
                }
                foreach (var item in allAttribute)
                {
                    if (item is StringLengthAttribute)
                    {
                        var slAttribute = item as StringLengthAttribute;
                        var validateVal = property.GetValue(model, null);

                        if (!slAttribute.IsValid(validateVal))
                        {
                            return false;

                        }
                    }
                }

            }

            return true;

        }



        #endregion

    }
}
