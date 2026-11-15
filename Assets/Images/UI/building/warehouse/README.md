# 仓库与背包样式

界面：`res://UI/WarehouseUI.tscn`、`res://UI/Explore/Warehouse2UI.tscn`、`res://UI/Explore/BagUI.tscn`。

项目原有 `warhouseBg.png` 和 `Explore/ExploreNew/Bag/bagBg.png` 已与参考截图的底图匹配，因此直接复用，没有重新生图。标题、箱子图标、关闭按钮、分页、格子和滚动区域均为独立 UI 节点。

## 共用资源

| 资源 | 用途 |
| --- | --- |
| warhouseBg.png | 蓝色仓库面板；组合界面的上方面板使用九宫格保留标题及边角 |
| crates.png | 仓库标题箱子图标 |
| tabNormal.png / tabSelectedl.png | 分类分页两态底图 |
| all / food / medicine / equip / tools 的 `_1`、`_2` 图片 | 分类分页两态图标 |
| inventory_slot.png | 共用蓝色物品格，复用原背包格素材，166 × 166 |
| inventory_selection.png | 共用橙色选中框，复用原背包选中框素材 |
| inventory_theme.tres | 默认像素字体、无底色关闭按钮、细长蓝色滚动条 |
| badge_common / badge_uncommon / badge_rare / badge_epic.svg | 四档稀有度的紧凑切角底板，沿用绿、蓝、紫、橙分级 |
| badge_quantity.svg | 深蓝色数量底板，与格子边框统一配色 |
| `res://Assets/Images/UI/Details/close.svg` | 仓库与背包共用蓝色关闭图标 |

## 布局与行为

- 普通仓库为五列，格子间距 12，保留原最少 35 格和分类、排序、数量刷新及打开物品详情的逻辑。
- `UI/InventoryItem.tscn` 继承原 `UI/item.tscn`，仅把仓库使用的格子适配为 166 × 166。两份仓库脚本只替换所加载的场景路径，没有改动筛选或物品数量逻辑。
- `Warehouse2UI` 保留上方五列仓库、下方四列探索背包及容量显示，仍为仓库点击装入、背包点击移出。下方背包框使用 `NinePatchRect`，左右切片边距 110、上边距 385、下边距 190，保留把手、标题栏和底部扣件的原始比例，中间纵向平铺。上方仓库高 780、下方背包高 980，内容通过各自的滚动区域查看；没有压缩整张背包图，也没有生成短版图片。
- `BagUI` 使用四列、水平间距 8、垂直间距 14；新增滚动容器以适应容量增长。实际格数继续读取 `PlayerManager.BagNum`，没有为了参考图的占位行数修改容量。
- 背包单选、再次点击取消、空格不选中、数量显示、关闭时删除界面等行为均保留。
- 所有文字使用 `res://Assets/Fonts/font_bold.tres`。

## 格内标签

`UI/InventoryItem.tscn` 单独覆盖仓库格子的标签样式，两个仓库界面同步生效：

- 稀有度底板改为 52 × 52 的切角色底，位置为 (11, 9)，留出均匀内边距。
- 类型图标本身的纹理、坐标和尺寸完全沿用原 `UI/item.tscn`，没有修改。
- 数量底板改为 60 × 28 的深蓝底，右、下均内缩 10；数量文字在底板内居中，使用 22 号默认像素字体。
- 这些底板为简单 SVG 几何资源，没有调用生图；物品读取、稀有度切换和数量赋值逻辑不变。

## 已清理的旧资源

完成文件路径与 UID 引用检查后，删除以下 6 张旧图片及各自 `.import`：

- `warehouse/item.png`：替换为共用格子。
- `Bag/bagItem.png`：迁移为共用格子，避免重复保留。
- `Bag/itemChoose.png`：迁移为共用选中框，避免重复保留。
- `Bag/close.png`：替换为现有蓝色关闭图标。
- `warehouse/scroll.png`、`warehouse/scrollBg.png`：由主题样式绘制滚动条，不再使用图片。

仍被使用的底图、分类图标及分类按钮资源保留。分类、转移、实际容量、数量、选中、滚动、物品详情和关闭行为已通过运行检查。
