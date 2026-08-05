using Godot;
using MyProject;
using System;

public partial class ExploreChooseBtn : CenterContainer
{
	[Export] public Label desLabel;
	[Export] public TextureButton textureBtn;
    public ExploreUI exploreUI;
	public int eventID;
    public int rank;
    public override void _Ready()
	{
		textureBtn.Pressed += () =>
		{
			ExploreEvent exploreEvent = ConfigManager.Instance.exploreEventDic[eventID];

            EventChooseBar eventChooseBar= (EventChooseBar)UIManager.Instance.CreateUI("res://UI/Explore/EventChooseBar.tscn");
            eventChooseBar.exploreUI = exploreUI;
            if (exploreEvent.NextEvent[rank] == 9999)
			{
                UIManager.Instance.DeleteUI((Control)GetParent());
				exploreUI.OnRoomSelected(GameManager.Instance.roomID);
            }
			else 
			{ 
				eventChooseBar.Initial(exploreEvent.NextEvent[rank]);
			}
			
			UIManager.Instance.DeleteUI((Control)GetParent());
        };
	}

	public void Initial(string des,string image) 
	{
		desLabel.Text = des;
	}

}
