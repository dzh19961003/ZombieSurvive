using Godot;
using Godot.Collections;
using MyProject;


public partial class RoomChooseBar : NinePatchRect
{
    [Export] public HBoxContainer roomContainer;
    [Export] public TextureButton backBtn;
    [Export] public HBoxContainer roomProgressBar;
    [Export] public TextureRect detailsPanel;
    [Export] public Label exploreProgress;
    [Export] public TextureButton exploreBtn;

    public ExploreUI exploreUI;
    private GameManager gm;
    private bool finish = false;

    public override void _Ready()
    {
        backBtn.Pressed += () =>
        {
            CommonTips tips = UIManager.Instance.ShowCommonTips("离开建筑", "确认要离开当前建筑并结束探索吗");
            tips.OnConfirm = () => exploreUI.LeaveExplore();
        };
        exploreBtn.Pressed += InitialExplore;
        gm = GameManager.Instance;
    }
    public void Init(ExploreUI owner, Array<int> layerRooms, int maxLayer)
    {
        exploreUI = owner;

        if (maxLayer < GameManager.Instance.exploreLayer)
        {
            return;
        }
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
    }
    public void Init(ExploreUI owner)
    {
        exploreUI = owner;
    }
    private void InitialExplore()
    {
        if (GameManager.Instance.exploreProgress[GameManager.Instance.roomID] >= 100)
        {
            finish = true;
        }
        exploreUI.OnRoomSelected(gm.roomID, false);
        exploreUI.RefreshExploreUI(true);
    }
}
