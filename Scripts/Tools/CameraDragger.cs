using Godot;

namespace MyProject
{
    // 挂在 Camera2D 上：手指/鼠标拖动时移动相机，靠 Camera2D 的 Limit 自动卡边界。
    // 用 _UnhandledInput：点 UI 按钮时事件被按钮吃掉，不会触发拖动；点空白才拖。
    public partial class CameraDragger : Camera2D
    {
        private bool _dragging;
        private Vector2 _startScreen;   // 按下时屏幕坐标
        private Vector2 _startCamera;   // 按下时相机坐标

        public override void _UnhandledInput(InputEvent @event)
        {
            // 触摸（手机）
            if (@event is InputEventScreenTouch touch)
            {
                if (touch.Pressed) BeginDrag(touch.Position);
                else EndDrag();
            }
            else if (@event is InputEventScreenDrag drag && _dragging)
            {
                UpdateDrag(drag.Position);
            }
            // 鼠标左键（PC 测试）
            else if (@event is InputEventMouseButton mouse && mouse.ButtonIndex == MouseButton.Left)
            {
                if (mouse.Pressed) BeginDrag(mouse.Position);
                else EndDrag();
            }
            else if (@event is InputEventMouseMotion motion && _dragging)
            {
                UpdateDrag(motion.Position);
            }
        }

        private void BeginDrag(Vector2 screenPos)
        {
            _dragging = true;
            _startScreen = screenPos;
            _startCamera = GlobalPosition;
        }

        private void UpdateDrag(Vector2 screenPos)
        {
            // offset = 起点 - 当前点：手指往下(y增) → 相机往上(y减)，看到上方内容
            Vector2 offset = _startScreen - screenPos;
            GlobalPosition = _startCamera + offset;
            // Camera2D 的 Limit 会自动把相机夹在边界内，不用自己 clamp
        }

        private void EndDrag()
        {
            _dragging = false;
        }
    }
}
