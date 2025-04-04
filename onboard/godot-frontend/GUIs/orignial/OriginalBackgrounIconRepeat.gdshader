shader_type canvas_item;

uniform float scale = 1.0;
uniform vec2 direction;

uniform float alphaScale = 1.0;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	float screen_width = 1.0 / SCREEN_PIXEL_SIZE.r;
	float screen_height = 1.0 / SCREEN_PIXEL_SIZE.g;

	// delta movement value
	float dx = -direction.r * mod(TIME, 1.0 / direction.r);
	float dy = direction.g * mod(TIME, 1.0 / direction.g);

	COLOR = texture(TEXTURE, fract(UV * vec2(scale, scale * (screen_height / screen_width))) + vec2(dx, dy));

	COLOR.a *= alphaScale;
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
