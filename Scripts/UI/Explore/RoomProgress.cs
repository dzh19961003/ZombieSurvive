using Godot;

public partial class RoomProgress : Control
{
    [Export] public TextureRect lineLeft;     //左边的连接线
    [Export] public TextureRect lineRight;    //右边的连接线
    [Export] public TextureRect off;          //未探索
    [Export] public TextureRect on;           //已探索
    [Export] public TextureRect current;      //正探索

    //index：当前是第几个节点（从 0 开始）；maxLayer：总层数
    public void Initial(int index, int maxLayer)
    {
        //当前正在探索的层（从 0 开始）
        int nowLayer = GameManager.Instance.exploreLayer - 1;

        //走过的层 -> 已探索；当前层 -> 正探索；还没到的层 -> 未探索
        on.Visible = index < nowLayer;
        current.Visible = index == nowLayer;
        off.Visible = index > nowLayer;

        //首尾两端不画连接线，避免多出一截悬空的线
        lineLeft.Visible = index > 0;
        lineRight.Visible = index < maxLayer - 1;
    }
}
