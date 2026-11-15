shader_type canvas_item;

// Recolor existing pixel icons while preserving their alpha and silhouette.
uniform vec4 tint : source_color = vec4(0.48, 0.74, 0.89, 1.0);
uniform float brightness_cutoff : hint_range(0.0, 1.0) = 0.15;

void fragment() {
	vec4 pixel = texture(TEXTURE, UV);
	float brightness = max(pixel.r, max(pixel.g, pixel.b));
	COLOR = vec4(tint.rgb, pixel.a * step(brightness_cutoff, brightness) * tint.a);
}
