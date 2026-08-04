using Godot;
using MyProject;

// 搜索策略面板（从 ExploreUI 里独立出来的 exploreChoose 节点）。
// 由 ExploreUI.OnRoomSelected 用 UIManager.CreateUI 创建，关闭时由 ExploreUI.LeaveExplore 统一 DeleteUI。
// 原来通过 Visible 控制出现/隐藏，现在改成创建/销毁整个面板。
public partial class ExploreChooseBar : NinePatchRect
{
    [Export] public TextureButton backBtn; // 中途撤离按钮

    // 所属的 ExploreUI，由 Init 传入
    public ExploreUI exploreUI;

    public override void _Ready()
    {
        // 中途撤离按钮：弹确认框，确认后交给 ExploreUI 统一销毁所有探索界面
        // （exploreUI 在 Init 里才赋值，但这里是点击时才用到，到那时已经赋值过了）
        backBtn.Pressed += () =>
        {
            CommonTips tips = UIManager.Instance.ShowCommonTips("终止探索", "确认要离开当前位置并继续前进吗");
            tips.OnConfirm = () => exploreUI.LeaveRoom();
        };
    }

    // 初始化：由 ExploreUI.OnRoomSelected 在创建本面板后调用
    public void Init(ExploreUI owner)
    {
        exploreUI = owner;
    }
}
