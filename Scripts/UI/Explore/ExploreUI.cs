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

	public override void _Ready()
	{
		RefreshExplore(1, 1);
    }
	public void RefreshExplore(int exploreState,int layer) 
	{		
		Building building = ConfigManager.Instance.buildingDic[GameManager.Instance.currentBuildingID];

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
		GD.Print(maxLayer);

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
			case 1:
				roomContainer.Visible = true;
                chooseBar.Visible = false;
				roomBg.Visible = true;
				desLabel.Text = building.Des;
                for (int i = 0; i < LayerArray[GameManager.Instance.exploreLayer-1].Count; i++)
				{
                    var room = GD.Load<PackedScene>("res://UI/Explore/roomChoose.tscn");
                    RoomChoose roomChoose = room.Instantiate<RoomChoose>();
                    roomContainer.AddChild(roomChoose);
					roomChoose.InitialRoom(LayerArray[GameManager.Instance.exploreLayer - 1][i]);
                }
				
                break;
            case 2:
                roomContainer.Visible = false;
                chooseBar.Visible = true;
                roomBg.Visible = false;
                break;
            default:

				break;
		}
		
	}
}
