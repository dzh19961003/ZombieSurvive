using Godot;
using Godot.Collections;
using MyProject;

// 房间选择面板（从 ExploreUI 里独立出来的 roomChoose 节点）。
// 由 ExploreUI 用 UIManager.CreateUI 创建，关闭时由 ExploreUI.LeaveExplore 统一 DeleteUI。
// 原来通过 Visible 控制出现/隐藏，现在改成创建/销毁整个面板。
public partial class RoomChooseBar : NinePatchRect
{
    [Export] public HBoxContainer roomContainer;   // 放房间选项按钮的容器
    [Export] public TextureButton backBtn;        // 返回地图按钮（离开建筑）
    [Export] public HBoxContainer roomProgressBar; // 探索进度层级展示

    // 所属的 ExploreUI，由 Init 传入；房间选项点击时要通过它通知切换面板
    public ExploreUI exploreUI;

    public override void _Ready()
    {
        // 离开建筑按钮：弹确认框，确认后交给 ExploreUI 统一销毁所有探索界面
        // （exploreUI 在 Init 里才赋值，但这里是点击时才用到，到那时已经赋值过了）
        backBtn.Pressed += () =>
        {
            CommonTips tips = UIManager.Instance.ShowCommonTips("离开建筑", "确认要离开当前建筑并结束探索吗");
            tips.OnConfirm = () => exploreUI.LeaveExplore();
        };
    }

    // 初始化：由 ExploreUI.RefreshExplore 在创建本面板后调用
    // layerRooms = 当前层的房间ID列表；maxLayer = 建筑最大层数
    public void Init(ExploreUI owner, Array<int> layerRooms, int maxLayer)
    {
        exploreUI = owner;

        // 建筑探索进度层级展示：每一层生成一个进度点
        for (int i = 0; i < maxLayer; i++)
        {
            var room = GD.Load<PackedScene>("res://UI/Explore/roomProgress.tscn");
            RoomProgress roomProgress = room.Instantiate<RoomProgress>();
            roomProgressBar.AddChild(roomProgress);
            if (i < GameManager.Instance.exploreLayer)
            {
                roomProgress.Initial();
            }
        }

        // 生成当前层的房间选项
        for (int i = 0; i < layerRooms.Count; i++)
        {
            var room = GD.Load<PackedScene>("res://UI/Explore/roomChoose.tscn");
            RoomChoose roomChoose = room.Instantiate<RoomChoose>();
            roomContainer.AddChild(roomChoose);
            roomChoose.InitialRoom(layerRooms[i]);
            roomChoose.ID = layerRooms[i];
        }
        // 把返回按钮移到最后（始终在最右侧）
        roomContainer.MoveChild(backBtn, -1);
    }
}
