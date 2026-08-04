using System;
using System.Collections.Generic;

namespace MyProject
{
    /// <summary>
    /// 自动生成的数据类，对应 Room JSON 配置
    /// 由 JsonToClassGenerator 插件生成
    /// </summary>
    public class Room
    {
        /// <summary>原始字段: ID</summary>
        public int ID { get; set; }

        /// <summary>原始字段: name</summary>
        public string Name { get; set; }

        /// <summary>原始字段: des</summary>
        public string Des { get; set; }

        /// <summary>原始字段: image</summary>
        public string Image { get; set; }

        /// <summary>原始字段: food</summary>
        public int Food { get; set; }

        /// <summary>原始字段: medic</summary>
        public int Medic { get; set; }

        /// <summary>原始字段: equip</summary>
        public int Equip { get; set; }

        /// <summary>原始字段: material</summary>
        public int Material { get; set; }

        /// <summary>原始字段: roomLayer</summary>
        public int RoomLayer { get; set; }

        /// <summary>原始字段: eventPool</summary>
        public List<int> EventPool { get; set; }

        /// <summary>原始字段: eventPool2</summary>
        public List<int> EventPool2 { get; set; }

        /// <summary>原始字段: subTask</summary>
        public List<int> SubTask { get; set; }

        /// <summary>原始字段: triggerProgress</summary>
        public List<int> TriggerProgress { get; set; }
    }
}
