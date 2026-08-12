using System;
using System.Collections.Generic;

namespace MyProject
{
    /// <summary>
    /// 自动生成的数据类，对应 RoofWorkstationItem JSON 配置
    /// 由 JsonToClassGenerator 插件生成
    /// </summary>
    public class RoofWorkstationItem
    {
        /// <summary>原始字段: ID(int)</summary>
        public int ID { get; set; }

        /// <summary>原始字段: itemID(int)</summary>
        public int ItemID { get; set; }

        /// <summary>原始字段: type(int)</summary>
        public int Type { get; set; }

        /// <summary>原始字段: level(int)</summary>
        public int Level { get; set; }

        /// <summary>原始字段: materialID(int)</summary>
        public List<int> MaterialID { get; set; }

        /// <summary>原始字段: materialNum(int)</summary>
        public List<int> MaterialNum { get; set; }

        /// <summary>原始字段: stamina(int)</summary>
        public int Stamina { get; set; }

        /// <summary>原始字段: getID(int)</summary>
        public List<int> GetID { get; set; }

        /// <summary>原始字段: getNum(int)</summary>
        public List<int> GetNum { get; set; }
    }
}
