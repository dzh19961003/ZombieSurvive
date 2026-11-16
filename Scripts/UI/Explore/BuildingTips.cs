using Godot;
using MyProject;

public partial class BuildingTips : Control
{
    private const string ScenePath = "res://UI/Explore/BuildingTips.tscn";
    private static readonly Color SupplyFull = new Color(0.29f, 0.70f, 0.91f);
    private static readonly Color SupplyEmpty = new Color(0.11f, 0.27f, 0.36f);

    [Export] private Label nameLabel;
    [Export] private HBoxContainer star;
    [Export] private Label desLabel;
    [Export] private BaseButton confirmBtn;
    [Export] private Button cancelBtn;
    [Export] private HBoxContainer foodStars;
    [Export] private HBoxContainer medicStars;
    [Export] private HBoxContainer equipStars;
    [Export] private HBoxContainer materialStars;
    [Export] private ProgressBar progressBar;
    [Export] private Label progressLabel;
    [Export] private ScrollContainer descriptionScroll;
    [Export(PropertyHint.Range, "0,100,1")] public int previewProgress = 90;

    private int buildingID;

    public override void _Ready()
    {
        cancelBtn.Pressed += Close;
        confirmBtn.Pressed += Confirm;
        VisibilityChanged += () =>
        {
            if (IsVisibleInTree() && buildingID > 0) RefreshProgress();
        };

        // 单独运行场景时保留可检查的示例；游戏内由 InitialTips 绑定真实建筑。
        if (ConfigManager.Instance == null || GameManager.Instance == null)
        {
            nameLabel.Text = "实验小学";
            desLabel.Text = "旧日的教学楼依然矗立，教室与库房中可能留有可用物资，探索时请保持警惕。";
            SetDifficulty(5);
            SetSupply(foodStars, 5);
            SetSupply(medicStars, 5);
            SetSupply(equipStars, 5);
            SetSupply(materialStars, 5);
            SetProgress(previewProgress);
        }
    }

    public void InitialTips(int ID)
    {
        var config = ConfigManager.Instance;
        if (config == null || !config.buildingDic.TryGetValue(ID, out Building building)) return;

        buildingID = ID;
        nameLabel.Text = building.Name;
        desLabel.Text = building.Des;
        descriptionScroll.ScrollVertical = 0;
        SetDifficulty(building.Stars);
        SetSupply(foodStars, building.Food);
        SetSupply(medicStars, building.Medic);
        SetSupply(equipStars, building.Equip);
        SetSupply(materialStars, building.Material);
        RefreshProgress();
    }

    public void RefreshProgress()
    {
        SetProgress(GameManager.Instance?.GetBuildingExploreProgress(buildingID) ?? 0);
    }

    private void SetProgress(int progress)
    {
        int value = Mathf.Clamp(progress, 0, 100);
        progressBar.Value = value;
        progressLabel.Text = $"{value}%";
    }

    private void SetDifficulty(int rating)
    {
        int value = Mathf.Clamp(rating, 0, 5);
        for (int i = 0; i < star.GetChildCount(); i++)
            ((TextureRect)star.GetChild(i)).Modulate = new Color(1, 1, 1, i < value ? 1f : 0.16f);
    }

    private static void SetSupply(HBoxContainer row, int rating)
    {
        int value = Mathf.Clamp(rating, 0, 5);
        for (int i = 0; i < row.GetChildCount(); i++)
            ((ColorRect)row.GetChild(i)).Color = i < value ? SupplyFull : SupplyEmpty;
    }

    private void Close()
    {
        if (UIManager.Instance != null) UIManager.Instance.HideUI(ScenePath);
        else Hide();
    }

    private void Confirm()
    {
        var game = GameManager.Instance;
        var ui = UIManager.Instance;
        if (game == null || ui == null || buildingID <= 0) return;

        if (game.CurrentTimePeriod == 1)
        {
            EnterExplore();
            game.AdvanceTime();
        }
        else if (game.CurrentTimePeriod == 2)
        {
            var tips = (CommonTips)ui.ShowCommonTips("探索提示", "确定进入今天的第二次探索吗？临近黄昏丧尸的能力会显著提升");
            tips.OnConfirm = () =>
            {
                EnterExplore();
                game.AdvanceTime();
            };
        }
        else
        {
            ui.ShowCommonTips2("今天已经太晚了，明天再来继续探索吧");
        }
    }

    public void EnterExplore()
    {
        var game = GameManager.Instance;
        var ui = UIManager.Instance;
        if (game == null || ui == null || buildingID <= 0) return;

        ui.HideUI(ScenePath);
        game.currentBuildingID = buildingID;
        game.exploreState = 1;
        game.exploreLayer = 1;
        game.lastLayerFinished = false;
        var roomChoose = (RoomChooseUI)ui.CreateUI("res://UI/Explore/RoomChooseUI.tscn");
        roomChoose.RefreshAll();
    }
}
