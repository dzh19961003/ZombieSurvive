using System;
using System.Collections.Generic;

namespace MyProject
{
    /// <summary>
    /// 自动生成的数据类，对应 Building JSON 配置
    /// 由 JsonToClassGenerator 插件生成
    /// </summary>
    public class Building
    {
        /// <summary>原始字段: ID(int)</summary>
        public int ID { get; set; }

        /// <summary>原始字段: name(string)</summary>
        public string Name { get; set; }

        /// <summary>原始字段: image(string)</summary>
        public string Image { get; set; }

        /// <summary>原始字段: stars(int)</summary>
        public int Stars { get; set; }

        /// <summary>原始字段: des(string)</summary>
        public string Des { get; set; }

        /// <summary>原始字段: enterDes(string)</summary>
        public string EnterDes { get; set; }

        /// <summary>原始字段: food(int)</summary>
        public int Food { get; set; }

        /// <summary>原始字段: medic(int)</summary>
        public int Medic { get; set; }

        /// <summary>原始字段: equip(int)</summary>
        public int Equip { get; set; }

        /// <summary>原始字段: material(int)</summary>
        public int Material { get; set; }

        /// <summary>原始字段: roomID(int)</summary>
        public List<int> RoomID { get; set; }
    }
}
