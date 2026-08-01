using System;
using System.Collections.Generic;

namespace MyProject
{
    /// <summary>
    /// 自动生成的数据类，对应 Event JSON 配置
    /// 由 JsonToClassGenerator 插件生成
    /// </summary>
    public class Event
    {
        /// <summary>原始字段: ID</summary>
        public int ID { get; set; }

        /// <summary>原始字段: eventType</summary>
        public int EventType { get; set; }

        /// <summary>原始字段: headType</summary>
        public int HeadType { get; set; }

        /// <summary>原始字段: des</summary>
        public string Des { get; set; }

        /// <summary>原始字段: image</summary>
        public string Image { get; set; }

        /// <summary>原始字段: option</summary>
        public List<string> Option { get; set; }

        /// <summary>原始字段: demand</summary>
        public List<object> Demand { get; set; }

        /// <summary>原始字段: demandNum</summary>
        public List<object> DemandNum { get; set; }

        /// <summary>原始字段: nextEvent</summary>
        public List<int> NextEvent { get; set; }

        /// <summary>原始字段: icon</summary>
        public List<object> Icon { get; set; }
    }
}
