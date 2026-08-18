using Godot;
using Godot.Collections;
using MyProject;
using System;

public partial class EventChooseBtn : CenterContainer
{
    [Export] public Label desLabel;
    [Export] public TextureButton textureBtn;
    [Export] public Label desLabel2;
    [Export] public TextureRect needBtn;
    [Export] public Label numLabel;

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
            GameManager gameManager = GameManager.Instance;
            //进度大于100，变为已完成状态
            if (GameManager.Instance.exploreProgress[GameManager.Instance.roomID]>=100)
            {
                finish = true;
            }
            //进度为90，且当前事件类型为103
            if (GameManager.Instance.exploreProgress[GameManager.Instance.roomID] == 90 && ConfigManager.Instance.exploreEventDic[eventID].EventType == 103)
            {
                finish = true;
            }
            //普通事件结尾
            if (exploreEvent.NextEvent[rank] == 9999)
            {
                UIManager.Instance.DeleteUI((Control)GetParent());
                exploreUI.OnRoomSelected(GameManager.Instance.roomID, finish);
                GameManager.Instance.currentEventID = 1;
                exploreUI.RefreshExploreUI(true);
            }
            //关键支线结尾
            else if (exploreEvent.NextEvent[rank] == 9998)
            {
                UIManager.Instance.DeleteUI((Control)GetParent());
                exploreUI.OnRoomSelected(GameManager.Instance.roomID, finish);
                GameManager.Instance.currentEventID = 1;
                exploreUI.RefreshExploreUI(true);

                Array<int> subTaskArray = new Array<int>();
                if (!gameManager.subTaskDic.ContainsKey(gameManager.roomID))
                {
                    gameManager.subTaskDic.Add(gameManager.roomID, subTaskArray);
                }
                else
                {
                    subTaskArray = gameManager.subTaskDic[gameManager.roomID];
                }
                subTaskArray.Add(gameManager.currentSubTask);
                gameManager.subTaskDic[gameManager.roomID] = subTaskArray;
                gameManager.exploreProgress[gameManager.roomID] += 10;
            }
            //继续事件
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
        if (ConfigManager.Instance.exploreEventDic[eventID].Demand.Count >= rank+1)
        {
            desLabel2.Visible = true;
            needBtn.Visible = true;
            numLabel.Visible = true;
            needBtn.Texture = UIManager.Instance.GetItemIcon(ConfigManager.Instance.exploreEventDic[eventID].Demand[rank]);
            numLabel.Text = "x"+ConfigManager.Instance.exploreEventDic[eventID].DemandNum[rank].ToString();
        }
    }

}
