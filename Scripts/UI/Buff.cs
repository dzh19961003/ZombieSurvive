// ============================================================
//  Buff — 单个状态条的显示组件
// ============================================================

using Godot;
using MyProject;

public partial class Buff : Control
{
    public int ID;

    [Export] public TextureRect PosIcon;
    [Export] public TextureButton Bg;
    [Export] public Label StateName;
    [Export] public Label TimeLen;

    public override void _Ready()
    {
        Bg.Pressed += OnBuffClick;

        // 如果 ID 已设置，立即初始化
        if (ID != 0)
        {
            InitialState();
        }
    }

    public void InitialState()
    {
        if (ID == 0)
        {
            SetEmpty();
            return;
        }

        // 防御性检查
        if (ConfigManager.Instance == null)
        {
            GD.PrintErr($"[Buff] ConfigManager 为 null！ID={ID}");
            return;
        }

        if (!ConfigManager.Instance.stateDic.ContainsKey(ID))
        {
            GD.PrintErr($"[Buff] stateDic 中找不到 ID={ID}！");
            return;
        }

        State state = ConfigManager.Instance.stateDic[ID];

        // 设置文本
        StateName.Text = state.Name;
        TimeLen.Text = state.Time + "天";

        // 根据正面/负面设置样式
        if (state.Positive == 1)
        {
            PosIcon.FlipH = false;
            StateName.AddThemeColorOverride("font_color", Colors.Green);
        }
        else
        {
            PosIcon.FlipH = true;
            StateName.AddThemeColorOverride("font_color", Colors.Red);
        }

        // 显示所有节点
        PosIcon.Visible = true;
        Bg.Visible = true;
        StateName.Visible = true;
        TimeLen.Visible = true;

        GD.Print($"[Buff] 初始化完成：ID={ID}, Name={state.Name}");
    }

    private void SetEmpty()
    {
        PosIcon.Visible = false;
        Bg.Visible = false;
        StateName.Visible = false;
        TimeLen.Visible = false;
    }

    private void OnBuffClick()
    {
        if (ID == 0) return;
        State state = ConfigManager.Instance.stateDic[ID];
        UIManager.Instance.ShowCommonTips(state.Name, state.Effect);
    }
}