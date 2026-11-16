using Godot;
using MyProject;
using System;

public partial class RoomCard : Control
{
    [Export] public Label nameLabel;
    [Export] public Label progressLabel;
    [Export] public HBoxContainer supplyRow;
    [Export] public Button clickBtn;
    [Export] public Control mask;

    // 与设计图一致：药品、食物、装备、材料；仍按各字段是否非零显示。
    private const string IconDir = "res://Assets/Images/UI/Explore/ExploreNew/";
    private static readonly string[] SupplyIcons = { "medicine.png", "food.png", "equipment.png", "tools.png" };

    // 当前卡片代表的房间ID
    public int RoomID;

    // 本房间是否已探索完（剩余次数0），RoomChooseUI 用它在点击时弹对应提示
    public bool IsFinished;

    // 点击后由 RoomChooseUI 接管（参数是房间ID）
    public Action<int> OnCardClicked;

    public override void _Ready()
    {
        clickBtn.Pressed += () => OnCardClicked?.Invoke(RoomID);
    }

    // 三个以上房间时等比缩小整张卡，避免文字、图标和点击区域各自缩放而错位。
    public void SetDisplayWidth(float width)
    {
        CustomMinimumSize = new Vector2(width, 539);
        var visual = GetNode<Control>("Visual");
        float scale = width / 394f;
        visual.Scale = Vector2.One * scale;
        visual.Position = new Vector2(0, (539 - 539 * scale) / 2);
    }

    // room = 房间配置，left = 该房间剩余可探索次数
    public void Setup(Room room, int left)
    {
        RoomID = room.ID;
        nameLabel.Text = room.Name;
        progressLabel.Text = $"{left}/{room.ExploreNum}";

        // 剩余次数为 0 时盖黑色半透明遮罩（遮罩 mouse_filter=Ignore，点击穿透到 ClickBtn，
        // 由 RoomChooseUI 在 EnterRoom 里判断 IsFinished 决定弹什么提示，不进入探索）
        IsFinished = left <= 0;
        mask.Visible = IsFinished;

        // 先清掉上一次生成的图标
        foreach (Node n in supplyRow.GetChildren())
        {
            supplyRow.RemoveChild(n);
            n.QueueFree();
        }

        // 四种物资，配置里数量不为 0 才显示图标
        int[] amounts = { room.Medic, room.Food, room.Equip, room.Material };
        supplyRow.AddThemeConstantOverride("separation", Array.FindAll(amounts, amount => amount != 0).Length > 3 ? 20 : 38);
        for (int i = 0; i < amounts.Length; i++)
        {
            if (amounts[i] == 0) continue;

            var icon = new TextureRect();
            icon.Texture = GD.Load<Texture2D>(IconDir + SupplyIcons[i]);
            // 物资图都是128x128原图，必须允许缩放，否则按原尺寸直出会撑爆卡片
            icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            icon.CustomMinimumSize = new Vector2(64, 64);
            icon.MouseFilter = MouseFilterEnum.Ignore;
            supplyRow.AddChild(icon);
        }
    }
}
