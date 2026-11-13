using Godot;
using MyProject;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// 第五大点：探索详情界面
// 一次搜索后抽出一个事件：描述逐字打印 → 发奖励(飘字) → 生成选项 → 点选项后跳转到下一个事件或收尾
public partial class ExploreChooseUI : Control
{
    // 顶部状态栏（和第三、四大点完全一样）
    [Export] public Label hpLabel;
    [Export] public Label armorLabel;
    [Export] public Label staminaLabel;
    [Export] public Label bagLabel;
    [Export] public Label titleLabel;
    [Export] public HBoxContainer layerDots;

    // 房间信息框：房间名、剩余次数、噪音
    [Export] public Label roomNameLabel;
    [Export] public Label roomLeftLabel;
    [Export] public Label noiseLabel;
    [Export] public TextureProgressBar noiseBar;

    // 事件区：大图、描述、选项容器、物品飘字层
    [Export] public TextureRect eventImage;
    [Export] public Label desLabel;
    [Export] public VBoxContainer optionBox;
    [Export] public Control floatLayer;
    [Export] public TextureRect bgImage;

    private const string IconDir = "res://Assets/Images/UI/Explore/ExploreNew/";
    private const string OptionPath = "res://UI/Explore/eventOptionBar.tscn";
    private const string StrategyPath = "res://UI/Explore/ExploreStrategyUI.tscn";

    private static readonly Color NormalColor = new Color(0.91f, 0.89f, 0.78f);
    private static readonly Color WarnColor = new Color(1f, 0.35f, 0.3f);

    // 事件结束后回到下面那层探索策略界面，直接刷新它，不重新创建
    public ExploreStrategyUI strategy;

    private Room currentRoom;
    private ExploreEvent currentEvent;
    private readonly List<int> eventPool = new List<int>();

    public override void _Ready()
    {
        // F6 单跑这个场景时单例都不存在，用假数据把界面画出来
        if (GameManager.Instance == null || PlayerManager.Instance == null || ConfigManager.Instance == null)
        {
            ShowPreview();
            return;
        }
    }

    public override void _ExitTree()
    {
        // TextTyper 的结束事件是全局静态的，界面销毁时一定要退订
        TextTyper.OnTypeEnd -= OnDesTyped;
    }

    // 由 ExploreStrategyUI 在 CreateUI 之后调用；searchType: 1=快速 2=仔细
    public void Init(int searchType, ExploreStrategyUI owner)
    {
        strategy = owner;
        RefreshAll();

        BuildEventPool(searchType);
        int eventID = PickEventID();
        if (eventID == 0)
        {
            GD.PrintErr("[ExploreChooseUI] 事件池为空，本次不出事件");
            BackToStrategy();
            return;
        }
        ShowEvent(eventID);
    }

    #region 界面刷新
    public void RefreshAll()
    {
        var gm = GameManager.Instance;
        Building building = ConfigManager.Instance.buildingDic[gm.currentBuildingID];
        titleLabel.Text = building.Name;
        SpawnLayerDots(building.LayerNum, gm.exploreLayer);
        RefreshNumbers();

        currentRoom = ConfigManager.Instance.roomDic[gm.roomID];
        roomNameLabel.Text = currentRoom.Name;
        roomLeftLabel.Text = gm.GetRoomExploreNum(currentRoom.ID, currentRoom.ExploreNum).ToString();
        RefreshNoise();
        ShowBuildingBg(building);
    }

    public void RefreshNumbers()
    {
        var pm = PlayerManager.Instance;
        var gm = GameManager.Instance;
        hpLabel.Text = pm.Hp.ToString();
        armorLabel.Text = pm.Armor.ToString();
        staminaLabel.Text = pm.ExploreStamina.ToString();

        int kinds = gm.BagItemDic.Count;
        bagLabel.Text = $"{kinds}/{pm.BagNum}";
        bagLabel.AddThemeColorOverride("font_color", kinds > pm.BagNum ? WarnColor : NormalColor);
    }

    private void RefreshNoise()
    {
        int noise = Mathf.Clamp(GameManager.Instance.exploreNoise, 0, 100);
        noiseBar.Value = noise;
        noiseLabel.Text = noise + "%";
    }

    private void SpawnLayerDots(int total, int current)
    {
        foreach (Node n in layerDots.GetChildren()) n.QueueFree();
        for (int i = 1; i <= total; i++)
        {
            string file = i <= current ? "progressIn.png" : "progressOut.png";
            var dot = new TextureRect();
            dot.Texture = GD.Load<Texture2D>(IconDir + file);
            dot.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            dot.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            dot.CustomMinimumSize = new Vector2(30, 30);
            layerDots.AddChild(dot);
        }
    }

    private void ShowBuildingBg(Building building)
    {
        // 以后补了建筑背景图，按 building 表 image 字段放进 BuildingBg 目录会自动显示
        string path = $"res://Assets/Images/BuildingBg/{building.Image}.png";
        if (!ResourceLoader.Exists(path)) return;
        bgImage.Texture = GD.Load<Texture2D>(path);
        bgImage.Visible = true;
    }
    #endregion

    #region 事件池与支线
    // 策划案 B/C：快速搜索读 room.EventPool，仔细搜索读 room.EventPool2
    // 过滤沿用旧规则：只留 headType=1 且不是支线(102/103) 的事件当"起始事件"
    private void BuildEventPool(int searchType)
    {
        eventPool.Clear();
        var poolIDs = searchType == 1 ? currentRoom.EventPool : currentRoom.EventPool2;
        if (poolIDs == null) return;

        foreach (int poolID in poolIDs)
        {
            if (!ConfigManager.Instance.eventPoolDic.TryGetValue(poolID, out EventPool pool)) continue;
            foreach (int id in pool.Event)
            {
                if (!ConfigManager.Instance.exploreEventDic.TryGetValue(id, out ExploreEvent ev)) continue;
                if (ev.HeadType != 1) continue;
                if (ev.EventType == 102 || ev.EventType == 103) continue;
                if (!eventPool.Contains(id)) eventPool.Add(id);
            }
        }
    }

    // 已探索次数 = 最大次数 - 剩余次数；ceil(exploreNum × triggerProgress / 100) 到了就必定触发支线
    private int PickEventID()
    {
        var gm = GameManager.Instance;
        int used = currentRoom.ExploreNum - gm.GetRoomExploreNum(currentRoom.ID, currentRoom.ExploreNum);
        var done = GetDoneSubTasks();

        if (currentRoom.SubTask != null)
        {
            for (int i = 0; i < currentRoom.SubTask.Count; i++)
            {
                int subID = currentRoom.SubTask[i];
                if (done.Contains(subID)) continue;
                int trigger = (currentRoom.TriggerProgress != null && i < currentRoom.TriggerProgress.Count)
                    ? currentRoom.TriggerProgress[i] : 0;
                int threshold = Mathf.CeilToInt(currentRoom.ExploreNum * trigger / 100f);
                if (threshold <= 0 || used < threshold) continue;
                if (!ConfigManager.Instance.exploreEventDic.ContainsKey(subID)) continue;

                gm.currentSubTask = subID;
                GD.Print($"[ExploreChooseUI] 已探索 {used} 次 ≥ 阈值 {threshold}，强制触发支线 {subID}");
                return subID;
            }
        }

        if (eventPool.Count == 0) return 0;
        return Tools.GetRandomNumber(eventPool);
    }

    private Godot.Collections.Array<int> GetDoneSubTasks()
    {
        var gm = GameManager.Instance;
        if (!gm.subTaskDic.ContainsKey(gm.roomID))
            gm.subTaskDic[gm.roomID] = new Godot.Collections.Array<int>();
        return gm.subTaskDic[gm.roomID];
    }

    private void MarkSubTaskDone(int subID)
    {
        var done = GetDoneSubTasks();
        if (!done.Contains(subID)) done.Add(subID);
        GD.Print($"[ExploreChooseUI] 支线 {subID} 标记完成");
    }

    // 关键支线(103)没走完时，剩余次数为 0 也强行留一次，界面会显示 1/8
    private bool HasUnfinishedKeySubTask()
    {
        if (currentRoom.SubTask == null) return false;
        var done = GetDoneSubTasks();
        foreach (int subID in currentRoom.SubTask)
        {
            if (done.Contains(subID)) continue;
            if (ConfigManager.Instance.exploreEventDic.TryGetValue(subID, out ExploreEvent ev) && ev.EventType == 103)
                return true;
        }
        return false;
    }
    #endregion

    #region 展示一个事件
    private void ShowEvent(int eventID)
    {
        if (!ConfigManager.Instance.exploreEventDic.TryGetValue(eventID, out ExploreEvent ev))
        {
            GD.PrintErr($"[ExploreChooseUI] 找不到事件 {eventID}");
            BackToStrategy();
            return;
        }
        currentEvent = ev;
        GameManager.Instance.currentEventID = eventID;

        // 普通支线(102)触发即算完成；关键支线(103)要走到 9998 才标记
        if (ev.EventType == 102) MarkSubTaskDone(eventID);
        if (ev.EventType == 103) GameManager.Instance.currentSubTask = eventID;

        // 事件大图，读 explore_event 表 image 字段
        string imgPath = $"res://Assets/Images/UI/Explore/{ev.Image}.png";
        eventImage.Texture = ResourceLoader.Exists(imgPath) ? GD.Load<Texture2D>(imgPath) : null;
        eventImage.Visible = eventImage.Texture != null;

        // 每次换事件都清掉上一轮的选项和飘字
        foreach (Node n in optionBox.GetChildren()) n.QueueFree();
        foreach (Node n in floatLayer.GetChildren()) n.QueueFree();

        RefreshNumbers();
        roomLeftLabel.Text = GameManager.Instance.GetRoomExploreNum(currentRoom.ID, currentRoom.ExploreNum).ToString();

        // 描述逐字打印；打完才出选项和奖励
        TextTyper.OnTypeEnd -= OnDesTyped;
        TextTyper.OnTypeEnd += OnDesTyped;
        TextTyper.TypeText(desLabel, ev.Des);
    }

    private void OnDesTyped()
    {
        TextTyper.OnTypeEnd -= OnDesTyped;
        if (!GodotObject.IsInstanceValid(this)) return;
        SpawnOptions();
        GiveRewards();
    }

    private void SpawnOptions()
    {
        var ev = currentEvent;
        var scene = GD.Load<PackedScene>(OptionPath);
        for (int i = 0; i < ev.Option.Count; i++)
        {
            int rank = i;   // 闭包里要固定住当前序号
            int next = i < ev.NextEvent.Count ? ev.NextEvent[i] : 9999;
            bool isExit = next == 9999 || next == 9998;

            bool enough = true;
            Texture2D demandTex = null;
            int need = 0;
            if (!isExit && i < ev.Demand.Count)
            {
                int demand = ev.Demand[i];
                need = i < ev.DemandNum.Count ? ev.DemandNum[i] : 0;
                enough = CheckDemand(demand, need, out demandTex);
            }

            var bar = scene.Instantiate<EventOptionBar>();
            optionBox.AddChild(bar);
            bar.Setup(ev.Option[i], enough, isExit, demandTex, need,
                () => OnOptionClicked(rank));
        }
    }

    // demand ≥ 10000 读属性，< 10000 读背包物品；返回是否达标，并给出右侧图标
    private bool CheckDemand(int demand, int need, out Texture2D icon)
    {
        icon = null;
        var gm = GameManager.Instance;

        if (demand >= 10000)
        {
            if (UIManager.Instance != null) icon = UIManager.Instance.GetItemIcon(demand);
            return GetProperty(demand) >= need;
        }

        // 物品需求只看背包（探索中消耗统一从背包扣，不从仓库扣）
        if (ConfigManager.Instance.itemDic.ContainsKey(demand) && UIManager.Instance != null)
            icon = UIManager.Instance.GetItemIcon(demand);
        return gm.GetBagItemCount(demand) >= need;
    }

    private int GetProperty(int demand)
    {
        var pm = PlayerManager.Instance;
        switch (demand)
        {
            case 10001: return (int)pm.Hp;
            case 10003: return pm.Strength;
            case 10004: return pm.Agility;
            case 10005: return pm.Intelligence;
            case 10010: return (int)pm.Armor;
            default: return 0;   // 表里 demand 目前只有强健/速度/智力
        }
    }
    #endregion

    #region 奖励与飘字
    // 描述打印完毕后发奖励。itemID/itemNum 成对，属性直接加，物品从池子里抽后进背包
    private void GiveRewards()
    {
        var ev = currentEvent;
        var gm = GameManager.Instance;
        var pm = PlayerManager.Instance;
        int order = 0;

        for (int i = 0; i < ev.ItemID.Count; i++)
        {
            int id = ev.ItemID[i];
            int num = i < ev.ItemNum.Count ? ev.ItemNum[i] : 0;

            // 100000 是剧情标记（记下"蓉市生物研究所"），item 表里没有，别往背包塞脏数据
            if (id == 100000)
            {
                GD.Print($"[ExploreChooseUI] 剧情标记 {id} / {num}，跳过发放");
                continue;
            }

            if (id >= 10000)
            {
                pm.AddItem(id, num);
                SpawnFloat(UIManager.Instance.GetItemIcon(id), num, order++);
            }
            else
            {
                if (!ConfigManager.Instance.itemPoolDic.TryGetValue(id, out ItemPool pool)) continue;
                // 同一个物品抽到多次就合并成一格显示
                var got = new Dictionary<int, int>();
                for (int j = 0; j < num; j++)
                {
                    int itemID = Tools.GetRandomNumber(pool.Item);
                    if (!gm.AddBagItem(itemID, 1))
                        GD.Print($"[ExploreChooseUI] 背包已满，物品 {itemID} 待丢弃逻辑处理");
                    got[itemID] = got.ContainsKey(itemID) ? got[itemID] + 1 : 1;
                }
                foreach (var kv in got)
                    SpawnFloat(UIManager.Instance.GetItemIcon(kv.Key), kv.Value, order++);
            }
        }

        // stateID 按约定先空置，只打日志
        if (ev.StateID != null)
            foreach (int sid in ev.StateID)
                GD.Print($"[ExploreChooseUI] 事件附带状态 {sid}（状态逻辑暂空置）");

        RefreshNumbers();
    }

    private void SpawnFloat(Texture2D icon, int num, int order)
    {
        if (icon == null) return;

        var node = new Control();
        node.Position = new Vector2(30, 140 - order * 104);
        node.Modulate = new Color(1, 1, 1, 0);
        node.MouseFilter = MouseFilterEnum.Ignore;

        var bg = new TextureRect();
        bg.Texture = GD.Load<Texture2D>(IconDir + "itemGet.png");
        bg.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        bg.StretchMode = TextureRect.StretchModeEnum.Scale;
        bg.Position = Vector2.Zero;
        bg.Size = new Vector2(240, 70);
        bg.MouseFilter = MouseFilterEnum.Ignore;
        node.AddChild(bg);

        // 图标 PNG 自带大片透明留白（如可乐罐图案只占 50×88 / 128×128），
        // 框给到 90×90 才画得出设计图里约 35×60 的可见图案
        var iconRect = new TextureRect();
        iconRect.Texture = icon;
        iconRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        iconRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        iconRect.Position = new Vector2(2, -8);
        iconRect.Size = new Vector2(90, 90);
        iconRect.MouseFilter = MouseFilterEnum.Ignore;
        node.AddChild(iconRect);

        var lab = new Label();
        lab.Text = (num >= 0 ? "+" : "") + num;
        lab.Position = new Vector2(100, 0);
        lab.Size = new Vector2(140, 70);
        lab.VerticalAlignment = VerticalAlignment.Center;
        lab.AddThemeFontSizeOverride("font_size", 50);
        lab.AddThemeColorOverride("font_color", Colors.White);
        lab.MouseFilter = MouseFilterEnum.Ignore;
        node.AddChild(lab);

        floatLayer.AddChild(node);
        _ = PlayFloatAnim(node, order * 300);
    }

    // 0.5s 渐入 → 1s 保持 → 0.5s 渐出，全程向上移动直到消失
    private async Task PlayFloatAnim(Control node, int startDelayMs)
    {
        if (startDelayMs > 0) await Task.Delay(startDelayMs);
        if (!GodotObject.IsInstanceValid(node)) return;

        Vector2 start = node.Position;
        float t = 0f;
        const float total = 2.0f;
        while (t < total)
        {
            await Task.Delay(16);
            if (!GodotObject.IsInstanceValid(node)) return;
            t += 0.016f;

            float alpha = t < 0.5f ? t / 0.5f
                : t > 1.5f ? (total - t) / 0.5f : 1f;
            node.Modulate = new Color(1, 1, 1, Mathf.Clamp(alpha, 0f, 1f));
            node.Position = start + new Vector2(0, -120f * (t / total));
        }
        if (GodotObject.IsInstanceValid(node)) node.QueueFree();
    }
    #endregion

    #region 选项点击与收尾
    private void OnOptionClicked(int rank)
    {
        var ev = currentEvent;
        var gm = GameManager.Instance;

        // isRemove=1 的物品需求，选中后从背包扣掉
        if (rank < ev.Demand.Count && rank < ev.DemandNum.Count && rank < ev.IsRemove.Count
            && ev.IsRemove[rank] == 1 && ev.Demand[rank] < 10000)
        {
            int removed = gm.RemoveBagItem(ev.Demand[rank], ev.DemandNum[rank]);
            GD.Print($"[ExploreChooseUI] 需求消耗物品 {ev.Demand[rank]} ×{removed}");
        }

        int next = rank < ev.NextEvent.Count ? ev.NextEvent[rank] : 9999;
        if (next == 9999 || next == 9998)
        {
            FinishEvent(next == 9998);
            return;
        }

        // nextEvent 指向 eventType=202 时触发战斗（战斗逻辑暂空置）
        if (ConfigManager.Instance.exploreEventDic.TryGetValue(next, out ExploreEvent nextEv)
            && nextEv.EventType == 202)
        {
            GD.Print($"[ExploreChooseUI] 触发战斗，怪物池 {currentRoom.EnemyPool}（战斗逻辑暂空置）");
            FinishEvent(false);
            return;
        }

        ShowEvent(next);
    }

    private void FinishEvent(bool keySubTaskDone)
    {
        var gm = GameManager.Instance;
        if (keySubTaskDone && gm.currentSubTask > 0)
            MarkSubTaskDone(gm.currentSubTask);

        // 关键支线没完成，剩余次数用尽也强行留一次（房间上会显示 1/8）
        int left = gm.GetRoomExploreNum(currentRoom.ID, currentRoom.ExploreNum);
        if (left == 0 && HasUnfinishedKeySubTask())
        {
            gm.SetRoomExploreNum(currentRoom.ID, 1);
            GD.Print("[ExploreChooseUI] 关键支线未完成，保留一次探索机会");
        }

        BackToStrategy();
    }

    private void BackToStrategy()
    {
        TextTyper.OnTypeEnd -= OnDesTyped;
        var ui = UIManager.Instance;
        ui.DeleteUI(this);
        if (strategy != null && GodotObject.IsInstanceValid(strategy))
            strategy.RefreshAll();
        else
            ui.CreateUI(StrategyPath);
    }
    #endregion

    #region F6 单跑预览（假数据，不读项目单例）
    private void ShowPreview()
    {
        hpLabel.Text = "100";
        armorLabel.Text = "120";
        staminaLabel.Text = "25";
        bagLabel.Text = "1/20";
        titleLabel.Text = "渔网网咖";
        SpawnLayerDots(4, 2);
        roomNameLabel.Text = "网吧大厅";
        roomLeftLabel.Text = "8";
        noiseBar.Value = 80;
        noiseLabel.Text = "80%";

        eventImage.Texture = GD.Load<Texture2D>("res://Assets/Images/UI/Explore/material_image.png");
        desLabel.Text = "一堆散落的木板和碎石堵在通道中。\n交错的钉子让搬运变得麻烦，\n但里面可能还藏着可用的材料。\n你打算如何处理这堆杂物？";

        var scene = GD.Load<PackedScene>(OptionPath);
        string[] texts = { "搬开杂物，回收可用材料", "分析结构，寻找省力办法", "不再停留，离开这里" };
        bool[] enough = { true, false, true };
        bool[] isExit = { false, false, true };
        string[] icons = { "strong.png", "brain.png", "" };
        for (int i = 0; i < 3; i++)
        {
            Texture2D tex = icons[i] == "" ? null
                : GD.Load<Texture2D>("res://Assets/Images/UI/" + icons[i]);
            var bar = scene.Instantiate<EventOptionBar>();
            optionBox.AddChild(bar);
            bar.Setup(texts[i], enough[i], isExit[i], tex, 10, () => { });
        }

        var cola = GD.Load<Texture2D>("res://Assets/Images/Items/cola_icon.png");
        SpawnFloat(cola, 1, 0);
        SpawnFloat(cola, 1, 1);
    }
    #endregion
}
