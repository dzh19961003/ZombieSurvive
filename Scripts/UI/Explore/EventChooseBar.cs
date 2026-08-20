using Godot;
using MyProject;
using System;

public partial class EventChooseBar : VBoxContainer
{
	public ExploreUI exploreUI;
    public int eventID = 0;
	public override void _Ready()
	{
        TextTyper.OnTypeEnd += SpawnOptions;
    }
	public void Initial() 
	{   
        exploreUI.RefreshExploreUI(true);
        ExploreEvent exploreEvent= ConfigManager.Instance.exploreEventDic[eventID];        
        TextTyper.TypeText(exploreUI.desLabel, exploreEvent.Des);
		exploreUI.image.Visible = true;        
        //加载事件图片
        exploreUI.image.Texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/Explore/" + exploreEvent.Image+ ".png");		
    }
	private void SpawnOptions() 
	{
        Visible = true;
        ExploreEvent exploreEvent = ConfigManager.Instance.exploreEventDic[eventID];
        //生成三个选项
        for (int i = 0; i < exploreEvent.Option.Count; i++)
        {
            var chooseBtn = GD.Load<PackedScene>("res://UI/Explore/EventChooseBtn.tscn");
            EventChooseBtn exploreChooseBtn = chooseBtn.Instantiate<EventChooseBtn>();
            this.AddChild(exploreChooseBtn);
            exploreChooseBtn.eventID = eventID;
            exploreChooseBtn.rank = i;
            exploreChooseBtn.Initial(exploreEvent.Option[i], "");
            exploreChooseBtn.exploreUI = exploreUI;
        }
        TextTyper.OnTypeEnd -= SpawnOptions;
    }
}
