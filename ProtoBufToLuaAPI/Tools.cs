using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoBufToLuaAPI
{
    public static class Tools
    {
        public static string GetClassName(string str)
        {
            string type = MessageStruct.type;
            int index = str.IndexOf(type);
            if (index < 0)
            {
                type = EnumStruct.type;
                index = str.IndexOf(type);
            }

            if (index < 0)
                return string.Empty;

            int strLen = type.Length;
            string className = str.Substring(str.IndexOf(type) + type.Length + 1);

            if (className.IndexOf("{") > 0)
                className = className.Substring(0, className.IndexOf("{"));

            if (className.IndexOf("//") > 0)
                className = className.Substring(0, className.IndexOf("//"));

            return className;
        }
        /// <summary>
        /// 这一行字符串是否在指定结构
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool StringIsStruct(string str)
        {
            if (str.Contains("=") || str.Contains("//") || str.Contains("string") || str.Contains("int"))
                return false;

            var isMessage = str.Contains(MessageStruct.type);
            var isEnum = str.Contains(EnumStruct.type);
            return isMessage || isEnum;
        }

    }
    class MessageStruct
    {
        public class MessageData
        {
            public string name;//字段名
            public string type;//字段类型
            public string comments;//字段注释
            public bool isArry;//是否是数组
        }
        public static string type = "message";
        public string name { get; set; }
        public string comments { get; set; }
        public List<MessageData> fields { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="structData">数据结构</param>
        /// <param name="comments">这个类的注释</param>
        public MessageStruct(List<string> structData,string comments)
        {
            this.name = Tools.GetClassName(structData[0]);
            if (string.IsNullOrEmpty(comments) == false)
                this.comments = comments.Replace("//", "---");

            this.fields = new List<MessageData>();

            this.ReadField(structData);
        }
        /// <summary>
        /// 添加注释
        /// </summary>
        /// <param name="name">字段名字</param>
        /// <param name="data">值</param>
        public void AddCommeents(string name, string data)
        {
            var field = this.GetFiled(name);
            field.comments = data;
        }
        /// <summary>
        /// 添加字段类型
        /// </summary>
        /// <param name="name">字段名</param>
        /// <param name="type">字段类型</param>
        public void AddFieldType(string name, string type)
        {
            var field = this.GetFiled(name);
            field.type = type;
        }
        private void SetIsArry(string name, string line)
        {
            var field = this.GetFiled(name);

            if (line.Contains("repeated"))
                field.isArry = true;
            else
                field.isArry = false;
        }
        private MessageData GetFiled(string name)
        {
            foreach (MessageData data in this.fields)
            {
                if (data.name == name)
                    return data;
            }

            var field = new MessageData();
            field.name = name;
            this.fields.Add(field);
            return field;
        }
        private void ReadField(List<string> structData)
        {
            for (int i = 0; i < structData.Count; ++i)
                this.ReadAllChar(structData[i]);
        }
        private void ReadAllChar(string field)
        {
            if (field.Contains("=") == false || field.Contains(";") == false)
                return;

            string type = GetLineMessgaeType(field);

            string name = GetFieldName(field,type);

            this.AddFieldType(name, MessageTypeToLuaType(type));
            this.AddCommeents(name, GetCommeents(field));
            this.SetIsArry(name, field);
        }

        private string GetFieldName(string field,string type)
        {
            int startIndex = field.IndexOf(type) + type.Length;

            for (int i = startIndex; i < field.Length; ++i)
            {
                if (field[i] != ' ')
                {
                    startIndex = i;
                    break;
                }
            }

            string data = field.Substring(startIndex);
            string name = data.Substring(0, data.IndexOf(' '));
            return name;
        }

        private string GetCommeents(string line)
        {
            if (line.Contains("//") == false)
                return "";

            return line.Substring(line.IndexOf("//") + 2);
        }

        private string MessageTypeToLuaType(string type)
        {
            string luaType = string.Empty;
            switch (type)
            {
                case "double": luaType = "number"; break;
                case "float": luaType = "number"; break;
                case "int32": luaType = "number"; break;
                case "int64": luaType = "number"; break;
                case "uint32": luaType = "number"; break;
                case "uint64": luaType = "number"; break;
                case "sint32": luaType = "number"; break;
                case "sint64": luaType = "number"; break;
                case "fixed32": luaType = "number"; break;
                case "fixed64": luaType = "number"; break;
                case "sfixed32": luaType = "number"; break;
                case "sfixed64": luaType = "number"; break;
                case "bool": luaType = "boolean"; break;
                case "string": luaType = "string"; break;
                case "bytes": luaType = "string"; break;
                default:luaType = type;break;
            }
            return luaType;
        }
        /// <summary>
        /// 获取这一行proto的文件类型
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string GetLineMessgaeType(string line)
        {
            string type = string.Empty;

            if (line.Contains("double"))
                type = "double";
            else if (line.Contains("float"))
                type = "float";
            else if (line.Contains("int32"))
                type = "int32";
            else if (line.Contains("int64"))
                type = "int64";
            else if (line.Contains("uint32"))
                type = "uint32";
            else if (line.Contains("uint64"))
                type = "uint64";
            else if (line.Contains("sint32"))
                type = "sint32";
            else if (line.Contains("sint64"))
                type = "sint64";
            else if (line.Contains("fixed32"))
                type = "fixed32";
            else if (line.Contains("fixed64"))
                type = "fixed64";
            else if (line.Contains("sfixed32"))
                type = "sfixed32";
            else if (line.Contains("sfixed64"))
                type = "sfixed64";
            else if (line.Contains("bool"))
                type = "bool";
            else if (line.Contains("string"))
                type = "string";
            else if (line.Contains("bytes"))
                type = "bytes";
            else
            {
                //不是系统定义的类型,是自定义的类型，需要进行解析
                string head = string.Empty;
                int startIndex = 0;

                if (line.Contains("repeated"))
                    head = "repeated";
                if (line.Contains("optional"))
                    head = "optional";
                //如果有前缀
                if (string.IsNullOrEmpty(head) == false)
                    startIndex = line.IndexOf(head) + head.Length;

                for (int i = startIndex; i < line.Length; ++i)
                {
                    if (line[i] != ' ')
                    {
                        startIndex = i;
                        break;
                    }
                }

                string data = line.Substring(startIndex);
                type = data.Substring(0, data.IndexOf(' '));

            }
            return type;
        }

    }
    //用于存贮结构体类型的类
    class EnumStruct
    {
        public static string type = "enum";
        public string name { get; set; }//枚举名字
        public string comments { get; set; }//注释
        public List<string> fields { get; set; }

        public EnumStruct(List<string> structData,string comments)
        {
            this.fields = new List<string>();

            if (string.IsNullOrEmpty(comments) && comments.Contains("//"))
                this.comments = comments.Substring(comments.IndexOf("//") + 2);
            this.name = Tools.GetClassName(structData[0]);
            this.SetEnumStruct(structData);
        }

        /// <summary>
        /// 设置枚举数据，因为枚举的话只需要设置就可以，无需设置注释
        /// 所以直接读取数据即可
        /// </summary>
        /// <param name="structData"></param>
        private void SetEnumStruct(List<string> structData)
        {
            for (int i = 0; i < structData.Count; ++i)
            {
                var data = structData[i];
                if (data.Contains("="))
                { 
                    data = data.Replace(';', ',');
                    data = data.Replace("//","--");
                    this.fields.Add(data);
                }
            }
        }
    }
}
