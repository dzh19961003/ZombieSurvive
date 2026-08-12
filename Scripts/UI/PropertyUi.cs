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

        // 三项属性显示节点（力量 / 敏捷 / 智力）
        private Label _strengthLabel;
        private Label _agilityLabel;
        private Label _intelligenceLabel;
        private TextureProgressBar _strengthBar;
        private TextureProgressBar _agilityBar;
        private TextureProgressBar _intelligenceBar;
        private Label _strengthBarText;
        private Label _agilityBarText;
        private Label _intelligenceBarText;

        // 生命值显示节点（血条 + "当前/最大" 文字）
        private TextureProgressBar _healthBar;
        private Label _healthText;

        // 属性进度条上限（与场景中 TextureProgressBar 默认 max_value 一致）
        private const int AttrMax = PlayerManager.ExpMax;

        public override void _Ready()
        {

            if (TalentAreas == null)
                TalentAreas = GetNodeOrNull<GridContainer>("Traits/TraitAreas");

  
            _strengthLabel      = GetNodeOrNull<Label>("strength/number");
            _agilityLabel       = GetNodeOrNull<Label>("agile/number");
            _intelligenceLabel  = GetNodeOrNull<Label>("intelligence/number");
            _strengthBar        = GetNodeOrNull<TextureProgressBar>("strength/progress");
            _agilityBar         = GetNodeOrNull<TextureProgressBar>("agile/progress");
            _intelligenceBar    = GetNodeOrNull<TextureProgressBar>("intelligence/progress");
            _strengthBarText    = GetNodeOrNull<Label>("strength/progress/Label");
            _agilityBarText     = GetNodeOrNull<Label>("agile/progress/Label2");
            _intelligenceBarText= GetNodeOrNull<Label>("intelligence/progress/Label3");

            // 血条节点：Health 本身是 TextureProgressBar，Health/Label 是 "当前/最大" 文字
            _healthBar          = GetNodeOrNull<TextureProgressBar>("Health");
            _healthText         = GetNodeOrNull<Label>("Health/Label");

            close_button.Pressed += OnCloseButtonPressed;

            // UIManager.ShowUI 是缓存机制：首次创建后才 _Ready，之后只切换 Visible。
            // 因此监听可见性变化，面板再次显示时自动刷新属性。
            VisibilityChanged += OnVisibilityChanged;

            // 从 PlayerManager 获取状态ID数组，生成标签
            SpawnStateList(PlayerManager.Instance.GetStateArray());

            // 从 PlayerManager 获取天赋ID数组，生成天赋标签
            SpawnTalentList(PlayerManager.Instance.GetTalentID());

            // 首次刷新属性显示
            RefreshAttributes();

            PlayerManager.Instance.GetItem += RefreshAttributes;
            PlayerManager.Instance.GetItem2 += SpawnStateList;
        }

        private void OnVisibilityChanged()
        {
            if (Visible) RefreshAttributes();
        }

        /// <summary>
        /// 从 PlayerManager 读取力量 / 敏捷 / 智力及其经验值，刷新到 UI。
        /// number 显示属性值，进度条显示经验进度（满 ExpMax 时属性 +1）。
        /// 可在外部属性变化后主动调用，也会在面板每次显示时自动触发。
        /// </summary>
        public void RefreshAttributes()
        {
            var pm = PlayerManager.Instance;
            if (pm == null) return;

            SetAttrDisplay(_strengthLabel, _strengthBar, _strengthBarText, pm.Strength, pm.Strength_exp);
            SetAttrDisplay(_agilityLabel, _agilityBar, _agilityBarText, pm.Agility, pm.Agility_exp);
            SetAttrDisplay(_intelligenceLabel, _intelligenceBar, _intelligenceBarText, pm.Intelligence, pm.Intelligence_exp);

            // 刷新血条：MaxValue 设为当前最大生命值（含状态加成），Value 显示当前生命值
            if (_healthBar != null)
            {
                _healthBar.MaxValue = pm.MaxHp;
                _healthBar.Value = Mathf.Clamp(pm.Hp, 0, pm.MaxHp);
            }
            if (_healthText != null)
            {
                _healthText.Text = $"{pm.Hp}/{pm.MaxHp}";
            }
        }

        private void SetAttrDisplay(Label numLabel, TextureProgressBar bar, Label barText, int attrValue, int expValue)
        {
            if (numLabel != null) numLabel.Text = attrValue.ToString();
            if (bar != null)      bar.Value = Mathf.Clamp(expValue, 0, AttrMax);
            if (barText != null)  barText.Text = $"{expValue}/{AttrMax}";
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
            var stateScene = GD.Load<PackedScene>("res://UI/States.tscn");

            // 3. 循环创建 Buff 实例
            for (int i = 0; i < stateIDArray.Count; i++)
            {
                States states = stateScene.Instantiate<States>();

                // Buff 的 _Ready() 会检查 ID，所以必须在 AddChild 之前设置
                states.ID = stateIDArray[i];

                StateAreas.AddChild(states);
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
            var talentScene = GD.Load<PackedScene>("res://UI/Talents.tscn");

            // 3. 循环创建 Trait 实例
            for (int i = 0; i < talentID.Count; i++)
            {
                // 实例化 Trait
                Talents talents = talentScene.Instantiate<Talents>();

                // 先加入场景树，再调 Setup 初始化显示
                // Trait 的 _Ready() 会自动查找子节点，AddChild 后 _Ready 被调用
                TalentAreas.AddChild(talents);
                talents.Setup(talentID[i]);
            }

            // 4. 刷新容器布局（GridContainer 动态添加子节点后必须调这个）
            TalentAreas.UpdateMinimumSize();
        }
    }
}
