using System;
using System.Collections.Generic;

namespace MyProject
{
    /// <summary>
    /// 自动生成的数据类，对应 EnemyPool JSON 配置
    /// 由 JsonToClassGenerator 插件生成
    /// </summary>
    public class EnemyPool
    {
        /// <summary>原始字段: ID(int)</summary>
        public int ID { get; set; }

        /// <summary>原始字段: enemy(int)</summary>
        public List<int> Enemy { get; set; }

        /// <summary>原始字段: weight(int)</summary>
        public List<int> Weight { get; set; }
    }
}
