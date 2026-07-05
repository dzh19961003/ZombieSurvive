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

        /// <summary>PlaceInfo 配置列表</summary>
        public List<PlaceInfo> placeInfoList { get; private set; }
        /// <summary>PlaceInfo 配置字典（以 ID 为键）</summary>
        public Dictionary<int, PlaceInfo> placeInfoDic { get; private set; }

        public override void _Ready()
        {
            if (Instance != null)
            {
                GD.PrintErr("[ConfigManager] 单例已存在，重复创建！");
                QueueFree();
                return;
            }

            Instance = this;

            placeInfoList = JsonLoader.LoadToList<PlaceInfo>("place_info");
            placeInfoDic = JsonLoader.LoadToDic<PlaceInfo>("place_info");
            GD.Print("[ConfigManager] PlaceInfo loaded: List=" + (placeInfoList?.Count ?? 0) + ", Dic=" + (placeInfoDic?.Count ?? 0));

            GD.Print("[ConfigManager] 所有配置表加载完成");
        }
    }
}
