using Godot;
using MyProject;

public partial class ExploreStrategyUI : Control
{
    // 顶部状态栏（和第三大点 RoomChooseUI 一样）
    [Export] public Label hpLabel;
    [Export] public Label armorLabel;
    [Export] public Label staminaLabel;
    [Export] public Label bagLabel;
    [Export] public Label titleLabel;
    [Export] public HBoxContainer layerDots;

    // 1.房间名称、剩余次数、噪音
    [Export] public Label roomNameLabel;
    [Export] public Label roomLeftLabel;
    [Export] public Label noiseLabel;
    [Export] public TextureProgressBar noiseBar;

    // 5.房间描述、2/3搜索卡、4中断按钮、背景
    [Export] public Label desLabel;
    [Export] public Button quickBtn;
    [Export] public Button carefulBtn;
    [Export] public TextureButton leaveBtn;
    [Export] public TextureRect bgImage;

    // 探索完毕后要切换的节点
    [Export] public Label leaveText;
    [Export] public ColorRect quickMask;
    [Export] public Button quickMaskBtn;
    [Export] public ColorRect carefulMask;
    [Export] public Button carefulMaskBtn;

    // F6 单跑这个场景时，勾选其中一个可以看对应的禁用样式
    [Export] public bool previewFinished = false;
    [Export] public bool previewNoStamina = false;

    private const string IconDir = "res://Assets/Images/UI/Explore/ExploreNew/";
    private const string RoomChoosePath = "res://UI/Explore/RoomChooseUI.tscn";
    private const string ExploreChoosePath = "res://UI/Explore/ExploreChooseUI.tscn";
    private const string FinishedTip = "这里已经没有什么好探索的了。";
    private const string NoStaminaTip = "已经没有体力了，今天先回去休息吧。";

    private static readonly Color NormalColor = new Color(0.91f, 0.89f, 0.78f);
    private static readonly Color WarnColor = new Color(1f, 0.35f, 0.3f);

    private Room currentRoom;
    private bool desTyped = false;
    private bool finished = false;     // 探索次数用完：会改描述框和退出按钮
    private bool noStamina = false;    // 体力归零：只遮住两个搜索卡，别的行为都不变
    private bool blocked = false;      // 上面任意一个成立，两张卡就不可点

    public override void _Ready()
    {
        quickBtn.Pressed += () => DoSearch(1);
        carefulBtn.Pressed += () => DoSearch(2);
        leaveBtn.Pressed += OnLeaveClicked;
        quickMaskBtn.Pressed += OnMaskClicked;
        carefulMaskBtn.Pressed += OnMaskClicked;

        // F6 单跑这个场景时单例都不存在，用假数据把界面画出来
        if (GameManager.Instance == null || PlayerManager.Instance == null || ConfigManager.Instance == null)
        {
            ShowPreview();
            return;
        }
        RefreshAll();
    }

    // 统一刷新入口，后续别的流程可以直接调
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

        RefreshBlockState();

        // 描述只在进界面时打印一次，后面刷新不重复打字；探索完毕时显示固定文案，不打印房间描述
        if (!finished && !desTyped)
        {
            TextTyper.TypeText(desLabel, currentRoom.Des);
            desTyped = true;
        }
    }

    // 每次刷新判定一次：两个搜索卡要不要被遮罩禁用
    // 两种情况：房间探索次数用完（探索完毕）、体力归零
    private void RefreshBlockState()
    {
        var gm = GameManager.Instance;
        var pm = PlayerManager.Instance;
        if (gm == null || pm == null || currentRoom == null) return;

        // 次数用完是「终局」，优先级最高；两者同时成立时按探索完毕处理，避免两套文案打架
        bool nowFinished = gm.GetRoomExploreNum(currentRoom.ID, currentRoom.ExploreNum) <= 0;
        bool nowNoStamina = !nowFinished && pm.ExploreStamina <= 0;

        // 1.遮罩和搜索按钮：两种情况都只是不能点搜索
        bool nowBlocked = nowFinished || nowNoStamina;
        if (nowBlocked != blocked)
        {
            blocked = nowBlocked;
            SetMasksVisible(blocked);
        }

        // 2.描述框和退出按钮：只有「探索完毕」才改，体力归零完全不碰
        if (nowFinished != finished)
        {
            finished = nowFinished;
            ApplyFinishedLook(finished);
        }

        // 3.记下来当前该弹哪句提示（点遮罩时用）
        noStamina = nowNoStamina;
    }

    // 两张卡盖黑色半透明遮罩，遮罩上的透明按钮负责接点击
    private void SetMasksVisible(bool value)
    {
        quickMask.Visible = value;
        quickMaskBtn.Visible = value;
        carefulMask.Visible = value;
        carefulMaskBtn.Visible = value;
        quickBtn.Disabled = value;
        carefulBtn.Disabled = value;
    }

    // 只有「探索次数用完」才切换描述文案和退出按钮文字
    private void ApplyFinishedLook(bool value)
    {
        desLabel.Text = value ? FinishedTip : (currentRoom != null ? currentRoom.Des : "");
        desLabel.AddThemeColorOverride("font_color", value ? Colors.White : NormalColor);
        desLabel.AddThemeFontSizeOverride("font_size", value ? 48 : 40);
        if (value) desTyped = true;   // 别让打字机把固定文案盖掉

        leaveText.Text = value ? "离开区域" : "中断探索";
        leaveText.AddThemeColorOverride("font_color", value ? Colors.White : NormalColor);
    }

    // 点遮罩只弹一条浮动提示，不做任何搜索；体力归零和探索完毕文案不同
    private void OnMaskClicked()
    {
        string tip = noStamina ? NoStaminaTip : FinishedTip;
        UIManager.Instance?.ShowCommonTips2(tip);
    }

    // 刷新 1、2、4、5 四个数值（沿用 RoomChooseUI 的写法）
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

    // 楼层圆点：贴图是128x128，不设缩放会按原尺寸直出变成大球
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

    // type: 1=快速搜索 2=仔细搜索
    private void DoSearch(int type)
    {
        var pm = PlayerManager.Instance;
        var gm = GameManager.Instance;
        if (pm == null || gm == null)   // F6 预览状态，不跑逻辑
        {
            GD.Print("[ExploreStrategyUI] 预览模式，不执行搜索逻辑");
            return;
        }

        // 卡片已被遮罩盖住（次数用完或体力归零）；这里再兜一层，防止别的代码路径绕过遮罩
        if (blocked)
        {
            OnMaskClicked();
            return;
        }

        int cost = type == 1 ? 1 : 2;
        if (!pm.CostExploreStamina(cost))
        {
            UIManager.Instance.ShowCommonTips("体力不足", "当前体力不足，无法继续搜索。");
            return;
        }

        // 一次搜索 = 一次探索，点下去立刻扣掉房间剩余次数
        gm.CostRoomExploreNum(gm.roomID);

        int addNoise = Tools.GetRandomNumber(type == 1 ? Consts.quickNoiseProgress : Consts.carefulNoiseProgress);
        gm.exploreNoise += addNoise;
        if (gm.exploreNoise >= 100)
        {
            gm.exploreNoise -= 100;   // 满100减100，多余部分不保留
            TriggerBattle();
        }

        RefreshNumbers();
        RefreshNoise();
        roomLeftLabel.Text = gm.GetRoomExploreNum(currentRoom.ID, currentRoom.ExploreNum).ToString();

        // 五、探索详情界面：抽事件、逐字打描述、发奖励、生成选项
        // 本界面不销毁，事件链走完删掉详情界面后直接露出来并 RefreshAll
        var ui = UIManager.Instance.CreateUI(ExploreChoosePath);
        if (ui is ExploreChooseUI choose) choose.Init(type, this);
    }

    // 战斗逻辑按约定先空置：只从 room 表读怪物池占位
    private void TriggerBattle()
    {
        GD.Print($"[ExploreStrategyUI] 触发战斗，怪物池 {currentRoom.EnemyPool}（战斗逻辑暂空置）");
    }

    private void OnLeaveClicked()
    {
        if (UIManager.Instance == null) return;

        // 探索完毕：不弹风险提示，直接返回房间选择界面
        if (finished)
        {
            BackToRoomChoose();
            return;
        }

        var gm = GameManager.Instance;
        int noise = gm == null ? 0 : gm.exploreNoise;
        string tip;
        if (noise <= 30) tip = "当前噪音低，撤离时不会引起丧尸注意，是否确定中途撤离？";
        else if (noise <= 60) tip = "当前噪音较高，撤离时可能会引起丧尸注意，是否确定中途撤离？";
        else tip = "当前噪音值高，撤离时会引发丧尸暴动，是否确定中途撤离？";

        CommonTips tips = UIManager.Instance.ShowCommonTips("撤离提示", tip);
        tips.OnConfirm = () =>
        {
            // 低噪音不触发；中噪音按 噪音/100 概率；高噪音必定触发
            bool fight = noise > 60 || (noise > 30 && GD.RandRange(1, 100) <= noise);
            if (fight) TriggerBattle();
            BackToRoomChoose();
        };
    }

    // 中断/探索完毕后的去向：非末层进入下一层，末层保持当前层并标记"从末层房间出来"
    private void BackToRoomChoose()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        Building building = ConfigManager.Instance.buildingDic[gm.currentBuildingID];
        if (gm.exploreLayer >= building.LayerNum)
        {
            // 末层：不再 +1，标记后重建末层房间选择界面，显示"这次已经探索完毕了"全遮罩
            gm.lastLayerFinished = true;
        }
        else
        {
            gm.exploreLayer++;
        }

        UIManager.Instance.DeleteUI(this);
        UIManager.Instance.CreateUI(RoomChoosePath);
    }

    // 以后补了建筑背景图，按 building 表 image 字段丢进 BuildingBg 目录会自动显示；现在保持隐藏
    private void ShowBuildingBg(Building building)
    {
        string path = $"res://Assets/Images/BuildingBg/{building.Image}.png";
        if (!ResourceLoader.Exists(path)) return;
        bgImage.Texture = GD.Load<Texture2D>(path);
        bgImage.Visible = true;
    }

    // F6 单跑预览用的假数据（不读项目单例）
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
        desLabel.Text = "网咖大厅一片寂静，电脑屏幕早已熄灭。桌边散落着一些生活物资，包间的门半掩着。你准备开始搜索，先选择合适的方式。";

        // 单跑预览：勾哪个开关就看哪种禁用样式
        if (previewFinished)
        {
            finished = true;
            blocked = true;
            ApplyFinishedLook(true);
            SetMasksVisible(true);
        }
        else if (previewNoStamina)
        {
            staminaLabel.Text = "0";
            noStamina = true;
            blocked = true;
            SetMasksVisible(true);   // 描述和退出按钮保持正常态不动
        }
    }
}
