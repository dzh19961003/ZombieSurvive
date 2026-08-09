using Godot;
using MyProject;
using System;

public partial class EventChooseBtn : CenterContainer
{
    [Export] public Label desLabel;
    [Export] public TextureButton textureBtn;
    public ExploreUI exploreUI;
    public int eventID;
    public int rank;
    private EventChooseBar lastBar;
    private bool finish = false;
    public override void _Ready()
    {
        textureBtn.Pressed += () =>
        {
            ExploreEvent exploreEvent = ConfigManager.Instance.exploreEventDic[eventID];

            if (GameManager.Instance.exploreProgress[GameManager.Instance.roomID]>=100)
            {
                finish = true;
            }

            if (exploreEvent.NextEvent[rank] == 9999)
            {
                UIManager.Instance.DeleteUI((Control)GetParent());
                exploreUI.OnRoomSelected(GameManager.Instance.roomID, finish);
                GameManager.Instance.currentEventID = 1;
                exploreUI.RefreshExploreUI(true);
            }
            else
            {
                EventChooseBar eventChooseBar = (EventChooseBar)UIManager.Instance.CreateUI("res://UI/Explore/EventChooseBar.tscn");
                eventChooseBar.exploreUI = exploreUI;
                GameManager.Instance.currentEventID = exploreEvent.NextEvent[rank];
                GetParent().QueueFree();
                eventChooseBar.Initial(exploreEvent.NextEvent[rank]);
            }
        };
    }

    public void Initial(string des, string image)
    {
        desLabel.Text = des;
    }

}
