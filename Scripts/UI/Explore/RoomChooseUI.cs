using Godot;
using MyProject;
using System.Collections.Generic;

public partial class RoomChooseUI : Control
{
    [Export] public Label hpLabel;        // 1.生命值
    [Export] public Label armorLabel;     // 2.护甲值
    [Export] public Label staminaLabel;   // 4.探索体力
    [Export] public Label bagLabel;       // 5.背包容量
    [Export] public Label titleLabel;     // 3.地点名称
    [Export] public Label hintLabel;      // 7.选择前进方向
    [Export] public Label desLabel;       // 8.建筑描述
    [Export] public HBoxContainer layerDots; // 3.楼层圆点
    [Export] public HBoxContainer roomRow;   // 6.房间卡片
    [Export] public TextureButton leaveBtn;  // 9.离开地点
    [Export] public TextureRect bgImage;     // 建筑背景图（暂时没有资源，先用纯色占位）
    [Export] public TextureButton nextLayerBtn;  // 全部房间探索完后出现「继续前进」
    [Export] public Label nextLayerText;         // 「继续前进」文字

    // F6 单跑这个场景时，勾选其中一个可以看对应的全部探索完样式
    [Export] public bool previewAllFinished = false;  // 非末层：显示「继续前进」
    [Export] public bool previewLastLayer = false;    // 末层：全遮罩只能离开

    private const string IconDir = "res://Assets/Images/UI/Explore/ExploreNew/";
    private const string CardPath = "res://UI/Explore/roomCard.tscn";
    private const string RoomChoosePath = "res://UI/Explore/RoomChooseUI.tscn";

    // 单张房间探索完的提示 / 末层全部探索完的提示
    private const string RoomFinishedTip = "这里已经没有什么好探索的了。";
    private const string LastLayerTip = "这次已经探索完毕了，下次再来吧。";

    // 正常是米色，背包种类超出上限时前面的数字变红
    private static readonly Color NormalColor = new Color(0.91f, 0.89f, 0.78f);
    private static readonly Color WarnColor = new Color(1f, 0.35f, 0.3f);

    // 描述文字只在进入界面时打印一次，后面刷新不重复打字
    private bool desTyped = false;
    // 末层全遮罩状态：只能点「离开地点」，点卡片弹「这次已经探索完毕了」
    private bool lastLayerMasked = false;

    public override void _Ready()
    {
        leaveBtn.Pressed += OnLeaveClicked;
        nextLayerBtn.Pressed += OnNextLayerClicked;
        StartBreath();

        // F6 单独跑这个场景时单例都不存在，用一份假数据把界面画出来，方便直接预览布局
        if (GameManager.Instance == null || PlayerManager.Instance == null || ConfigManager.Instance == null)
        {
            ShowPreview();
            return;
        }
        RefreshAll();
    }

    // ── 统一刷新入口：1/2/4/5 数值 + 3 标题楼层 + 6 房间卡 + 8 描述 ──
    public void RefreshAll()
    {
        RefreshNumbers();

        Building building = ConfigManager.Instance.buildingDic[GameManager.Instance.currentBuildingID];
        titleLabel.Text = building.Name;
        SpawnLayerDots(building.LayerNum, GameManager.Instance.exploreLayer);
        SpawnRooms(building, GameManager.Instance.exploreLayer);
        ShowBuildingBg(building);
        CheckLayerState(building);

        if (!desTyped)
        {
            TextTyper.TypeText(desLabel, building.EnterDes);
            desTyped = true;
        }
    }

    // 判定当前层是否所有房间都探索完了：
    //  - 非末层：全部为0 → 显示「继续前进」按钮（点卡片仍弹"这里已没有什么好探索的了"）
    //  - 末层：全部为0 或 从末层房间出来（GM.lastLayerFinished）→ 全遮罩，只能点「离开地点」
    private void CheckLayerState(Building building)
    {
        var gm = GameManager.Instance;
        bool allFinished = true;
        foreach (int id in building.RoomID)
        {
            Room r = ConfigManager.Instance.roomDic[id];
            if (r.RoomLayer != gm.exploreLayer) continue;
            if (gm.GetRoomExploreNum(r.ID, r.ExploreNum) > 0) { allFinished = false; break; }
        }

        bool isLastLayer = gm.exploreLayer >= building.LayerNum;
        bool showNext = allFinished && !isLastLayer;
        bool maskAll = isLastLayer && (allFinished || gm.lastLayerFinished);

        // 探索完时隐藏「选择前进方向」（非末层被「继续前进」替代，末层只剩「离开地点」）
        hintLabel.Visible = !showNext && !maskAll;
        nextLayerBtn.Visible = showNext;
        nextLayerText.Visible = showNext;
        SetLastLayerMasked(maskAll);
    }

    // 末层全遮罩：点卡片只弹提示不进入探索；点「离开地点」直接离开不弹确认
    private void SetLastLayerMasked(bool value)
    {
        lastLayerMasked = value;

        // 从末层房间出来时，即使该层还有没搜完的房间，也强制盖住所有卡片（用户拍板：出来就全遮罩）
        foreach (Node n in roomRow.GetChildren())
            if (n is RoomCard c) c.mask.Visible = value || c.IsFinished;

        if (value)
        {
            desLabel.Text = LastLayerTip;
            desTyped = true;   // 别让打字机把固定文案盖掉
        }
    }

    // 以后补了建筑背景图，按 building 表的 image 字段命名丢进 BuildingBg 目录就会自动显示
    // 现在没有对应资源，保持隐藏，用纯色占位
    private void ShowBuildingBg(Building building)
    {
        string path = $"res://Assets/Images/BuildingBg/{building.Image}.png";
        if (!ResourceLoader.Exists(path)) return;

        bgImage.Texture = GD.Load<Texture2D>(path);
        bgImage.Visible = true;
    }

    // 只刷新策划案里的 1、2、4、5 四个数值，后面别的流程可以直接调
    public void RefreshNumbers()
    {
        var pm = PlayerManager.Instance;
        var gm = GameManager.Instance;

        hpLabel.Text = pm.Hp.ToString();
        armorLabel.Text = pm.Armor.ToString();
        staminaLabel.Text = pm.ExploreStamina.ToString();

        int kinds = gm.BagItemDic.Count;
        int cap = pm.BagNum;
        bagLabel.Text = $"{kinds}/{cap}";
        bagLabel.AddThemeColorOverride("font_color", kinds > cap ? WarnColor : NormalColor);
    }

    // 楼层圆点：共 layerNum 个，实心个数 = 当前层，其余空心
    private void SpawnLayerDots(int total, int current)
    {
        foreach (Node n in layerDots.GetChildren()) n.QueueFree();

        for (int i = 1; i <= total; i++)
        {
            string file = i <= current ? "progressIn.png" : "progressOut.png";
            var dot = new TextureRect();
            dot.Texture = GD.Load<Texture2D>(IconDir + file);
            // 原图是128x128，不设这两行会按原尺寸直出，圆点会变成96px的大球
            dot.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            dot.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            dot.CustomMinimumSize = new Vector2(30, 30);
            layerDots.AddChild(dot);
        }
    }

    // 生成当前层的所有房间卡片（每层若干个，玩家选一个）
    private void SpawnRooms(Building building, int layer)
    {
        foreach (Node n in roomRow.GetChildren()) n.QueueFree();

        var rooms = new List<Room>();
        foreach (int id in building.RoomID)
        {
            Room r = ConfigManager.Instance.roomDic[id];
            if (r.RoomLayer == layer) rooms.Add(r);
        }

        var cardScene = GD.Load<PackedScene>(CardPath);
        foreach (Room r in rooms)
        {
            RoomCard card = cardScene.Instantiate<RoomCard>();
            roomRow.AddChild(card);
            int left = GameManager.Instance.GetRoomExploreNum(r.ID, r.ExploreNum);
            card.Setup(r, left);
            card.OnCardClicked = EnterRoom;
        }
    }

    // 选中房间：记到 GM 后打开四、探索策略界面
    // 本界面先销毁，中断探索或探索结束时会重新创建下一层的房间选择界面
    private void EnterRoom(int roomID)
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        // 末层全遮罩：点任何卡片只弹提示，不进入探索
        if (lastLayerMasked)
        {
            UIManager.Instance?.ShowCommonTips2(LastLayerTip);
            return;
        }

        // 单张卡片已探索完：点它弹"这里已没有什么好探索的了"，也不进入探索
        RoomCard target = null;
        foreach (Node n in roomRow.GetChildren())
            if (n is RoomCard c && c.RoomID == roomID) { target = c; break; }
        if (target != null && target.IsFinished)
        {
            UIManager.Instance?.ShowCommonTips2(RoomFinishedTip);
            return;
        }

        gm.roomID = roomID;
        gm.exploreState = 1;
        UIManager.Instance.CreateUI("res://UI/Explore/ExploreStrategyUI.tscn");
        UIManager.Instance.DeleteUI(this);
    }

    // 非末层全部房间探索完后出现「继续前进」：进入下一层房间选择
    private void OnNextLayerClicked()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        gm.exploreLayer++;
        UIManager.Instance.DeleteUI(this);
        UIManager.Instance.CreateUI(RoomChoosePath);
    }

    // 9.离开地点：末层全遮罩时直接离开不弹确认，否则先确认
    private void OnLeaveClicked()
    {
        if (UIManager.Instance == null) return;

        // 末层全遮罩：探索已全部完毕，直接关闭回主场景
        if (lastLayerMasked)
        {
            UIManager.Instance.DeleteUI(this);
            return;
        }

        CommonTips tips = UIManager.Instance.ShowCommonTips("离开地点", $"是否要离开{titleLabel.Text}？");
        tips.OnConfirm = () => UIManager.Instance.DeleteUI(this);
    }

    // 7.「选择前进方向」呼吸动画：1秒渐出 + 1秒渐入，一直循环
    private void StartBreath()
    {
        Tween tw = CreateTween().SetLoops();
        tw.TweenProperty(hintLabel, "modulate:a", 0.3f, 1.0f);
        tw.TweenProperty(hintLabel, "modulate:a", 1.0f, 1.0f);
    }

    // F6 单跑预览用的假数据（不读项目单例）
    private void ShowPreview()
    {
        hpLabel.Text = "100";
        armorLabel.Text = "120";
        staminaLabel.Text = "25";
        bagLabel.Text = "1/20";
        titleLabel.Text = "渔网网咖";
        SpawnLayerDots(3, 1);
        desLabel.Text = "网咖大厅一片寂静，电脑屏幕早已熄灭。桌边散落着一些生活物资，包间的门半掩着。你需要选择下一处搜索的房间。";

        foreach (Node n in roomRow.GetChildren()) n.QueueFree();
        var previewRooms = new List<Room>
        {
            new Room { ID = 9,  Name = "大厅吧台", ExploreNum = 8,  Food = 2, Equip = 1 },
            new Room { ID = 10, Name = "散座区",   ExploreNum = 16, Food = 2, Equip = 1, Material = 1 }
        };

        var cardScene = GD.Load<PackedScene>(CardPath);
        int[] leftNums = { 8, 12 };
        for (int i = 0; i < previewRooms.Count; i++)
        {
            RoomCard card = cardScene.Instantiate<RoomCard>();
            roomRow.AddChild(card);
            card.Setup(previewRooms[i], leftNums[i]);
            card.OnCardClicked = EnterRoom;
        }

        // 单跑预览：勾哪个开关就看哪种全部探索完样式
        if (previewAllFinished || previewLastLayer)
        {
            int idx = 0;
            foreach (Node n in roomRow.GetChildren())
                if (n is RoomCard c) c.Setup(previewRooms[idx++ % previewRooms.Count], 0);

            if (previewAllFinished)
            {
                // 非末层：全部剩余0 → 显示「继续前进」按钮
                nextLayerBtn.Visible = true;
                nextLayerText.Visible = true;
            }
            else
            {
                // 末层：全遮罩，只能离开
                SetLastLayerMasked(true);
            }
        }
    }
}
