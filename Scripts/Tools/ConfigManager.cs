using System;
using System.Collections.Generic;
using MyProject.Tools;
using Godot;

namespace MyProject
{
	/// <summary>
	/// 配置管理单例。游戏启动时自动加载所有 JSON 配置表。
	/// 由 JsonToClassGenerator 插件自动生成，请勿手动编辑。
	/// </summary>
	public partial class ConfigManager : Node
	{
		public static ConfigManager Instance { get; private set; }

		/// <summary>Item 配置列表</summary>
		public List<Item> itemList { get; private set; }
		/// <summary>Item 配置字典（以 ID 为键）</summary>
		public Dictionary<int, Item> itemDic { get; private set; }

		/// <summary>PlaceInfo 配置列表</summary>
		public List<PlaceInfo> placeInfoList { get; private set; }
		/// <summary>PlaceInfo 配置字典（以 ID 为键）</summary>
		public Dictionary<int, PlaceInfo> placeInfoDic { get; private set; }

		/// <summary>State 配置列表</summary>
		public List<State> stateList { get; private set; }
		/// <summary>State 配置字典（以 ID 为键）</summary>
		public Dictionary<int, State> stateDic { get; private set; }

		/// <summary>Talent 配置列表</summary>
		public List<Talent> talentList { get; private set; }
		/// <summary>Talent 配置字典（以 ID 为键）</summary>
		public Dictionary<int, Talent> talentDic { get; private set; }

		public override void _Ready()
		{
			if (Instance != null)
			{
				GD.PrintErr("[ConfigManager] 单例已存在，重复创建！");
				QueueFree();
				return;
			}

			Instance = this;

			itemList = JsonLoader.LoadToList<Item>("item");
			itemDic = JsonLoader.LoadToDic<Item>("item");
			GD.Print("[ConfigManager] Item loaded: List=" + (itemList?.Count ?? 0) + ", Dic=" + (itemDic?.Count ?? 0));

			placeInfoList = JsonLoader.LoadToList<PlaceInfo>("place_info");
			placeInfoDic = JsonLoader.LoadToDic<PlaceInfo>("place_info");
			GD.Print("[ConfigManager] PlaceInfo loaded: List=" + (placeInfoList?.Count ?? 0) + ", Dic=" + (placeInfoDic?.Count ?? 0));

			stateList = JsonLoader.LoadToList<State>("state");
			stateDic = JsonLoader.LoadToDic<State>("state");
			GD.Print("[ConfigManager] State loaded: List=" + (stateList?.Count ?? 0) + ", Dic=" + (stateDic?.Count ?? 0));

			talentList = JsonLoader.LoadToList<Talent>("talent");
			talentDic = JsonLoader.LoadToDic<Talent>("talent");
			GD.Print("[ConfigManager] Talent loaded: List=" + (talentList?.Count ?? 0) + ", Dic=" + (talentDic?.Count ?? 0));

			GD.Print("[ConfigManager] 所有配置表加载完成");
		}
	}
}
