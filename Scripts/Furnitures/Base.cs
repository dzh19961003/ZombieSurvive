using Godot;
using System.Collections.Generic;

public partial class Base : Node2D
{
    private class TextureSwap
    {
        public Sprite2D Sprite;
        public string DayFile;
        public string NightFile;
    }

    private readonly List<TextureSwap> _staticSwaps = new();
    private Train _train;
    private Workstation _workstation;

    private const string TexDir = "res://Assets/Images/Base/";

    public override void _Ready()
    {
        // 注册需要昼夜切换的静态精灵（节点路径, 白天贴图, 夜间贴图）
        RegisterStaticSwap("Bg", "bg_expanded.png", "night_bg_expanded.png");
        RegisterStaticSwap("Tent", "tent_1.png", "night_tent_1.png");
        RegisterStaticSwap("Plant", "plant_1.png", "night_plant_1.png");
        RegisterStaticSwap("Plant/Plant2", "plant_1.png", "night_plant_1.png");

        _train = GetNodeOrNull<Train>("Train");
        _workstation = GetNodeOrNull<Workstation>("Workstation");

        CallDeferred(nameof(DeferredInit));
    }

    private void DeferredInit()
    {
        if (GameManager.Instance == null)
        {
            GD.PrintErr("[Base] GameManager 未就绪，无法初始化昼夜贴图");
            return;
        }
        RefreshAllTextures();
        GameManager.Instance.TimeChanged += OnTimeChanged;
    }

    public override void _ExitTree()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TimeChanged -= OnTimeChanged;
        }
    }

    private void OnTimeChanged(int period, int day)
    {
        CallDeferred(nameof(RefreshAllTextures));
    }

    private void RefreshAllTextures()
    {
        bool isNight = GameManager.Instance?.CurrentTimePeriod == 3;

        // 静态精灵直接切换贴图
        foreach (var swap in _staticSwaps)
        {
            if (swap.Sprite == null) continue;
            string file = isNight ? swap.NightFile : swap.DayFile;
            var tex = GD.Load<Texture2D>(TexDir + file);
            if (tex != null)
                swap.Sprite.Texture = tex;
        }

        // 动态建筑（有等级的）让它们自己刷新（内部会判断昼夜）
        _train?.RefreshTexture();
        _workstation?.RefreshTexture();
    }

    private void RegisterStaticSwap(string nodePath, string dayFile, string nightFile)
    {
        var sprite = GetNodeOrNull<Sprite2D>(nodePath);
        if (sprite == null)
        {
            GD.PrintErr($"[Base] 未找到节点: {nodePath}");
            return;
        }
        _staticSwaps.Add(new TextureSwap
        {
            Sprite = sprite,
            DayFile = dayFile,
            NightFile = nightFile
        });
    }
}
