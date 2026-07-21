using Godot;
using System.Collections.Generic;

// ============================================================
//  UIManager — 简单UI管理器
//  作用：统一负责"打开面板"和"关闭面板"，不用在各个脚本里互相找节点
// ============================================================

public partial class UIManager : Node
{
	// 全局单例
	public static UIManager Instance { get; private set; }

	// 记录已经加载过的面板，避免重复创建
	// key = 面板场景的路径，value = 面板节点本身
	private Dictionary<string, Control> _panels = new();

	public override void _Ready()
	{
		Instance = this;
	}

	// ─────────────────────────────────────────────────────────
	//  ShowPanel：打开一个面板
	//
	//  用法示例：
	//    UIManager.Instance.ShowUI(Paths.Shop);
	//
	//  第一次调用时会加载场景并创建节点；
	//  之后再调用同一路径，直接显示已有节点（不重复创建）。
	// ─────────────────────────────────────────────────────────
	public Control ShowUI(string scenePath)
	{
		// 如果这个面板从来没打开过，就创建它
		if (!_panels.ContainsKey(scenePath))
		{        
			var scene = GD.Load<PackedScene>(scenePath);
			if (scene == null)
			{
				GD.PrintErr("找不到这个面板场景：" + scenePath);
				return null;
			}
			var panel = scene.Instantiate<Control>();
			AddChild(panel);          // 加到 UIManager 节点下面
			_panels[scenePath] = panel;
		}

		// 把面板显示出来
		_panels[scenePath].Visible = true;
		return _panels[scenePath];
	}

	// ─────────────────────────────────────────────────────────
	//  HidePanel：隐藏一个面板（节点还在，只是看不见）
	//
	//  用法示例：
	//    UIManager.Instance.HidePanel(UIPaths.Shop);
	// ─────────────────────────────────────────────────────────
	public void HideUI(string scenePath)
	{
		if (_panels.ContainsKey(scenePath))
		{
			_panels[scenePath].Visible = false;
		}
	}

	// ─────────────────────────────────────────────────────────
	//  HideAll：关闭所有面板（切换场景前可以调用）
	// ─────────────────────────────────────────────────────────
	public void HideAll()
	{
		foreach (var panel in _panels.Values)
		{
			panel.Visible = false;
		}
	}
	
}
