# -*- coding: utf-8 -*-
"""UI 视觉验证：把设计图和成品图对齐到同一分辨率，输出并排图、局部放大图和叠加图。

用法：
    python Tools/ui_visual_check.py <设计图> <成品图> [输出目录] [--crop-left N] [--preset 名称] [--measure]

    --crop-left N   设计图左边有 N 像素白边时先裁掉（房间选择设计图是 19）
    --preset 名称   选区域预设：default=房间选择，strategy=探索策略，choose=探索详情；新界面在 REGION_PRESETS 里加一组即可
    --measure       额外打印两图的"蓝色边框长线 / 橙色元素"坐标，方便量化尺寸

例：
    python Tools/ui_visual_check.py C:/Users/wp/Desktop/_doc_images/img01_row81.png \
        .workbuddy/preview/RoomChooseUI_preview_v2.png \
        .workbuddy/preview/visual_check --crop-left 19 --measure

输出文件统一放在输出目录，文件名前缀 final_ / 自定义：
    01_full.png     整屏并排（缩到 0.5 便于整体看）
    00_overlay.png  两张图各 50% 叠加，看元素错位
    02~06_*.png     常用区域放大对比
    report.txt      本次参数与区域清单

约定：所有对比图都是【左设计图 / 右成品】，坐标已统一到 1080x1920。
"""
import os
import sys
from PIL import Image, ImageDraw

W, H = 1080, 1920
BG = (24, 24, 24)

# 常用检查区域：(文件名, 左,上,右,下, 放大倍数)
# 每个界面加一组预设，用 --preset 切换，不用每次改脚本
REGION_PRESETS = {
    "default": [   # 第三大点 房间选择界面
        ("02_topbar", 0, 0, 1080, 420, 1),
        ("03_title", 280, 0, 820, 200, 2),
        ("04_cards", 80, 680, 1000, 1300, 1),
        ("05_bottom", 0, 1300, 1080, 1920, 1),
        ("06_bag", 0, 180, 260, 420, 2),
    ],
    "strategy": [  # 第四大点 探索策略界面
        ("02_header_roominfo", 0, 0, 1080, 400, 1),
        ("03_roominfo", 260, 140, 830, 380, 2),
        ("04_cards", 100, 730, 1000, 1320, 1),
        ("05_bottom", 0, 1300, 1080, 1920, 1),
    ],
    "choose": [    # 第五大点 探索详情界面
        ("02_header_roominfo", 0, 0, 1080, 400, 1),
        ("03_event_float", 0, 760, 1080, 1110, 1),
        ("04_des", 0, 1090, 1080, 1510, 1),
        ("05_options", 0, 1500, 1080, 1840, 1),
        ("06_option_right", 820, 1500, 1080, 1840, 2),
    ],
}


def normalize(path, crop_left=0):
    """Excel 里抠出来的设计图常带白边、且被非等比压扁，统一拉到 1080x1920 才能直接量坐标"""
    im = Image.open(path).convert("RGB")
    if crop_left:
        im = im.crop((crop_left, 0, im.width, im.height))
    return im.resize((W, H), Image.LANCZOS)


def label(canvas, text, x, y):
    ImageDraw.Draw(canvas).text((x + 6, y + 4), text, fill=(255, 210, 90))


def side_by_side(a, b, box, zoom=1, tag=True):
    ca, cb = a.crop(box), b.crop(box)
    w, h = ca.size
    c = Image.new("RGB", (w * 2 + 12, h), BG)
    c.paste(ca, (0, 0))
    c.paste(cb, (w + 12, 0))
    if tag:
        label(c, "DESIGN", 0, 0)
        label(c, "CURRENT", w + 12, 0)
    if zoom != 1:
        c = c.resize((int(c.width * zoom), int(c.height * zoom)), Image.LANCZOS)
    return c


def bands(v, threshold, gap=3):
    idx = [int(i) for i in range(len(v)) if v[i] > threshold]
    if not idx:
        return []
    out, s, p = [], idx[0], idx[0]
    for i in idx[1:]:
        if i > p + gap:
            out.append((s, p))
            s = i
        p = i
    out.append((s, p))
    return out


def measure(name, im):
    """打印蓝色边框长线、橙色元素的行列区间，用来核对框和圆点的实际像素尺寸"""
    import numpy as np
    a = np.array(im).astype(int)
    r, g, b = a[:, :, 0], a[:, :, 1], a[:, :, 2]
    blue = (b > 150) & (b > r + 50) & (g > r + 20) & (g > 90) & (r > 55) & (r < 190)
    orange = (r > 210) & (g > 110) & (g < 205) & (b < 110)
    print(f"[{name}] 蓝色边框 行:", bands(blue.sum(1), W * 0.25))
    print(f"[{name}] 蓝色边框 列:", bands(blue.sum(0), H * 0.10))
    print(f"[{name}] 橙色元素 行:", bands(orange.sum(1), 3))
    print(f"[{name}] 橙色元素 列:", bands(orange.sum(0), 2))


def main():
    args = [x for x in sys.argv[1:]]
    crop_left, do_measure, preset = 0, False, "default"
    if "--preset" in args:
        i = args.index("--preset")
        preset = args[i + 1]
        del args[i:i + 2]
    if "--measure" in args:
        do_measure = True
        args.remove("--measure")
    if "--crop-left" in args:
        i = args.index("--crop-left")
        crop_left = int(args[i + 1])
        del args[i:i + 2]
    if len(args) < 2:
        print(__doc__)
        return
    design, current = args[0], args[1]
    out = args[2] if len(args) > 2 else ".workbuddy/preview/visual_check"
    os.makedirs(out, exist_ok=True)

    a = normalize(design, crop_left)
    b = normalize(current)

    full = side_by_side(a, b, (0, 0, W, H), 0.5)
    full.save(os.path.join(out, "01_full.png"))

    # 50/50 叠加：元素位置不一致时会看到重影
    Image.blend(a, b, 0.5).save(os.path.join(out, "00_overlay.png"))

    regions = REGION_PRESETS.get(preset, REGION_PRESETS["default"])
    for name, l, t, r, bo, zoom in regions:
        side_by_side(a, b, (l, t, r, bo), zoom).save(os.path.join(out, name + ".png"))

    with open(os.path.join(out, "report.txt"), "w", encoding="utf-8") as f:
        f.write("设计图: %s (crop_left=%d)\n成品图: %s\n归一化: %dx%d\n区域预设: %s\n"
                % (design, crop_left, current, W, H, preset))
        f.write("输出: 01_full / 00_overlay / " + ", ".join(x[0] for x in regions) + "\n")
        f.write("约定: 左=设计图, 右=成品\n")

    if do_measure:
        measure("设计图", a)
        measure("成品", b)
    print("已输出到", out)


if __name__ == "__main__":
    main()
