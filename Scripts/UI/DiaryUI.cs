using Godot;
using MyProject;
using System;

/// <summary>
/// 日记面板：顶部显示"X日"，正文"点按续写"逐段显示。
/// 第一段自动出现，第二段起点屏幕出下一段；
/// 灰色"点击续写日记"提示紧贴当前最后一段下方（每帧跟随），持续淡入淡出。
/// 由 StartMenu.OpenDiary() 创建并调用 Show()。
/// </summary>
public partial class DiaryUI : Control
{
    private Label dateLabel;
    private VBoxContainer contentBox;
    private Button closeBtn;
    private Label hintLabel;
    private Panel panel;

    // 提示的淡入淡出计时（0~1 三角波，来回变透明）
    private float _hintTimer;

    public override void _Ready()
    {
        dateLabel = GetNode<Label>("Panel/TitleRow/dateLabel");
        contentBox = GetNode<VBoxContainer>("Panel/ContentBox");
        closeBtn = GetNode<Button>("Panel/CloseBtn");
        panel = GetNode<Panel>("Panel");
        // 提示 Label 在 Panel 下（不在 ContentBox 里），自己控制坐标
        hintLabel = GetNode<Label>("Panel/HintLabel");

        closeBtn.Pressed += OnClosePressed;
        hintLabel.Visible = false;
    }

    public override void _Process(double delta)
    {
        // 每帧把 hint 跟随到 ContentBox 最后一段的下方（只在 hint 可见时定位）
        if (!Visible || hintLabel == null || !IsInstanceValid(hintLabel)) return;

        if (hintLabel.Visible)
        {
            // 找 ContentBox 最后一个子节点（段落 Label）
            Node lastPara = null;
            foreach (var child in contentBox.GetChildren())
            {
                lastPara = child;
            }
            if (lastPara is Label lastLabel)
            {
                // 都用 Control.Position / GlobalPosition（Control 继承 CanvasItem）
                float lastH = lastLabel.Size.Y;
                // lastLabel 在 contentBox 内的位置 + contentBox 在 panel 内的位置
                // 直接算出在 panel 本地的 Y
                float lastInPanelY = (lastLabel.GlobalPosition.Y - panel.GlobalPosition.Y) + lastH;
                hintLabel.Position = new Vector2(hintLabel.Position.X, lastInPanelY + 24);
            }
        }

        // 持续淡入淡出（只在 hint 可见时跑）
        if (hintLabel.Visible)
        {
            _hintTimer += (float)delta / 0.8f;
            float t = _hintTimer % 2f;
            float alpha = t < 1f ? 1f - t : t - 1f;
            hintLabel.Modulate = new Color(1, 1, 1, 0.2f + alpha * 0.8f);
        }
        else
        {
            _hintTimer = 0;
        }
    }

    /// <summary>
    /// 显示日记。date 为 1~22，正文按"点按续写"逐段显示：
    /// 第一段自动出，第二段起点屏幕出下一段，提示每帧跟随最后一段下方。
    /// </summary>
    public void Show(int date, string text)
    {
        Visible = true;
        _hintTimer = 0;
        dateLabel.Text = date + "日";

        // 清掉上一轮残留的段落 Label（hintLabel 在 Panel 下，不是 ContentBox 子节点，不会被误删）
        foreach (var child in contentBox.GetChildren())
        {
            contentBox.RemoveChild(child);
            child.QueueFree();
        }

        if (hintLabel != null && IsInstanceValid(hintLabel))
        {
            hintLabel.Visible = false;
            hintLabel.Modulate = new Color(1, 1, 1, 1);
        }

        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        // 点按续写：第一段自动出，第二段起点屏幕
        TextTyper.TypeParagraphsByClick(contentBox, text, hintLabel);
    }

    private void OnClosePressed()
    {
        TextTyper.CancelClickTyping();
        UIManager.Instance.DeleteUI(this);
    }
}
