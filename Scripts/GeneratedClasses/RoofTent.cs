using System;
using System.Collections.Generic;

namespace MyProject
{
    /// <summary>
    /// 自动生成的数据类，对应 RoofTent JSON 配置
    /// 由 JsonToClassGenerator 插件生成
    /// </summary>
    public class RoofTent
    {
        /// <summary>原始字段: ID(int)</summary>
        public int ID { get; set; }

        /// <summary>原始字段: level(int)</summary>
        public int Level { get; set; }

        /// <summary>原始字段: roofStamina(int)</summary>
        public int RoofStamina { get; set; }

        /// <summary>原始字段: exploreStamina(int)</summary>
        public int ExploreStamina { get; set; }

        /// <summary>原始字段: icon(string)</summary>
        public string Icon { get; set; }

        /// <summary>原始字段: itemID(int)</summary>
        public List<int> ItemID { get; set; }

        /// <summary>原始字段: itemNum(int)</summary>
        public List<int> ItemNum { get; set; }
    }
}
