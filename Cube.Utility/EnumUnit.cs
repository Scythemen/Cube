using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

namespace Cube.Utility
{
    /// <summary>
    /// 枚举操作方法集合
    /// </summary>
    public class EnumUnit
    {
        /// <summary>
        /// 获得枚举所有信息
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        /// <returns>返回所有描述信息</returns>
        public static IList<EnumInfo> GetEnumInfos(Type enumType)
        {
            IList<EnumInfo> lstInfos = new List<EnumInfo>();
            FieldInfo[] fields = enumType.GetFields();
            foreach (FieldInfo field in fields)
            {
                object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (objs != null && objs.Length > 0)
                {
                    DescriptionAttribute da = objs[0] as DescriptionAttribute;
                    EnumInfo info = new EnumInfo
                    {
                        Description = da.Description,
                        FieldName = field.Name,
                        Name = Enum.Parse(enumType, field.Name),
                        Value = (int)(Enum.Parse(enumType, field.Name))
                    };
                    lstInfos.Add(info);
                }
            }
            return lstInfos;
        }

        /// <summary>
        /// 返回此种结构类型信息
        /// </summary>
        /// <param name="enumName">枚举类型的名称</param>
        /// <returns>返回此种结构类型信息</returns>
        public static EnumInfo GetEnumInfo(Enum enumName)
        {
            EnumInfo info = new EnumInfo();
            Type eType = enumName.GetType();
            info.Name = enumName;
            info.FieldName = Enum.GetName(eType, enumName);//获取值对应的名称
            FieldInfo field = eType.GetField(info.FieldName);//获取名称对应的信息
            if (field != null)
            {
                object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (objs != null && objs.Length > 0)
                {
                    DescriptionAttribute da = objs[0] as DescriptionAttribute;
                    info.Description = da.Description;
                    info.FieldName = field.Name;
                    info.Name = enumName;
                    return info;
                }
            }
            return info;
        }

        /// <summary>
        /// 返回此种结构类型信息
        /// </summary>
        /// <param name="enumType">typeof ( 枚举 )</param>
        /// <param name="name">枚举类型的名称</param>
        /// <returns>返回此种结构类型信息</returns>
        public static EnumInfo GetEnumInfo(Type enumType, string name)
        {
            FieldInfo field = enumType.GetField(name);//获取名称对应的信息
            if (field != null)
            {
                object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (objs != null && objs.Length > 0)
                {
                    DescriptionAttribute da = objs[0] as DescriptionAttribute;
                    EnumInfo info = new EnumInfo();
                    info.Description = da.Description;
                    info.FieldName = field.Name;
                    info.Name = Enum.Parse(enumType, field.Name);
                    info.Value = (int)Enum.Parse(enumType, field.Name);
                    return info;
                }
            }
            return null;
        }

        /// <summary>
        /// 返回此种结构类型信息
        /// </summary>
        /// <param name="enumType">typeof ( 枚举 )</param>
        /// <param name="value">枚举类型的值</param>
        /// <returns>返回此种结构类型信息</returns>
        public static EnumInfo GetEnumInfo(Type enumType, object value)
        {
            string name = Enum.GetName(enumType, value);
            return GetEnumInfo(enumType, name);
        }

        /// <summary>
        /// 返回此种结构类型的说明
        /// </summary>
        /// <param name="enumType">typeof ( 枚举 )</param>
        /// <param name="value">枚举类型的值，10进制或16进制</param>
        /// <returns>类型的说明</returns>
        public static string GetEnumDescription(Type enumType, object value)
        {
            string name = Enum.GetName(enumType, value);
            FieldInfo field = enumType.GetField(name);//获取名称对应的信息
            if (field != null)
            {
                object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (objs != null && objs.Length > 0)
                {
                    DescriptionAttribute da = objs[0] as DescriptionAttribute;
                    return da.Description;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 返回此种结构类型的说明
        /// </summary>
        /// <param name="enumType">typeof ( 枚举 )</param>
        /// <param name="name">名称</param>
        /// <returns>类型的说明</returns>
        public static string GetEnumDescription(Type enumType, string name)
        {
            FieldInfo field = enumType.GetField(name);//获取名称对应的信息
            if (field != null)
            {
                object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (objs != null && objs.Length > 0)
                {
                    DescriptionAttribute da = objs[0] as DescriptionAttribute;
                    return da.Description;
                }
            }
            return null;
        }


        public static IDictionary<int, string> ToDictionary<T>() where T : struct
        {
            var dictionary = new Dictionary<int, string>();
            var values = Enum.GetValues(typeof(T));
            foreach (var value in values)
            {
                int key = (int)value;
                dictionary.Add(key, value.ToString());
            }
            return dictionary;
        }

        public static IDictionary<string, string> ToDictionary2<T>() where T : struct
        {
            var dictionary = new Dictionary<string, string>();
            var values = Enum.GetValues(typeof(T));
            foreach (var value in values)
            {
                int key = (int)value;
                dictionary.Add(key.ToString(), value.ToString());
            }
            return dictionary;
        }

        //枚举注释方式如下
        //[Serializable]
        //public enum EnumAppWizardError
        //{
        //    [Description("未安装SDK！")]
        //    NOT_INSTALL_SDK,

        //    [Description("未安装Visual Studio ")]
        //    NOT_INSTALL_VS,

        //    [Description("编辑文件出错")]
        //    EDIT_FILE_ERROR,

        //    [Description("创建新工程项目出错")]
        //    CREAT_NEW_PROJECT_ERROR,

        //    [Description("删除文件时出错")]
        //    DELETE_FILE_ERROR,

        //}
    }


    /// <summary>
    /// 枚举结构类型的信息
    /// </summary>
    public class EnumInfo
    {
        public string FieldName { get; set; }
        public object Name { get; set; }
        public int Value { get; set; }
        public string Description { get; set; }
    }



}
