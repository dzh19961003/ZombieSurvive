using Godot;
using System;

public partial class Door : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.InputEvent += OnMousePressed;
	}

  
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
    private void OnMousePressed(Node viewport, InputEvent @event, long shapeIdx)
    {
		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left )
		{
			OpenDoor();
        }
    }


    private void OpenDoor()
    {
        CommonTips tips = UIManager.Instance.ShowCommonTips("外出探索", "确认结束上午并进行外出探索吗？（时间将切换到中午）");
        // 用 = 赋值（覆盖），不要用 += 累加，避免重复连接信号报错
        tips.OnConfirm = () => UIManager.Instance.ShowUI("res://UI/ExploreMap.tscn");
    }
}
