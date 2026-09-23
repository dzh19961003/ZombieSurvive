using Godot;
using Godot.Collections;
using MyProject;


public partial class RoomChooseBar : NinePatchRect
{
    private const int MaxProgressSlot = 4;      //进度行最多预留几个槽位

    [Export] public HBoxContainer roomContainer;
    [Export] public TextureButton backBtn;
    [Export] public HBoxContainer roomProgressBar;
    [Export] public TextureRect detailsPanel;
    [Export] public Label exploreProgress;
    [Export] public TextureButton exploreBtn;
    [Export] public Label locationLabel;        //“地点：xxx”文本
    [Export] public Label progressNum;          //“路线进度”右侧的 2/3 文本

    public ExploreUI exploreUI;
    private bool finish = false;
    private RoomChoose selectedRoom;            //当前处于选中状态的房间

    public override void _Ready()
    {
        backBtn.Pressed += () =>
        {
            CommonTips tips = UIManager.Instance.ShowCommonTips("离开建筑", "确认要离开当前建筑并结束探索吗");
            tips.OnConfirm = () => exploreUI.LeaveExplore();
        };
        exploreBtn.Pressed += InitialExplore;
    }
    public void Init(ExploreUI owner, Array<int> layerRooms, int maxLayer)
    {
        exploreUI = owner;
        selectedRoom = null;
        finish = false;
        locationLabel.Text = "地点：";

        if (maxLayer < GameManager.Instance.exploreLayer)
        {
            return;
        }
        CreateProgress(maxLayer);
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
    //所有层都探索完时调用：没有房间可选，只展示走完的进度
    public void Init(ExploreUI owner, int maxLayer)
    {
        exploreUI = owner;
        selectedRoom = null;
        finish = false;
        locationLabel.Text = "地点：";
        CreateProgress(maxLayer);
    }
    //生成“路线进度”那一行：每层一个节点，两端配上文字
    private void CreateProgress(int maxLayer)
    {
        //进度行固定预留 4 个槽位（roomProgressBar 的最小宽度就是这么算的），
        //所以层数少于 4 时节点仍然是居中显示的，两边文字不会跟着跑
        int count = Mathf.Min(maxLayer, MaxProgressSlot);
        for (int i = 0; i < count; i++)
        {
            var room = GD.Load<PackedScene>("res://UI/Explore/roomProgress.tscn");
            RoomProgress roomProgress = room.Instantiate<RoomProgress>();
            roomProgressBar.AddChild(roomProgress);
            roomProgress.Initial(i, count);
        }
        //全部走完时层号会超过总层数，这里夹一下，避免出现 4/3
        progressNum.Text = Mathf.Min(GameManager.Instance.exploreLayer, maxLayer) + "/" + maxLayer;
    }
    //选中某个房间：先清掉上一个房间的边框，保证同时只有一个房间被选中
    public void SelectRoom(RoomChoose room)
    {
        if (selectedRoom == room)
        {
            return;
        }
        if (selectedRoom != null)
        {
            selectedRoom.SetSelected(false);
        }
        selectedRoom = room;
        selectedRoom.SetSelected(true);
        locationLabel.Text = "地点：" + ConfigManager.Instance.roomDic[room.ID].Name;
    }
    private void InitialExplore()
    {
        //没选房间就点探索，直接忽略
        if (selectedRoom == null) return;

        //用当前选中的房间，而不是 GameManager 里残留的旧房间
        int roomID = selectedRoom.ID;
        GameManager.Instance.roomID = roomID;

        //每次重新判断，别沿用上一次的结果
        finish = false;
        if (GameManager.Instance.GetExploreProgress(roomID) >= 100)
        {
            finish = true;
        }

        exploreUI.OnRoomSelected(roomID, finish);
        exploreUI.RefreshExploreUI(true);
    }
}
