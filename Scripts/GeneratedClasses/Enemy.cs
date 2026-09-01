using System;
using System.Collections.Generic;

namespace MyProject
{
    /// <summary>
    /// 自动生成的数据类，对应 Enemy JSON 配置
    /// 由 JsonToClassGenerator 插件生成
    /// </summary>
    public class Enemy
    {
        /// <summary>原始字段: ID(int)</summary>
        public int ID { get; set; }

        /// <summary>原始字段: level(int)</summary>
        public int Level { get; set; }

        /// <summary>原始字段: headNum(int)</summary>
        public int HeadNum { get; set; }

        /// <summary>原始字段: bodyNum(int)</summary>
        public int BodyNum { get; set; }

        /// <summary>原始字段: armNum(int)</summary>
        public int ArmNum { get; set; }

        /// <summary>原始字段: battleEffectID(int)</summary>
        public List<int> BattleEffectID { get; set; }

        /// <summary>原始字段: headHP(double)</summary>
        public List<double> HeadHP { get; set; }

        /// <summary>原始字段: bodyHP(double)</summary>
        public List<double> BodyHP { get; set; }

        /// <summary>原始字段: armHP(double)</summary>
        public List<double> ArmHP { get; set; }
    }
}
