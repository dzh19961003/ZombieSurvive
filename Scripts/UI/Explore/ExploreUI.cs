using Godot;
using Godot.Collections;
using MyProject;


public partial class ExploreUI : Control
{
	[Export] public TextureRect bg;
	[Export] public Label desLabel;
	[Export] public VBoxContainer chooseBar;
    [Export] public NinePatchRect noiseBar;

    // 当前创建出来的两个独立面板引用（方便离开时统一销毁）
    // 这两个面板原来是 ExploreUI 里的子节点，用 Visible 控制出现；
    // 现在独立成场景，用 UIManager.CreateUI 创建、DeleteUI 销毁。
    private RoomChooseBar _roomChooseBar;
    private ExploreChooseBar _exploreChooseBar;

    public override void _Ready()
	{
        // 两个面板的返回按钮逻辑已经搬到各自脚本里（RoomChooseBar / ExploreChooseBar），
        // 它们点击后都调用本脚本的 LeaveExplore() 统一销毁。
    }

    public void RefreshExplore(int exploreState,int layer)
	{
		Building building = ConfigManager.Instance.buildingDic[GameManager.Instance.currentBuildingID];

        //获取最大房间层级并给每层房间赋值
        int maxLayer = 0;
        foreach (var item in building.RoomID)
		{
			if (ConfigManager.Instance.roomDic[item].RoomLayer>maxLayer)
			{
				maxLayer = ConfigManager.Instance.roomDic[item].RoomLayer;
            }
		}
        Array<int>[] LayerArray=new Array<int>[maxLayer];
		for (int i = 0; i < LayerArray.Length; i++)
		{
			LayerArray[i] = new Array<int>();
		}
        for (int i = 1; i < LayerArray.Length+1; i++)
		{
            foreach (var item in building.RoomID)
			{
				if (ConfigManager.Instance.roomDic[item].RoomLayer == i)
				{
					LayerArray[i-1].Add(item);
                }
			}
		}

        switch (exploreState)
		{
            //房间选择界面：创建房间选择面板（以前用 Visible 显示，现在用 CreateUI 创建）
            case 1:
                chooseBar.Visible = false;
                TextTyper.TypeText(desLabel, building.Des);
                // 用 CreateUI 创建独立的房间选择面板，不走缓存，每次都是全新实例
                _roomChooseBar = (RoomChooseBar)UIManager.Instance.CreateUI("res://UI/Explore/RoomChooseBar.tscn");
                _roomChooseBar.Init(this, LayerArray[GameManager.Instance.exploreLayer - 1], maxLayer);
                break;
            //具体事件选择：这个分支现在由 OnRoomSelected 处理，不再走 RefreshExplore
            default:
				break;
		}
    }

    // 房间选项点击后调用（由 RoomChoose.InitialExplore 转发过来）
    // 销毁房间选择面板，加载事件，创建搜索策略面板
    public void OnRoomSelected(int roomID)
    {
        // 先销毁房间选择面板
        if (_roomChooseBar != null)
        {
            UIManager.Instance.DeleteUI(_roomChooseBar);
            _roomChooseBar = null;
        }

        GameManager.Instance.LoadEvent(roomID);
        GameManager.Instance.exploreState = 2;
        TextTyper.TypeText(desLabel, ConfigManager.Instance.roomDic[roomID].Des);
        chooseBar.Visible = true;

        // 创建独立的搜索策略面板
        _exploreChooseBar = (ExploreChooseBar)UIManager.Instance.CreateUI("res://UI/Explore/ExploreChooseBar.tscn");
        _exploreChooseBar.Init(this);
    }

    // 离开探索：销毁所有探索相关面板和自身
    // RoomChooseBar 的“离开建筑”按钮、ExploreChooseBar 的“中途撤离”按钮确认后都调用这里
    public void LeaveExplore()
    {
        if (_roomChooseBar != null)
        {
            UIManager.Instance.DeleteUI(_roomChooseBar);
            _roomChooseBar = null;
        }
        UIManager.Instance.DeleteUI(this);
    }
    public void LeaveRoom()
    {
        if (_exploreChooseBar != null)
        {
            UIManager.Instance.DeleteUI(_exploreChooseBar);
            _exploreChooseBar = null;
        }
        GameManager.Instance.exploreState = 1;
        GameManager.Instance.exploreLayer += 1;
        RefreshExplore(GameManager.Instance.exploreState, GameManager.Instance.exploreLayer);
    }
}
