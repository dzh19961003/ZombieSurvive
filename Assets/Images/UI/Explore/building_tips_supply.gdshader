shader_type canvas_item;

// 沿用项目图标的轮廓与明暗层次，只把配色统一为 BuildingTips 的浅蓝色。
uniform vec4 ink_color : source_color = vec4(0.66, 0.85, 0.95, 1.0);

void fragment() {
    float lightness = dot(COLOR.rgb, vec3(0.299, 0.587, 0.114));
    float tone = lightness < 0.28 ? 0.30
        : lightness < 0.60 ? 0.72
        : lightness < 0.85 ? 0.88 : 1.0;
    COLOR = vec4(ink_color.rgb * tone, COLOR.a * ink_color.a);
}
