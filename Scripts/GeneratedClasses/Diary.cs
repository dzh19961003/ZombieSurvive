using System;
using System.Collections.Generic;

namespace MyProject
{
    /// <summary>
    /// 自动生成的数据类，对应 Diary JSON 配置
    /// 由 JsonToClassGenerator 插件生成
    /// </summary>
    public class Diary
    {
        /// <summary>原始字段: ID(int)</summary>
        public int ID { get; set; }

        /// <summary>原始字段: type(int)</summary>
        public int Type { get; set; }

        /// <summary>原始字段: day(int)</summary>
        public int Day { get; set; }

        /// <summary>原始字段: diaries(string)</summary>
        public string Diaries { get; set; }

        /// <summary>原始字段: scriptID(int)</summary>
        public List<int> ScriptID { get; set; }
    }
}
