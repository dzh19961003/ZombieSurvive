// ============================================================
//  PropertyUi — 属性面板（状态 + 天赋显示）
// ============================================================

using Godot;
using Godot.Collections; 
using MyProject;


namespace MyProject
{
    public partial class PropertyUi : Control
    {
        [Export] public Button close_button;
        [Export] public GridContainer StateAreas;
        [Export] public GridContainer TalentAreas;

        public override void _Ready()
        {
            // 如果 Export 没在编辑器里赋值（Godot 重新保存场景时可能清掉），
            // 按路径自动查找，避免 NullReferenceException
            if (TalentAreas == null)
                TalentAreas = GetNodeOrNull<GridContainer>("Traits/TraitAreas");

            close_button.Pressed += OnCloseButtonPressed;

            // 从 PlayerManager 获取状态ID数组，生成标签
            SpawnStateList(PlayerManager.Instance.stateArray);

            // 从 PlayerManager 获取天赋ID数组，生成天赋标签
            SpawnTalentList(PlayerManager.Instance.talentID);
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
            // 1. 清空容器里旧的 Buff 节点
            for (int i = StateAreas.GetChildCount() - 1; i >= 0; i--)
            {
                StateAreas.GetChild(i).QueueFree();
            }

            // 2. 加载 Buff 预制体
            var stateScene = GD.Load<PackedScene>("res://UI/Buff.tscn");

            // 3. 循环创建 Buff 实例
            for (int i = 0; i < stateIDArray.Count; i++)
            {
                Buff buff = stateScene.Instantiate<Buff>();

                // Buff 的 _Ready() 会检查 ID，所以必须在 AddChild 之前设置
                buff.ID = stateIDArray[i];

                StateAreas.AddChild(buff);
            }

            // 4. GridContainer 动态添加子节点后必须刷新布局
            StateAreas.UpdateMinimumSize();
        }

        /// <summary>
        /// 接收 Array 参数，循环生成 talent 标签
        /// </summary>
        public void SpawnTalentList(Array<int> talentID)
        {
            // 防御性检查：容器是否存在
            if (TalentAreas == null)
            {
                GD.PrintErr("[PropertyUi] TalentAreas 为 null！请在编辑器中设置 Export 或检查节点路径。");
                return;
            }

            // 1. 清空容器里旧的 Trait 节点
            for (int i = TalentAreas.GetChildCount() - 1; i >= 0; i--)
            {
                TalentAreas.GetChild(i).QueueFree();
            }

            // 2. 加载 Trait 预制体（只加载一次，循环里用 Instantiate 复制）
            var talentScene = GD.Load<PackedScene>("res://UI/Trait.tscn");

            // 3. 循环创建 Trait 实例
            for (int i = 0; i < talentID.Count; i++)
            {
                // 实例化 Trait
                Trait trait = talentScene.Instantiate<Trait>();

                // 先加入场景树，再调 Setup 初始化显示
                // Trait 的 _Ready() 会自动查找子节点，AddChild 后 _Ready 被调用
                TalentAreas.AddChild(trait);
                trait.Setup(talentID[i]);
            }

            // 4. 刷新容器布局（GridContainer 动态添加子节点后必须调这个）
            TalentAreas.UpdateMinimumSize();
        }
    }
}
