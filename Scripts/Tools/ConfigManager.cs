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

        /// <summary>Building 配置列表</summary>
        public List<Building> buildingList { get; private set; }
        /// <summary>Building 配置字典（以 ID 为键）</summary>
        public Dictionary<int, Building> buildingDic { get; private set; }

        /// <summary>Diary 配置列表</summary>
        public List<Diary> diaryList { get; private set; }
        /// <summary>Diary 配置字典（以 ID 为键）</summary>
        public Dictionary<int, Diary> diaryDic { get; private set; }

        /// <summary>EffectType 配置列表</summary>
        public List<EffectType> effectTypeList { get; private set; }
        /// <summary>EffectType 配置字典（以 ID 为键）</summary>
        public Dictionary<int, EffectType> effectTypeDic { get; private set; }

        /// <summary>Enemy 配置列表</summary>
        public List<Enemy> enemyList { get; private set; }
        /// <summary>Enemy 配置字典（以 ID 为键）</summary>
        public Dictionary<int, Enemy> enemyDic { get; private set; }

        /// <summary>EnemyPool 配置列表</summary>
        public List<EnemyPool> enemyPoolList { get; private set; }
        /// <summary>EnemyPool 配置字典（以 ID 为键）</summary>
        public Dictionary<int, EnemyPool> enemyPoolDic { get; private set; }

        /// <summary>Equip 配置列表</summary>
        public List<Equip> equipList { get; private set; }
        /// <summary>Equip 配置字典（以 ID 为键）</summary>
        public Dictionary<int, Equip> equipDic { get; private set; }

        /// <summary>EquipEffect 配置列表</summary>
        public List<EquipEffect> equipEffectList { get; private set; }
        /// <summary>EquipEffect 配置字典（以 ID 为键）</summary>
        public Dictionary<int, EquipEffect> equipEffectDic { get; private set; }

        /// <summary>Event 配置列表</summary>
        public List<Event> eventList { get; private set; }
        /// <summary>Event 配置字典（以 ID 为键）</summary>
        public Dictionary<int, Event> eventDic { get; private set; }

        /// <summary>EventPool 配置列表</summary>
        public List<EventPool> eventPoolList { get; private set; }
        /// <summary>EventPool 配置字典（以 ID 为键）</summary>
        public Dictionary<int, EventPool> eventPoolDic { get; private set; }

        /// <summary>ExploreEvent 配置列表</summary>
        public List<ExploreEvent> exploreEventList { get; private set; }
        /// <summary>ExploreEvent 配置字典（以 ID 为键）</summary>
        public Dictionary<int, ExploreEvent> exploreEventDic { get; private set; }

        /// <summary>Item 配置列表</summary>
        public List<Item> itemList { get; private set; }
        /// <summary>Item 配置字典（以 ID 为键）</summary>
        public Dictionary<int, Item> itemDic { get; private set; }

        /// <summary>ItemPool 配置列表</summary>
        public List<ItemPool> itemPoolList { get; private set; }
        /// <summary>ItemPool 配置字典（以 ID 为键）</summary>
        public Dictionary<int, ItemPool> itemPoolDic { get; private set; }

        /// <summary>RoofTrain 配置列表</summary>
        public List<RoofTrain> roofTrainList { get; private set; }
        /// <summary>RoofTrain 配置字典（以 ID 为键）</summary>
        public Dictionary<int, RoofTrain> roofTrainDic { get; private set; }

        /// <summary>RoofWorkstation 配置列表</summary>
        public List<RoofWorkstation> roofWorkstationList { get; private set; }
        /// <summary>RoofWorkstation 配置字典（以 ID 为键）</summary>
        public Dictionary<int, RoofWorkstation> roofWorkstationDic { get; private set; }

        /// <summary>RoofWorkstationItem 配置列表</summary>
        public List<RoofWorkstationItem> roofWorkstationItemList { get; private set; }
        /// <summary>RoofWorkstationItem 配置字典（以 ID 为键）</summary>
        public Dictionary<int, RoofWorkstationItem> roofWorkstationItemDic { get; private set; }

        /// <summary>Room 配置列表</summary>
        public List<Room> roomList { get; private set; }
        /// <summary>Room 配置字典（以 ID 为键）</summary>
        public Dictionary<int, Room> roomDic { get; private set; }

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

            buildingList = JsonLoader.LoadToList<Building>("building");
            buildingDic = JsonLoader.LoadToDic<Building>("building");
            GD.Print("[ConfigManager] Building loaded: List=" + (buildingList?.Count ?? 0) + ", Dic=" + (buildingDic?.Count ?? 0));

            diaryList = JsonLoader.LoadToList<Diary>("diary");
            diaryDic = JsonLoader.LoadToDic<Diary>("diary");
            GD.Print("[ConfigManager] Diary loaded: List=" + (diaryList?.Count ?? 0) + ", Dic=" + (diaryDic?.Count ?? 0));

            effectTypeList = JsonLoader.LoadToList<EffectType>("effect_type");
            effectTypeDic = JsonLoader.LoadToDic<EffectType>("effect_type");
            GD.Print("[ConfigManager] EffectType loaded: List=" + (effectTypeList?.Count ?? 0) + ", Dic=" + (effectTypeDic?.Count ?? 0));

            enemyList = JsonLoader.LoadToList<Enemy>("enemy");
            enemyDic = JsonLoader.LoadToDic<Enemy>("enemy");
            GD.Print("[ConfigManager] Enemy loaded: List=" + (enemyList?.Count ?? 0) + ", Dic=" + (enemyDic?.Count ?? 0));

            enemyPoolList = JsonLoader.LoadToList<EnemyPool>("enemy_pool");
            enemyPoolDic = JsonLoader.LoadToDic<EnemyPool>("enemy_pool");
            GD.Print("[ConfigManager] EnemyPool loaded: List=" + (enemyPoolList?.Count ?? 0) + ", Dic=" + (enemyPoolDic?.Count ?? 0));

            equipList = JsonLoader.LoadToList<Equip>("equip");
            equipDic = JsonLoader.LoadToDic<Equip>("equip");
            GD.Print("[ConfigManager] Equip loaded: List=" + (equipList?.Count ?? 0) + ", Dic=" + (equipDic?.Count ?? 0));

            equipEffectList = JsonLoader.LoadToList<EquipEffect>("equip_effect");
            equipEffectDic = JsonLoader.LoadToDic<EquipEffect>("equip_effect");
            GD.Print("[ConfigManager] EquipEffect loaded: List=" + (equipEffectList?.Count ?? 0) + ", Dic=" + (equipEffectDic?.Count ?? 0));

            eventList = JsonLoader.LoadToList<Event>("event");
            eventDic = JsonLoader.LoadToDic<Event>("event");
            GD.Print("[ConfigManager] Event loaded: List=" + (eventList?.Count ?? 0) + ", Dic=" + (eventDic?.Count ?? 0));

            eventPoolList = JsonLoader.LoadToList<EventPool>("event_pool");
            eventPoolDic = JsonLoader.LoadToDic<EventPool>("event_pool");
            GD.Print("[ConfigManager] EventPool loaded: List=" + (eventPoolList?.Count ?? 0) + ", Dic=" + (eventPoolDic?.Count ?? 0));

            exploreEventList = JsonLoader.LoadToList<ExploreEvent>("explore_event");
            exploreEventDic = JsonLoader.LoadToDic<ExploreEvent>("explore_event");
            GD.Print("[ConfigManager] ExploreEvent loaded: List=" + (exploreEventList?.Count ?? 0) + ", Dic=" + (exploreEventDic?.Count ?? 0));

            itemList = JsonLoader.LoadToList<Item>("item");
            itemDic = JsonLoader.LoadToDic<Item>("item");
            GD.Print("[ConfigManager] Item loaded: List=" + (itemList?.Count ?? 0) + ", Dic=" + (itemDic?.Count ?? 0));

            itemPoolList = JsonLoader.LoadToList<ItemPool>("item_pool");
            itemPoolDic = JsonLoader.LoadToDic<ItemPool>("item_pool");
            GD.Print("[ConfigManager] ItemPool loaded: List=" + (itemPoolList?.Count ?? 0) + ", Dic=" + (itemPoolDic?.Count ?? 0));

            roofTrainList = JsonLoader.LoadToList<RoofTrain>("roof_train");
            roofTrainDic = JsonLoader.LoadToDic<RoofTrain>("roof_train");
            GD.Print("[ConfigManager] RoofTrain loaded: List=" + (roofTrainList?.Count ?? 0) + ", Dic=" + (roofTrainDic?.Count ?? 0));

            roofWorkstationList = JsonLoader.LoadToList<RoofWorkstation>("roof_workstation");
            roofWorkstationDic = JsonLoader.LoadToDic<RoofWorkstation>("roof_workstation");
            GD.Print("[ConfigManager] RoofWorkstation loaded: List=" + (roofWorkstationList?.Count ?? 0) + ", Dic=" + (roofWorkstationDic?.Count ?? 0));

            roofWorkstationItemList = JsonLoader.LoadToList<RoofWorkstationItem>("roof_workstation_item");
            roofWorkstationItemDic = JsonLoader.LoadToDic<RoofWorkstationItem>("roof_workstation_item");
            GD.Print("[ConfigManager] RoofWorkstationItem loaded: List=" + (roofWorkstationItemList?.Count ?? 0) + ", Dic=" + (roofWorkstationItemDic?.Count ?? 0));

            roomList = JsonLoader.LoadToList<Room>("room");
            roomDic = JsonLoader.LoadToDic<Room>("room");
            GD.Print("[ConfigManager] Room loaded: List=" + (roomList?.Count ?? 0) + ", Dic=" + (roomDic?.Count ?? 0));

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
