// ============================================================
//  PropertyUi — 属性面板（状态显示）
// ============================================================

using Godot;
using Godot.Collections;  // ← 添加这一行
using MyProject;

namespace MyProject
{
    public partial class PropertyUi : Control
    {
        [Export] public Button close_button;
        [Export] public GridContainer StateAreas;

        public override void _Ready()
        {
            close_button.Pressed += OnCloseButtonPressed;

            // 从 PlayerManager 获取状态ID数组，生成标签
            SpawnStateList(PlayerManager.Instance.stateArray);
        }

        private void OnCloseButtonPressed()
        {
            UIManager.Instance.HideUI(Paths.PropertyUI);
        }

        /// <summary>
        /// 接收 Array 参数，循环生成 state 标签
        /// </summary>
        public void SpawnStateList(Array<int> stateIDArray)
        {
            for (int i = StateAreas.GetChildCount() - 1; i >= 0; i--)
            {
                StateAreas.GetChild(i).QueueFree();
            }

            for (int i = 0; i < stateIDArray.Count; i++)
            {
                var stateScene = GD.Load<PackedScene>("res://UI/Buff.tscn");
                Buff buff = stateScene.Instantiate<Buff>();

                // 先设置 ID，再加入场景树
                buff.ID = stateIDArray[i];

                StateAreas.AddChild(buff);
            }
        }
    }
}