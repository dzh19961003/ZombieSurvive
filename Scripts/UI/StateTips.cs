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
            AddThemeColorOverride(nameLabel.Name, Colors.Red);
        }
        else
        {
            AddThemeColorOverride(nameLabel.Name, Colors.Green);
        }
        desLabel.Text = ConfigManager.Instance.stateDic[ID].Effect;
    }


}
