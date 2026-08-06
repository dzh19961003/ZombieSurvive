using Godot;
using MyProject;
using System;

public partial class EventChooseBar : VBoxContainer
{
	public ExploreUI exploreUI;
	public override void _Ready()
	{
	}
	public void Initial(int eventID) 
	{
		ExploreEvent exploreEvent= ConfigManager.Instance.exploreEventDic[eventID];
		TextTyper.TypeText(exploreUI.desLabel, exploreEvent.Des);
		exploreUI.image.Visible = true;
		exploreUI.image.Texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/Explore/" + exploreEvent.Image+ ".png");

        for (int i = 0; i < exploreEvent.Option.Count; i++)
		{
		    var chooseBtn = GD.Load<PackedScene>("res://UI/Explore/EventChooseBtn.tscn");
		    EventChooseBtn exploreChooseBtn = chooseBtn.Instantiate<EventChooseBtn>();
		    this.AddChild(exploreChooseBtn);
			exploreChooseBtn.Initial(exploreEvent.Option[i], "");
			exploreChooseBtn.rank = i;
			exploreChooseBtn.eventID = eventID;
			exploreChooseBtn.exploreUI = exploreUI;
        }		
    }
}
