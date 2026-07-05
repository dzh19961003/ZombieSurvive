# ZombieSurvive 项目约定

## 基本信息
- 引擎：Godot 4.7
- 语言：C#
- 类型：2D 手机游戏（僵尸生存）
- 分辨率：1080×1920，竖屏，Canvas Items 拉伸

## 用户背景
- C# 初学者，Godot 不熟，代码要注释详细、避免过度抽象

## 框架约定
- 根场景 Main.tscn 永不卸载，UIManager/SceneLoader/ConfigManager/SaveManager 全挂在这里
- 不使用 GetTree().ChangeSceneToFile()，统一走 SceneLoader.Instance.GoTo()
- 子场景动态挂入 SceneContainer 节点
- 打开/关闭UI面板用 UIManager.Instance.ShowPanel(路径) / HidePanel(路径)
- 存档系统：实现 ISaveable 接口 + AddToGroup("Save")，SaveManager 自动发现

## 目录结构
- Scripts/Tools/ — 工具类（ConfigManager、UIManager、SceneLoader、SaveManager等）
- Scripts/GameLogic/ — 游戏逻辑
- UI/ — 面板预制体 .tscn
- Scene/ — 游戏子场景 .tscn
- Docs/ — 说明文档
