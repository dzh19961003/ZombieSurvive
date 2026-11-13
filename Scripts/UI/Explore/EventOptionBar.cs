using Godot;
using System;

// 探索详情界面里的一条选项（eventAble / eventDisable 两种底图）
public partial class EventOptionBar : Control
{
    [Export] public TextureRect bg;
    [Export] public Label textLabel;
    [Export] public TextureRect needIcon;
    [Export] public Label needNum;
    [Export] public TextureRect exitIcon;
    [Export] public Button button;

    private const string IconDir = "res://Assets/Images/UI/Explore/ExploreNew/";

    // demandTex = 需求图标（null 表示这个选项没有需求）
    // showExit  = nextEvent 是 9998/9999 时，右侧改显示橙色退出门
    public void Setup(string text, bool enough, bool showExit, Texture2D demandTex, int num, Action onClick)
    {
        textLabel.Text = text;

        // 未达标：换成灰底 + 整行变暗，按钮禁用
        bg.Texture = GD.Load<Texture2D>(IconDir + (enough ? "eventAble.png" : "eventDisable.png"));
        Modulate = enough ? Colors.White : new Color(0.55f, 0.58f, 0.62f, 1f);

        needIcon.Visible = demandTex != null && !showExit;
        needNum.Visible = demandTex != null && !showExit;
        if (demandTex != null && !showExit)
        {
            needIcon.Texture = demandTex;
            needNum.Text = num.ToString();
            // 达标绿字、未达标红字（和设计图一致）
            needNum.AddThemeColorOverride("font_color",
                enough ? new Color(0.55f, 0.85f, 0.5f, 1f) : new Color(0.9f, 0.4f, 0.4f, 1f));
        }

        exitIcon.Visible = showExit;
        if (showExit) exitIcon.Texture = GD.Load<Texture2D>(IconDir + "leave.png");

        button.Disabled = !enough;
        button.Pressed += () => onClick?.Invoke();
    }
}
