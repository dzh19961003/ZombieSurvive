using Godot;
using MyProject;
using System.Threading.Tasks;

/// <summary>
/// 开始菜单：新游戏 / 继续游戏。
/// 由 UIManager 在启动时显示（ShowUI 缓存，关闭后可直接 HideUI）。
/// 没有存档时"继续游戏"按钮直接隐藏（Visible=false）。
/// </summary>
public partial class StartMenu : Control
{
    private Label titleLabel;
    private Button newGameBtn;
    private Button continueBtn;

    public override void _Ready()
    {
        // 不依赖 Export 解析，显式取节点
        titleLabel = GetNode<Label>("TitleLabel");
        newGameBtn = GetNode<Button>("BtnBox/NewGameBtn");
        continueBtn = GetNode<Button>("BtnBox/ContinueBtn");

        newGameBtn.Pressed += OnNewGamePressed;
        continueBtn.Pressed += OnContinuePressed;

        // 延迟到 SaveManager 就绪后同步一次
        CallDeferred(nameof(SyncContinueBtn));
    }

    /// <summary>
    /// 按"有没有存档"决定"继续游戏"按钮显隐。没存档就隐藏，有存档就显示。
    /// </summary>
    public void SyncContinueBtn()
    {
        if (continueBtn == null || SaveManager.Instance == null) return;
        continueBtn.Visible = SaveManager.Instance.HasSaveFile();
    }

    private void OnNewGamePressed()
    {
        // 有存档 → 弹确认；没存档 → 直接进
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveFile())
        {
            var tips = UIManager.Instance.ShowCommonTips("新游戏", "是否放弃当前游戏并开启新的游戏？");
            tips.OnConfirm = StartNewGame;
            // 取消：留在开始菜单（弹窗自己收起）
        }
        else
        {
            StartNewGame();
        }
    }

    private void StartNewGame()
    {
        // 先删本地存档文件，避免下次"继续游戏"读到旧档
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSaveFile();
        }

        // 随机一个剧本，并把天数/进度重置
        GameManager.Instance.NewGameStart();

        // 关掉开始菜单，进入游戏主界面
        UIManager.Instance.HideUI(Paths.StartMenu);
        UIManager.Instance.ShowUI(Paths.MainUI);

        // 第一天早上也要弹日记
        OpenDiary();
    }

    private void OnContinuePressed()
    {
        // 兜底：没存档时按钮已隐藏，点不到；万一被点到也拦一下
        if (SaveManager.Instance == null || !SaveManager.Instance.HasSaveFile())
        {
            return;
        }
        ContinueGame();
    }

    private void ContinueGame()
    {
        // 1. 读档：SaveManager 自动分发给 GameManager（天数/剧本）、
        //    PlayerManager、PlantManager 等所有 "Save" 组节点
        SaveManager.Instance.Load();

        // 2. 把 GameManager 重置为"当前天的早晨"（探索流程清空重来）
        GameManager.Instance.RestoreToMorning();

        // 3. 补满体力，和"睡了一夜醒来"的状态一致
        PlayerManager.Instance.SleepRefillAll();

        // 4. 关掉开始菜单，进入主界面
        UIManager.Instance.HideUI(Paths.StartMenu);
        UIManager.Instance.ShowUI(Paths.MainUI);

        // 5. 弹当天日记（逐字打印）
        OpenDiary();
    }

    /// <summary>
    /// 打开日记面板，打印当前天数 + 当前剧本对应的内容。
    /// 先播一段"第X天"黑屏转场（0.5s淡入→停留1s→0.5s淡出），结束后再弹日记。
    /// 没有对应日记时仍会弹出（只是正文为空），避免"漏天"。
    /// </summary>
    public static async void OpenDiary()
    {
        // 1. 播黑屏转场（"第X天"淡入淡出）
        int day = GameManager.Instance.DayCount;
        await ShowDayTransition(day);

        // 2. 回到正常主界面后，停顿 1 秒再弹日记（给玩家一点缓冲）
        await Task.Delay(500);

        // 3. 弹日记
        DiaryUI diary = (DiaryUI)UIManager.Instance.CreateUI(Paths.DiaryUI);
        if (diary == null) return;

        string text = GameManager.Instance.GetDiaryTextForCurrentDay();
        diary.Show(day, text);
    }

    /// <summary>
    /// 黑屏转场：不透明黑屏淡入0.5s（屏幕中央"第X天"同步淡入），停留1s，再淡出0.5s。
    /// 全程挂在一个 CanvasLayer(100) 上保证盖住所有UI；结束后自行销毁。
    /// 16ms一帧的 Task.Delay 控制节奏（项目里 TextTyper 已验证此写法可用）。
    /// </summary>
    private static async Task ShowDayTransition(int day)
    {
        var layer = new CanvasLayer { Layer = 100 };
        UIManager.Instance.AddChild(layer);

        // 全屏黑幕
        var black = new ColorRect
        {
            Color = new Color(0, 0, 0, 1),
            AnchorRight = 1,
            AnchorBottom = 1,
            GrowHorizontal = Control.GrowDirection.Both,
            GrowVertical = Control.GrowDirection.Both,
            MouseFilter = Control.MouseFilterEnum.Stop, // 转场期间挡住下层点击
        };
        black.SetAnchorsPreset(Control.LayoutPreset.FullRect, false);
        black.Modulate = new Color(1, 1, 1, 0); // 先全透明
        layer.AddChild(black);

        // 中央"第X天"
        var label = new Label
        {
            Text = "第" + day + "天",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AnchorRight = 1,
            AnchorBottom = 1,
            GrowHorizontal = Control.GrowDirection.Both,
            GrowVertical = Control.GrowDirection.Both,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        label.SetAnchorsPreset(Control.LayoutPreset.FullRect, false);
        label.AddThemeFontSizeOverride("font_size", 96);
        label.AddThemeColorOverride("font_color", new Color(0.95f, 0.9f, 0.75f, 1));
        label.Modulate = new Color(1, 1, 1, 0); // 先全透明
        layer.AddChild(label);

        // ===== 淡入 0.25s（黑幕和文字同步）=====
        for (int f = 0; f < 15; f++)
        {
            float a = (f + 1) / 15f; // 0→1
            black.Modulate = new Color(1, 1, 1, a);
            label.Modulate = new Color(1, 1, 1, a);
            await Task.Delay(16);
        }
        black.Modulate = new Color(1, 1, 1, 1);
        label.Modulate = new Color(1, 1, 1, 1);

        // ===== 黑幕停留 1s（黑底上"第X天"画面保持 1 秒，再开始淡出）=====
        await Task.Delay(1000);

        // ===== 淡出 0.25s =====
        for (int f = 15; f >= 0; f--)
        {
            float a = f / 15f; // 1→0
            black.Modulate = new Color(1, 1, 1, a);
            label.Modulate = new Color(1, 1, 1, a);
            await Task.Delay(16);
        }

        // 转场结束，销毁挂层
        layer.QueueFree();
    }
}
