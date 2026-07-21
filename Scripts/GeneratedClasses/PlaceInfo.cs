using System;
using System.Collections.Generic;

namespace MyProject
{
	/// <summary>
	/// 自动生成的数据类，对应 PlaceInfo JSON 配置
	/// 由 JsonToClassGenerator 插件生成
	/// </summary>
	public class PlaceInfo
	{
		/// <summary>原始字段: ID</summary>
		public int ID { get; set; }

		/// <summary>原始字段: name</summary>
		public string Name { get; set; }

		/// <summary>原始字段: image</summary>
		public string Image { get; set; }

		/// <summary>原始字段: difficulty</summary>
		public int Difficulty { get; set; }

		/// <summary>原始字段: weight</summary>
		public int Weight { get; set; }

		/// <summary>原始字段: stage</summary>
		public int Stage { get; set; }

		/// <summary>原始字段: eventNum</summary>
		public int EventNum { get; set; }

		/// <summary>原始字段: enterEventID</summary>
		public int EnterEventID { get; set; }

		/// <summary>原始字段: endEventID</summary>
		public int EndEventID { get; set; }

		/// <summary>原始字段: item</summary>
		public List<int> Item { get; set; }
	}
}
