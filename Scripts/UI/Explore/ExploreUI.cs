using Godot;
using Godot.Collections;
using MyProject;


public partial class ExploreUI : Control
{
	[Export] public TextureRect bg;
	[Export] public Label desLabel;
    [Export] public NinePatchRect noiseBar;
    [Export] public Label exploreProgressLabel;
    [Export] public Label noiseProgressLabel;

    private RoomChooseBar _roomChooseBar;
    private ExploreChooseBar _exploreChooseBar;

    public override void _Ready()
	{

    }

    public void RefreshExplore()
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
        RefreshExploreUI();
        //初始化房间
        TextTyper.TypeText(desLabel, building.Des);
        _roomChooseBar = (RoomChooseBar)UIManager.Instance.CreateUI("res://UI/Explore/RoomChooseBar.tscn");
        _roomChooseBar.Init(this, LayerArray[GameManager.Instance.exploreLayer - 1], maxLayer);
    }
    public void RefreshExploreUI()
    {
        if (GameManager.Instance.exploreProgress.ContainsKey(GameManager.Instance.roomID))
        {
            exploreProgressLabel.Text = GameManager.Instance.exploreProgress[GameManager.Instance.roomID].ToString() + "%";
        }
        else
        {
            exploreProgressLabel.Text =  "0%";
        }        
        noiseProgressLabel.Text = GameManager.Instance.exploreNoise.ToString() + "%";
    }
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

        // 创建独立的搜索策略面板
        _exploreChooseBar = (ExploreChooseBar)UIManager.Instance.CreateUI("res://UI/Explore/ExploreChooseBar.tscn");
        _exploreChooseBar.Init(this);
    }

    // 离开探索：销毁所有探索相关面板和自身
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
        RefreshExplore();
    }
}
