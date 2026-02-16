shader_type canvas_item;


uniform int outline_width: hint_range(0, 50, 1) = 10;
uniform vec4 start_outline_color: source_color = vec4(1.0, 0.0, 0.0, 1.0);
uniform vec4 end_outline_color: source_color = vec4 (0.0, 1.0, 0.0, 1.0);

void fragment() {
    float max_alpha = 0.0;
    for(int x = -outline_width; x < outline_width; x++) {
        for(int y = -outline_width; y < outline_width; y++) {
            float texture_x = float(x) * TEXTURE_PIXEL_SIZE.x;
            float texture_y = float(y) * TEXTURE_PIXEL_SIZE.y;
            vec2 offset = vec2(texture_x, texture_y);

            float alpha = texture(TEXTURE, UV + offset).a;
            max_alpha = max(max_alpha, alpha);
        }
    }

    vec4 outline_color = mix(start_outline_color, end_outline_color, UV.x);
	vec4 normal_color = texture(TEXTURE, UV);
    COLOR = mix(max_alpha * outline_color, normal_color, normal_color.a);
}