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
- 探索进度：统一用 GameManager 的 GetExploreProgress / AddExploreProgress / SetExploreProgress
  读写，禁止在别处直接 exploreProgress[房间ID]（房间没记录过会 KeyNotFoundException）
- 类型坑：多个脚本只 using 了 Godot.Collections，没 using System.Collections.Generic，
  写 Dictionary<int,int> 实际是 Godot.Collections.Dictionary，报错栈会指向引擎 Dictionary.cs
  input_event 仍会触发。UIManager.IsSceneClickBlocked() 自动扫描直接子节点判定
  （Visible=true + mouse_filter≠Ignore + 宽高各占屏幕80%以上），新增面板无需写任何代码；
  建筑脚本点击前调一次即可。MainUI 因 mouse_filter=Ignore 天然放行

## 转场/淡入节奏控制
- 纯代码创建全屏 Control 用 CanvasLayer(Layer=100) 挂到 UIManager.Instance 下，保证盖住所有面板
- 控制节奏用 Task.Delay(16) 按帧步进，和 TextTyper 的淡入写法一致（项目已验证可用）
- StartMenu.OpenDiary() 是"日记前转场"的统一收口（新游戏/继续/睡觉 三条路径都走它）
- 纯代码建 Control 的锚点要用 SetAnchorsPreset(Control.LayoutPreset.FullRect, false)，
  不能用属性初始化器里的 AnchorsPreset（类型转换报 CS0266）；GrowDirection 同理别强转 int

## 目录结构
- Scripts/Tools/ — 工具类（ConfigManager、UIManager、SceneLoader、SaveManager等）
- Scripts/GameLogic/ — 游戏逻辑
- UI/ — 面板预制体 .tscn
- Scene/ — 游戏子场景 .tscn
- Docs/ — 说明文档
