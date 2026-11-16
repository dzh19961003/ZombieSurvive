shader_type canvas_item;
uniform vec4 ink_color : source_color = vec4(0.66, 0.85, 0.95, 1.0);
void fragment() {
    COLOR = vec4(ink_color.rgb, COLOR.a * ink_color.a);
}
