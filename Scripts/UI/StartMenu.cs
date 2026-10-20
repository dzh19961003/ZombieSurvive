using Godot;
using MyProject;

/// <summary>
/// 开始菜单：新游戏 / 继续游戏。
/// 由 UIManager 在启动时显示（ShowUI 缓存，关闭后可直接 HideUI）。
/// </summary>
public partial class StartMenu : Control
{
    private Label titleLabel;
    private Button newGameBtn;
    private Button continueBtn;

    public override void _Ready()
    {
        // 不依赖 Export 解析，显式取节点，避免跨实例化时 Export 没生效导致 null
        titleLabel = GetNode<Label>("TitleLabel");
        newGameBtn = GetNode<Button>("BtnBox/NewGameBtn");
        continueBtn = GetNode<Button>("BtnBox/ContinueBtn");

        newGameBtn.Pressed += OnNewGamePressed;

        // 继续游戏：读取存档 → 恢复"当前天的早上"
        continueBtn.Pressed += OnContinuePressed;

        // 没有存档时，把"继续游戏"置灰禁用
        RefreshContinueBtn();
    }

    /// <summary>
    /// 根据有没有存档，决定"继续游戏"按钮是否显示。
    /// 没有存档 → 隐藏按钮；有存档 → 显示。
    /// 在 _Ready 时调用一次；新游戏覆盖存档后也可再调一次刷新。
    /// </summary>
    public void RefreshContinueBtn()
    {
        if (continueBtn == null || SaveManager.Instance == null) return;
        bool hasSave = SaveManager.Instance.HasSaveFile();
        continueBtn.Visible = hasSave;
    }

    private void OnNewGamePressed()
    {
        // 有存档 → 弹确认；没存档 → 直接进
        if (SaveManager.Instance.HasSaveFile())
        {
            var tips = UIManager.Instance.ShowCommonTips("新游戏", "是否放弃当前游戏并开启新的游戏？");
            tips.OnConfirm = StartNewGame;
            // 取消：什么都不做，留在开始菜单（弹窗自己会 HideUI 收起）
        }
        else
        {
            StartNewGame();
        }
    }

    private void StartNewGame()
    {
        // 先删本地存档文件，避免下次"继续游戏"读到旧档
        SaveManager.Instance.DeleteSaveFile();

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
        // 兜底：理论上没存档时按钮已被隐藏，点不到；万一被点到也拦一下
        if (!SaveManager.Instance.HasSaveFile())
        {
            return;
        }
        ContinueGame();
    }

    private void ContinueGame()
    {
        // 1. 读档：SaveManager 会自动把数据分发给 GameManager（天数/剧本）、
        //    PlayerManager、PlantManager 等所有 "Save" 组节点
        SaveManager.Instance.Load();

        // 2. 把 GameManager 重置为"当前天的早晨"（探索流程清空重来）
        GameManager.Instance.RestoreToMorning();

        // 3. 补满体力，和"睡了一夜醒来"的状态一致
        //    注意：PlayerManager.timePeriodsElapsed 已从存档移除，读档后自动为 0，
        //    无需额外清零。
        PlayerManager.Instance.SleepRefillAll();

        // 4. 关掉开始菜单，进入主界面
        UIManager.Instance.HideUI(Paths.StartMenu);
        UIManager.Instance.ShowUI(Paths.MainUI);

        // 5. 弹当天日记（逐字打印）
        OpenDiary();
    }

    /// <summary>
    /// 打开日记面板，打印当前天数 + 当前剧本对应的内容。
    /// 没有对应日记时仍会弹出（只是正文为空），避免"漏天"。
    /// </summary>
    public static void OpenDiary()
    {
        DiaryUI diary = (DiaryUI)UIManager.Instance.CreateUI(Paths.DiaryUI);
        if (diary == null) return;

        int day = GameManager.Instance.DayCount;
        string text = GameManager.Instance.GetDiaryTextForCurrentDay();
        diary.Show(day, text);
    }
}
