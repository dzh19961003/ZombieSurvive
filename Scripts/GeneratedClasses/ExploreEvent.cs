using System;
using System.Collections.Generic;

namespace MyProject
{
    /// <summary>
    /// 自动生成的数据类，对应 ExploreEvent JSON 配置
    /// 由 JsonToClassGenerator 插件生成
    /// </summary>
    public class ExploreEvent
    {
        /// <summary>原始字段: ID(int)</summary>
        public int ID { get; set; }

        /// <summary>原始字段: eventType(int)</summary>
        public int EventType { get; set; }

        /// <summary>原始字段: headType(int)</summary>
        public int HeadType { get; set; }

        /// <summary>原始字段: des(string)</summary>
        public string Des { get; set; }

        /// <summary>原始字段: image(string)</summary>
        public string Image { get; set; }

        /// <summary>原始字段: option(string)</summary>
        public List<string> Option { get; set; }

        /// <summary>原始字段: demand(int)</summary>
        public List<int> Demand { get; set; }

        /// <summary>原始字段: demandNum(int)</summary>
        public List<int> DemandNum { get; set; }

        /// <summary>原始字段: nextEvent(int)</summary>
        public List<int> NextEvent { get; set; }

        /// <summary>原始字段: icon(string)</summary>
        public List<string> Icon { get; set; }

        /// <summary>原始字段: itemID(int)</summary>
        public List<int> ItemID { get; set; }

        /// <summary>原始字段: itemNum(int)</summary>
        public List<int> ItemNum { get; set; }

        /// <summary>原始字段: stateID(int)</summary>
        public List<int> StateID { get; set; }
    }
}
