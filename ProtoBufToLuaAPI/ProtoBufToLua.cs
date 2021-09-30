using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace ProtoBufToLuaAPI
{
    //用于存贮消息类型的类
   

    class ProtoBufToLua
    {
        private List<EnumStruct> enumList { get; set; }
        private List<MessageStruct> messageList { get; set; }
        private List<string> filePath { get; set; }//所有的.proto文件的路径
        public ProtoBufToLua()
        {
            enumList = new List<EnumStruct>();
            messageList = new List<MessageStruct>();
            filePath = new List<string>();
        }
        private string ExprotPath = null;
        public void Init()
        {
            InitPath();
            GenAPI();
            WriteAllEnum();
            WriteAllMessage();
            Console.WriteLine("请按任意键继续...");
            Console.ReadKey();
        }
        private void InitPath()
        {
            string path = Directory.GetCurrentDirectory();
            var parent = Directory.GetParent(path);
            //获得父路径下所有的文件夹名
            string[] dirs = Directory.GetDirectories(parent.FullName);

            for (int i = 0; i < dirs.Length; ++i)
            {
                var files = Directory.GetFiles(dirs[i]);
                foreach (var name in files)
                {
                    if (name.IndexOf(".proto") > 0)
                        filePath.Add(name);
                }
            }

            ExprotPath = parent.FullName + "\\" + "LuaAPI";
            //创建导出的文件的地址
            if (Directory.Exists(ExprotPath) == false)
                Directory.CreateDirectory(ExprotPath);
        }

        private void GenAPI()
        {
            foreach (string path in this.filePath)
            {
                Console.WriteLine("读取文件--->" + path);
                var lines = File.ReadAllLines(path);
                ReadProto(lines);
            }
        }

        private void ReadProto(string[] file)
        {
            for (long i = 0; i < file.Length; ++i)
            {
                string line = file[i];
                if (line == "")
                    continue;

                if (line.Contains("import") || line.Contains("package com.gy.server.packet;") 
                    || line.Contains("option java_package") || line.Contains("option java_outer_classname ")
                    || line.Contains("=") || line.Contains("//"))
                    continue;

                if (Tools.StringIsStruct(line))
                    GetStructLines(i, file);
            }
        }
        /// <summary>
        /// 获得一个结构体再文件中的所有的行数
        /// </summary>
        /// <param name="startIndex">结构体起始的行数</param>
        /// <param name="files">结构体所在文件的所有内容</param>
        /// <returns>返回这个结构截止下标</returns>
        private long GetStructLines(long startIndex,string[] files)
        {
            List<string> lins = new List<string>();
            lins.Add(files[startIndex++]);//后置++，添加第一行进列表后下标自动+1

            for (long i = startIndex; i < files.Length; ++i)
            {
                string data = files[i];
                if (Tools.StringIsStruct(data))
                {
                    i = GetStructLines(i, files);//获取到一个新的消息的所有行,嵌套的消息
                }
                else if (data.Contains("}"))//当前结构体的结尾,在这里将获取到的结构添加
                {
                    lins.Add(data);
                    DistinguishStruct(lins,i > 0 ? files[i - 1] : string.Empty);
                    return i;
                }
                else
                    lins.Add(data);
            }
            return startIndex;
        }
        /// <summary>
        /// 区分结构，将结构添加进数据中
        /// </summary>
        /// <param name="file">结构的所有字符串</param>
        /// <param name="comments">结构的注释</param>
        private void DistinguishStruct(List<string> file,string comments)
        {
            if (file.Count == 0)
                return;
            var title = file[0];

            if (StructIsAdd(title) || title.Contains("="))
                return;

            if (title.Contains(MessageStruct.type))
            {

                Console.WriteLine("读取到" + MessageStruct.type + "--->" + Tools.GetClassName(title));
                var data = new MessageStruct(file, comments);
                this.messageList.Add(data);
            }

            if (title.Contains(EnumStruct.type))
            {
                Console.WriteLine("读取到" + EnumStruct.type + "--->" + Tools.GetClassName(title));
                var data = new EnumStruct(file,comments);
                this.enumList.Add(data);
            }
        }
        private bool StructIsAdd(string title)
        {
            var className = Tools.GetClassName(title);
            foreach (var data in this.messageList)
            {
                if (data.name == className)
                    return true;
            }

            foreach (var data in this.enumList)
            {
                if (data.name == className)
                    return true;
            }

            return false;
        }
        /// <summary>
        /// 将读取到的结构体写到同一个文件中
        /// </summary>
        private void WriteAllEnum()
        {
            string typeName = "MessageEnum";
            string fileName = this.ExprotPath + "\\" + typeName + ".lua";
            List<string> files = new List<string>();
            files.Add(typeName + " = {}");
            for (int i = 0; i < this.enumList.Count; ++i)
            {
                var enumData = this.enumList[i];
                Console.WriteLine("写入结构体--->" + enumData.name);

                string className = string.Empty;
                className = "---@class " + enumData.name + " " + (string.IsNullOrEmpty(enumData.comments) ? " " : enumData.comments);
                files.Add(className);

                files.Add(typeName + "." + enumData.name + " = {");

                for (int j = 0; j < enumData.fields.Count; ++j)
                    files.Add(enumData.fields[j]);

                files.Add("}");
            }
            Console.WriteLine("正在写文件MessageEnum");
            File.WriteAllLines(fileName,files.ToArray());
            Console.WriteLine("枚举类型处理完毕");
        }
        /// <summary>
        /// 将读取到的message写到同一个文件中
        /// </summary>
        private void WriteAllMessage()
        {
            string typeName = "Message";
            string fileName = this.ExprotPath + "\\" + typeName + ".lua";

            List<string> files = new List<string>();

            for (int i = 0; i < this.messageList.Count; ++i)
            {
                var messageData = this.messageList[i];

                Console.WriteLine("写入消息--->" + messageData.name);

                string className = string.Empty;
                className = "---@class " + messageData.name;
                files.Add(className);

                for (int j = 0; j < messageData.fields.Count; ++j)
                    files.Add(string.Format("---@field {0} {1}{2} {3}", 
                        messageData.fields[j].name, 
                        messageData.fields[j].type,
                        messageData.fields[j].isArry ? "[]" : "",
                        messageData.fields[j].comments));

                files.Add(string.Format("{0} = ", messageData.name) + "{ }");
                files.Add("");
            }

            Console.WriteLine("正在写文件MessageEnum");
            File.WriteAllLines(fileName, files.ToArray());
            Console.WriteLine("消息类型处理完毕");
        }
    }
}
