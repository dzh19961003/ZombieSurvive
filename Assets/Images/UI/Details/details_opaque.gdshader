shader_type canvas_item;
render_mode unshaded;

// These textures contain translucent painted fills. Keep their cut-out outline,
// but make the actual panel solid so inventory items cannot show through.
uniform float silhouette_cutoff : hint_range(0.0, 1.0) = 0.1;
varying vec4 panel_modulate;

void vertex() {
	panel_modulate = COLOR;
}

void fragment() {
	vec4 pixel = texture(TEXTURE, UV);
	float coverage = step(silhouette_cutoff, pixel.a);
	COLOR = vec4(pixel.rgb, coverage) * panel_modulate;
}
