using Godot;
using MyProject;
using System;

public partial class CommonTips : Control
{
	[Export] public Button closeBtn;
    [Export] public TextureButton confirmBtn;
    [Export] public TextureButton cancelBtn;
    [Export] public Label titleLabel;
    [Export] public Label tipsLabel;

    // ─────────────────────────────────────────────────────────
    //  外部回调：每次显示弹窗时用 = 赋值（覆盖），不要用 += 累加。
    //  这样按钮的 Pressed 信号只在 _Ready() 里连接一次，
    //  不会出现“信号已连接”的重复连接错误。
    // ─────────────────────────────────────────────────────────
    public Action OnConfirm;   // 点击“确认”按钮时执行
    public Action OnCancel;    // 点击“取消”按钮时执行
    public Action OnBag;       // 点击“背包”按钮时执行（外出探索准备用）

    [Export] public TextureButton bagBtn;

    public override void _Ready()
	{
        // 三个按钮都关闭弹窗；确认/取消额外触发对应回调
        closeBtn.Pressed += () => UIManager.Instance.HideUI("res://UI/CommonTips.tscn");
        confirmBtn.Pressed += () =>
        {
            UIManager.Instance.HideUI("res://UI/CommonTips.tscn");
            OnConfirm?.Invoke();
        };
        cancelBtn.Pressed += () =>
        {
            UIManager.Instance.HideUI("res://UI/CommonTips.tscn");
            OnCancel?.Invoke();
        };
        // 背包按钮：不关闭弹窗，单独触发 OnBag
        if (bagBtn != null)
        {
            bagBtn.Pressed += () => OnBag?.Invoke();
        }
    }

    public void SpawnTips(string title,string tips)
    {
        titleLabel.Text = title;
        tipsLabel.Text = tips;
        // 每次打开默认隐藏背包按钮（需要时由调用方显式开启）
        SetBagBtnVisible(false);
    }

    // 控制右下角背包按钮是否显示（默认隐藏，只在外出探索等需要装物品的确认框打开）
    public void SetBagBtnVisible(bool visible)
    {
        if (bagBtn != null)
        {
            bagBtn.Visible = visible;
        }
    }

}
