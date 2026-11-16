using Godot;
using MyProject;
using System;

public partial class CommonTips : Control
{
	[Export] public Button closeBtn;
    [Export] public BaseButton confirmBtn;
    [Export] public BaseButton cancelBtn;
    [Export] public Label titleLabel;
    [Export] public Label tipsLabel;
    [Export] public ScrollContainer bodyScroll;

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
        closeBtn.Pressed += HideDialog;
        confirmBtn.Pressed += () =>
        {
            Action callback = OnConfirm;
            HideDialog();
            callback?.Invoke();
        };
        cancelBtn.Pressed += () =>
        {
            Action callback = OnCancel;
            HideDialog();
            callback?.Invoke();
        };
        // 背包按钮：不关闭弹窗，单独触发 OnBag
        if (bagBtn != null)
        {
            bagBtn.Pressed += () => OnBag?.Invoke();
        }
        confirmBtn.CallDeferred("grab_focus");
    }

    private void HideDialog()
    {
        UIManager.Instance?.HideUI("res://UI/CommonTips.tscn");
        Hide();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (IsVisibleInTree() && @event.IsActionPressed("ui_cancel"))
        {
            HideDialog();
            GetViewport().SetInputAsHandled();
        }
    }

    public void SpawnTips(string title,string tips)
    {
        // ShowUI 会复用弹窗；新一轮提示不能沿用上一次确认/取消/背包的操作。
        OnConfirm = null;
        OnCancel = null;
        OnBag = null;
        titleLabel.Text = title ?? string.Empty;
        tipsLabel.Text = tips ?? string.Empty;
        if (bodyScroll != null)
        {
            // 正文高度交给容器计算，新提示回到开头。
            bodyScroll.ScrollVertical = 0;
            bodyScroll.QueueSort();
        }
        // 每次打开默认隐藏背包按钮（需要时由调用方显式开启）
        SetBagBtnVisible(false);
        confirmBtn.CallDeferred("grab_focus");
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
