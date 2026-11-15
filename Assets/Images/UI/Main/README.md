# 主界面 HUD 资源

用于 `UI/MainUI.tscn`、`UI/sta_hunUI.tscn`、`UI/propertyBar.tscn`、`UI/time.tscn`，按确认的预览统一为深蓝底、蓝色像素切角边框和项目默认像素字体。

## 复用

- 主面板、时间面板、属性面板及睡觉按钮共用 `Details/armor_panel.png`，使用四侧 28 像素的九宫格和 `details_opaque.tres`，不重复制作面板图片。
- 字体沿用 `Fonts/font_bold.tres`，通过现有 `building/warehouse/inventory_theme.tres` 继承。
- 床、闪电、拳头、鞋、大脑及心形复用现有图片；心形通过现有图标着色器调整为柔和珊瑚红。
- 空头像框复用 `Details/armor_item_frame.png`。

## 新增扁平像素 SVG

| 文件 | 原始尺寸 | 用途 |
| --- | --- | --- |
| rice_flat.svg | 40 × 40 | 左上角米饭标识 |
| apple_active.svg | 24 × 28 | 饱食度有效格 |
| apple_inactive.svg | 24 × 28 | 饱食度空格 |
| hunger_strip.svg | 502 × 95 | 统一饱食状态底条 |
| health_track.svg | 408 × 48 | 血条底框 |
| health_fill.svg | 384 × 28 | 血条填充 |
| sun_flat.svg | 32 × 32 | 时间面板天气图标 |

SVG 使用简单像素几何形状；所有中文、日期、属性及血量数值仍为动态 Label，沿用原脚本和导出字段。`StaHunUi.cs` 仅替换图标和状态底条的资源路径，饱食度切换、体力数值及点击行为保持不变。共享的 `time.tscn` 在探索界面也沿用该样式。

## 旧资源清理

确认无引用后删除旧 HUD 图片 `UI/New/hunger.png`、`UI/New/foodProgress.png`、`UI/New/foodProgress_2.png` 及对应导入文件。物品系统仍使用的 `Items/rice_icon.png` 保留。

## 验证

- C# 编译通过。
- 在 Godot 中验证饱食度 0–3 的图标和状态名称、体力、血量、属性、日期及时间更新。
- 验证仓库和属性入口、睡觉确认弹窗及取消操作；测试未写入存档。
