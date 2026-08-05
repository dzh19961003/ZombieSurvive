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

		for (int i = 0; i < exploreEvent.Option.Count; i++)
		{
		    var chooseBtn = GD.Load<PackedScene>("res://UI/Explore/ExploreChooseBtn.tscn");
		    ExploreChooseBtn exploreChooseBtn = chooseBtn.Instantiate<ExploreChooseBtn>();
		    this.AddChild(exploreChooseBtn);
			exploreChooseBtn.Initial(exploreEvent.Option[i], "");
			exploreChooseBtn.rank = i;
			exploreChooseBtn.eventID = eventID;
			exploreChooseBtn.exploreUI = exploreUI;
        }		
    }
}
