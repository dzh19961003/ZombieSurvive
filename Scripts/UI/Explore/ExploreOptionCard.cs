using Godot;

public partial class ExploreOptionCard : Control
{
    [Export] public NinePatchRect selectEdge;      //已选择时的橙色边框
    [Export] public NinePatchRect namePlate;       //名字条
    [Export] public Label nameLabel;               //搜索方式名字
    [Export] public TextureRect image;             //插图
    [Export] public NinePatchRect selectBadge;     //“当前选择”角标
    [Export] public Button button;                 //整张卡片的点击区

    public int ExploreType;                        //1=仔细搜索 2=高效搜索

    private ExploreChooseBar _bar;

    public override void _Ready()
    {
        //向上找两级：ExploreOptionCard -> optionContainer -> ExploreChooseBar
        Node parent = GetParent().GetParent();
        _bar = parent as ExploreChooseBar;
        button.Pressed += () => _bar.SelectOption(this);
    }

    public void Initial(int type, string optionName, string imageName)
    {
        ExploreType = type;
        nameLabel.Text = optionName;
        image.Texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/Explore/" + imageName + ".png");
    }

    //由 ExploreChooseBar 调用：切换选中边框和“当前选择”角标
    public void SetSelected(bool selected)
    {
        selectEdge.Visible = selected;
        //selectBadge.Visible = selected;
    }
}
