using Godot;

// ============================================================
//  SceneLoader — 简单场景切换工具
//  挂在哪：Main.tscn 里新建一个节点，把这个脚本挂上去就行
//  作用：把游戏子场景（战斗、主菜单等）加载到 Main 里的一个"容器"节点中，
//        这样 UIManager 之类挂在 Main 上的东西就永远不会消失
// ============================================================

public partial class SceneLoader : Node
{
    public static SceneLoader Instance { get; private set; }

    [Export] public Node SceneContainer;

    // 当前加载的子场景节点（记录一下，切换时用来删掉旧的）
    private Node _currentScene;

    public override void _Ready()
    {
        Instance = this;        
    }

    // ─────────────────────────────────────────────────────────
    //  GoTo：切换到另一个子场景
    //
    //  用法示例：
    //    SceneLoader.Instance.GoTo("res://Scene/Battle.tscn");
    //
    //  注意：这会删掉当前子场景，创建新的子场景放进容器里。
    //        Main 节点本身（以及 UIManager 等）不受影响。
    // ─────────────────────────────────────────────────────────
    public void GoTo(string scenePath)
    {
        if (SceneContainer == null)
        {
            GD.PrintErr("SceneContainer 没有设置！");
            return;
        }

        // 先关掉所有 UI（可选，如果不需要可以删掉这行）
        UIManager.Instance?.HideAll();

        // 删掉旧场景
        if (_currentScene != null)
        {
            _currentScene.QueueFree();
            _currentScene = null;
        }

        // 加载新场景
        var scene = GD.Load<PackedScene>(scenePath);
        if (scene == null)
        {
            GD.PrintErr("找不到场景：" + scenePath);
            return;
        }

        _currentScene = scene.Instantiate();
        SceneContainer.AddChild(_currentScene);

        GD.Print("场景切换完成：" + scenePath);
    }
}
