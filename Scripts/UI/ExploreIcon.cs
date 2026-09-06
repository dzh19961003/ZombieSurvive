using Godot;
using MyProject;
using System;

public partial class ExploreIcon : Control
{
	[Export] public TextureRect icon;
    [Export] public Label addLabel;
    [Export] public Label minusLabel;
	[Export] public Label numLabel;

	public override void _Ready()
	{
	}

	public void Initial(int ID,int num)
	{
		addLabel.Visible = false;
        minusLabel.Visible = false;
        //GetItem里已经抽过随机，这里直接用传进来的ID取图标
        //GetItemIcon内部会自动区分：小于10000是物品，大于等于10000是状态
        icon.Texture = UIManager.Instance.GetItemIcon(ID);

        if (num >= 0)
        {
            addLabel.Visible = true;
        }
        else
        {
            minusLabel.Visible = true;
        }
        numLabel.Text = Math.Abs(num).ToString();
    }

}
