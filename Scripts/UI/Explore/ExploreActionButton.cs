using Godot;

// 两个探索界面共用的整卡按钮：原生皮肤状态 + 轻微下压，点击范围保持固定。
public partial class ExploreActionButton : Button
{
    [Export] public Control content;
    [Export] public CanvasItem blockedOverlay;

    public override void _Ready()
    {
        if (blockedOverlay != null) blockedOverlay.VisibilityChanged += QueueRedraw;
    }

    public override void _ExitTree()
    {
        if (GodotObject.IsInstanceValid(blockedOverlay))
            blockedOverlay.VisibilityChanged -= QueueRedraw;
    }

    public override void _Draw()
    {
        bool blocked = Disabled || (blockedOverlay != null && blockedOverlay.Visible);
        var mode = GetDrawMode();
        bool pressed = !blocked && (mode == DrawMode.Pressed || mode == DrawMode.HoverPressed);
        if (content != null) content.Position = new Vector2(0, pressed ? 2 : 0);

        // 已探索完的房间仍可点击弹提示，但不显示可操作的悬停/按下反馈。
        if (blocked) DrawStyleBox(GetThemeStylebox("disabled", "Button"), new Rect2(Vector2.Zero, Size));
    }
}
