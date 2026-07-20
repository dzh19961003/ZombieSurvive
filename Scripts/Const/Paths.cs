// ============================================================
//  UIPaths — 所有UI面板路径统一放这里
//
//  用法：
//    UIManager.Instance.ShowPanel(UIPaths.Shop);
//    UIManager.Instance.HidePanel(UIPaths.Shop);
//
//  规则：
//    每新建一个面板.tscn，在这里加一行 public const string Xxx = "路径";
//    路径用完整的 "res://" 开头。
// ============================================================

public static class Paths
{
    //UI面板路径：
    public const string MainUI = "res://UI/MainUI.tscn";


    //场景路径：
    //public const string Main = "res://Scene/main.tscn";



    // 以后每新建一个面板，就在下面继续加
}
