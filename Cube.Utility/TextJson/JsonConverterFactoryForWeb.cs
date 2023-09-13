using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cube.Utility.TextJson
{
    public class JsonConverterFactoryForWeb : JsonConverterFactory
    {
        static readonly Dictionary<Type, JsonConverter> dic = new Dictionary<Type, JsonConverter>();

        public JsonConverterFactoryForWeb()
        {
            Init();
        }


        public override bool CanConvert(Type typeToConvert)
        {
            return dic.ContainsKey(typeToConvert);
        }

        public override JsonConverter CreateConverter(
            Type type,
            JsonSerializerOptions options)
        {
            if (dic.ContainsKey(type))
            {
                return dic[type];
            }
            else
            {
                var convertor = (JsonConverter)Activator.CreateInstance(type);
                dic[type] = convertor;
                return convertor;
            }
        }


        static void Init()
        {
            dic.Add(typeof(int), new StringToInt32Converter());
            dic.Add(typeof(int?), new StringToNullableInt32Converter());
            dic.Add(typeof(long), new LongToStringConverter());
            dic.Add(typeof(long?), new NullableLongToStringConverter());
            dic.Add(typeof(decimal), new StringToDecimalConverter());
            dic.Add(typeof(decimal?), new StringToNullableDecimalConverter());
            dic.Add(typeof(bool), new StringToBooleanConverter());
            dic.Add(typeof(bool?), new StringToNullableBooleanConverter());
            dic.Add(typeof(DateTime), new StringToDatetimeConverter());
            dic.Add(typeof(DateTime?), new StringToNullableDatetimeConverter());
            dic.Add(typeof(string), new ObjectToStringConverter());
        }



    }
}
