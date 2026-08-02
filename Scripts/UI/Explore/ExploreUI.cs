using Godot;
using Godot.Collections;
using MyProject;


public partial class ExploreUI : Control
{
	[Export] public TextureRect bg;
	[Export] public Label desLabel;
	[Export] public VBoxContainer chooseBar;
	[Export] public HBoxContainer roomContainer;
	[Export] public NinePatchRect roomBg;
    [Export] public TextureButton backBtn;
    [Export] public HBoxContainer roomProgressBar;
    [Export] public NinePatchRect noiseBar;


    public override void _Ready()
	{
		backBtn.Pressed += () => { 
			CommonTips tips= UIManager.Instance.ShowCommonTips("离开建筑","确认要离开当前建筑并结束探索吗");
			tips.OnConfirm = () => UIManager.Instance.DeleteUI(this);
		};
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
		//建筑探索进度层级展示
		for (int i = 0; i < maxLayer; i++)
		{
			var room = GD.Load<PackedScene>("res://UI/Explore/roomProgress.tscn");
			RoomProgress roomProgress = room.Instantiate<RoomProgress>();
			roomProgressBar.AddChild(roomProgress);
			if (i<GameManager.Instance.exploreLayer)
			{
                roomProgress.Initial();
            }			
        }
       
        switch (exploreState)
		{
            //房间选择界面，生成房间信息
            case 1:				
                chooseBar.Visible = false;
				roomBg.Visible = true;
				TextTyper.TypeText(desLabel, building.Des);               
                for (int i = 0; i < LayerArray[GameManager.Instance.exploreLayer-1].Count; i++)
				{
                    var room = GD.Load<PackedScene>("res://UI/Explore/roomChoose.tscn");
                    RoomChoose roomChoose = room.Instantiate<RoomChoose>();
                    roomContainer.AddChild(roomChoose);
					roomChoose.InitialRoom(LayerArray[GameManager.Instance.exploreLayer - 1][i]);
                }
				roomContainer.MoveChild(backBtn, -1);
                break;
            //具体事件选择，生成选项信息
            case 2:
                chooseBar.Visible = true;
                roomBg.Visible = false;
                break;
            default:
				break;
		}
    }
}
