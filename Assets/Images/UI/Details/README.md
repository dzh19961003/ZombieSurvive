# 物品详情共用 UI 资源

场景：`res://UI/DetailsTag/DetailsArmer.tscn`、`res://UI/DetailsTag/DetailsFood.tscn`、`res://UI/DetailsTag/DetailsWeapon.tscn`、`res://UI/DetailsTag/DetailsMedic.tscn`、`res://UI/DetailsTag/DetailsMaterial.tscn`、`res://UI/DetailsTag/DetailsJewelry.tscn`。仅更换场景的视觉与布局，继续使用各自原有 C# 脚本和导出绑定。

| 文件 | 尺寸 | 用途 |
| --- | --- | --- |
| armor_panel.png | 396 × 588 | 主面板 |
| armor_item_frame.png | 160 × 156 | 四档稀有度共用底图，场景分别着色 |
| armor_attribute_card.png | 116 × 102 | 三张属性卡片 |
| armor_base_row.png | 364 × 34 | 基础护甲行 |
| armor_effect_panel.png | 364 × 90 | 特殊效果区域 |
| armor_equip_button.png | 336 × 46 | 装备按钮底图 |
| armor_header.svg | 36 × 36 | 像素护甲标题图标 |
| close.svg | 32 × 32 | 像素关闭图标 |
| details_theme.tres | — | 共用项目默认像素字体 `res://Assets/Fonts/font_bold.tres` |
| details_icon_tint.gdshader | — | 将现有碗、沙漏图标着色为蓝色，复用其透明轮廓 |
| details_opaque.gdshader / details_opaque.tres | — | 共用不透明面板材质，阻止后方物品透出，保留切角轮廓 |

PNG 素材使用内置 image_gen 生成；Godot Image 裁切按钮透明边距，按目标尺寸重采样，卡片采用九宫格重采样保留边框。SVG 为简单像素 UI 几何形状。文本为实时 Label，不包含在图片内。

物品大图在编辑器中使用 `jacket1_icon.png` / `rice_icon.png` / `knife1_icon.png` / `bandage_icon.png` / `wood_icon.png` / `ring1_icon.png` 占位，运行时由 `InitialTips` 读取物品配置。拳头、鞋子、大脑复用 `UI/New/` 现有图片。六个界面的所有 Label（含状态说明和数字）统一使用项目默认像素字体，没有系统字体替换或额外加粗。

右上角关闭按钮转发至原 `Bg2.pressed`，沿用关闭和清除选中边框的逻辑。原装备、食用、药品使用按钮均未绑定 pressed 操作，本次保留原状。共用 StateTips 场景和所有业务脚本均未修改。

## 背景不透明度

生成的面板原图含有半透明填充，六个详情界面的主背景、物品底框、属性卡片、效果区及状态卡片统一使用 `details_opaque.tres`。材质保留原 RGB 纹理和稀有度着色，将可见面板区域的 Alpha 固定为 1，滤除透明切角中的微弱残留像素。图标、文字和物品图片不使用此材质。

`DetailsWeapon` / `DetailsMedic` / `DetailsJewelry` 自动继承各自主界面的不透明背景；武器、护甲及饰品的独立状态说明背景也已覆盖。已对六个详情界面和可见状态卡片逐像素检查内部 Alpha 为 1，并确认外部切角仍透明，仓库叠加预览不会透出后方物品。无需修改原 PNG 或业务脚本。

## 食物详情复用方式

- 主面板：`armor_panel.png` 使用 `NinePatchRect`，四侧切片边距均为 28，显示尺寸 396 × 396。
- 下方状态说明：同一张 `armor_panel.png` 使用相同九宫格，显示尺寸 396 × 114。
- 右侧获得状态：`armor_effect_panel.png` 使用四侧边距 7 的九宫格，显示尺寸 164 × 89。
- 物品稀有度框：同一张 `armor_item_frame.png` 缩放为 188 × 188，保留四档显隐与着色。
- 食用按钮：同一张 `armor_equip_button.png` 缩放为 348 × 48。
- 碗图标、沙漏复用项目已有 `UI/New/itemType_1_1.png`、`UI/state_time.png`；仅用着色器统一配色。
- 下方状态卡片是食物场景内的 `StateTips` 脚本实例，保留原状态名称、说明、持续时间的赋值、正负状态颜色和显隐逻辑，不影响其他界面的状态卡片。

本轮食物详情未生成或复制任何新的 PNG / SVG 图片。

## 武器与药品详情复用方式

- `DetailsWeapon.tscn` 直接继承 `DetailsArmer.tscn`，复用全部面板、卡片、按钮、布局和默认像素字体。覆盖类型图标、“武器”、“总伤害”、“基础伤害”及编辑器占位图。
- 武器下方原有 StateTips 实例保留原绑定，只在派生场景内隐藏旧底图，加入复用主面板图片的九宫格，并调整状态名称、说明、时间的位置与样式。共用 `StateTips.tscn` 本身不变。
- `DetailsMedic.tscn` 直接继承 `DetailsFood.tscn`，只覆盖药品图标、编辑器占位图、“生命值回复”和“使用”文字。
- 标题使用现有 `UI/ItemType/weapon_4.png` / `medic_2.png`，配合已有着色器转成蓝色轮廓；运行时物品分类图标仍由原脚本读取。
- 两个场景均保留原路径、资源 UID、C# 类型、导出字段和原数值计算。已用原场景副本对照验证动态文本、稀有度、物品图标、状态显隐和描述、时间、正负状态颜色及关闭行为。

本轮武器与药品详情未生成或复制任何新的图片素材。

## 材料与饰品详情复用方式

- `DetailsMaterial.tscn` 使用共用主面板、物品框、说明底图和关闭按钮，主面板尺寸 396 × 350，保留“用于工作台制作物品或升级家具”的原说明。没有新增材料使用按钮。
- `DetailsJewelry.tscn` 继承材料详情的紧凑布局，主面板高度改为 414，覆盖原有 `DetailsJewelry.cs` 脚本及其全部导出绑定，显示“饰品详情／饰品／特殊效果”和原有装备按钮。
- 标题图标复用 `UI/ItemType/material_4.png` 和 `Items/ring1_icon.png`，沿用共用着色器的蓝色轮廓。运行时分类图标、名称、物品图标和饰品效果仍从原配置读取。
- `DetailsStateCard.tscn` 继承原 `StateTips.tscn`，只调整布局、默认字体和外观，复用同一张主面板的九宫格；供饰品下方可选状态卡片使用。原有 `StateTips` 类型、导出字段、文本默认值及方法不变，共用原场景未修改。
- 饰品关闭操作仍只关闭饰品界面，不会执行材料界面的关闭逻辑。原装备按钮的信号行为保留。
- 已与改造前的场景副本对照验证材料切换、四档稀有度、饰品效果和名称颜色、状态说明绑定、缓存重开及两种关闭方式。

本轮材料与饰品详情未生成或复制任何新的图片素材，也未修改业务脚本或物品配置。

## 生成提示词

参考图为用户提供的装备详情截图，只参考配色、材质与边框风格；所有输出均要求透明外部背景，无文字、图标或内嵌功能。

### Panel

Use case: ui-mockup. Generate ONE reusable blank game UI panel texture, portrait aspect 2:3, 1024x1536. Reference is the supplied armor details screenshot: match its dark navy blue/teal interior, restrained subtle mottled texture, cyan-blue pixel stepped chamfered border, thin black outer edge. Panel fills canvas edge-to-edge with transparent pixels only outside the stepped corners. Straight front-on 2D pixel art UI, crisp edges. All four corners symmetrically stepped. Blank empty interior, uniform subtle texture. NO text, NO icons, NO buttons, NO sub-panels, NO dividing lines, NO ornaments. Border thickness approximately 7px on 1024px canvas, outer dark shadow 6px. Interior #082735 to #0c3448, border #5199bb. This is a production nine-slice background asset for Godot. Reference image only defines STYLE, do NOT reproduce contents.

### Card

Use case: ui-mockup. Generate ONE reusable blank rectangular game UI card texture, landscape aspect 3:2, 1536x1024. Match supplied screenshot dark navy blue pixel-art armor detail panels. Empty subtly mottled blue-teal fill (#0b3042), thin clean cyan blue (#5199bb) border, symmetric tiny pixel stepped chamfers about 32px at all corners. Texture fills canvas edge-to-edge, transparent only outside stepped corner silhouettes. Flat front-on 2D sprite. Border 7px. NO text NO symbols NO icons NO ornaments NO dividers NO subpanels. This is a production nine-slice UI card.

### Button

Use case: ui-mockup. Generate ONE blank orange primary action button texture matching orange equip button in supplied screenshot. Ultra-wide horizontal 6:1 rectangular button, front-on flat 2D pixel art UI sprite. Bright saturated orange #ff8019 fill with very subtle warm texture, light apricot 5px upper/inner rim and darker orange lower edge. All four corners symmetrically cut into crisp pixel staircase chamfers of about 25px on 1536 wide asset. Canvas tightly fits button, transparency only outside stepped corners. NO text NO symbols NO icons NO drop shadow outside bounds NO extra objects. Button must be completely blank, production Godot texture.
