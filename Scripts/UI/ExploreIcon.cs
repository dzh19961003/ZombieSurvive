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
		if (ID<=10000)
		{
            int itemID = Tools.GetRandomNumber(ConfigManager.Instance.itemPoolDic[ID].Item);
            icon.Texture = UIManager.Instance.GetItemIcon(itemID);            
        }
        else
        {
            icon.Texture = UIManager.Instance.GetItemIcon(ID);
        }

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
