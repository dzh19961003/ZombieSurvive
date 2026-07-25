using Godot;
using MyProject;

public partial class StateTips : Control
{
	[Export] public Label nameLabel;
    [Export] public Label desLabel;
    public override void _Ready()
	{

	}
	public void Initail(int ID) 
	{
		nameLabel.Text = ConfigManager.Instance.stateDic[ID].Name;
        if (ConfigManager.Instance.stateDic[ID].Positive == 0)
        {
            nameLabel.AddThemeColorOverride("font_color", Colors.Red);
        }
        else
        {
            nameLabel.AddThemeColorOverride("font_color", Colors.Green);
        }
        desLabel.Text = ConfigManager.Instance.stateDic[ID].Effect;
    }


}
