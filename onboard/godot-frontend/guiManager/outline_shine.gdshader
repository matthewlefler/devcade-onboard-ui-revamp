shader_type canvas_item;


uniform int outline_width: hint_range(0, 50, 1) = 10;
uniform vec4 start_outline_color: source_color = vec4(1.0, 0.0, 0.0, 1.0);
uniform vec4 end_outline_color: source_color = vec4 (0.0, 1.0, 0.0, 1.0);

uniform float shine_speed = 0.4f;
uniform float shine_width = 0.05f;
uniform int num_segments = 8;

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

    float mod_val = max(TEXTURE_PIXEL_SIZE.x, TEXTURE_PIXEL_SIZE.y);
    float d = TIME * -shine_speed;

    for(int i = 0; i < num_segments; i++) {
        if(UV.x > mod(d + (float(i) * shine_width / float(num_segments)), 1.0) && UV.x < mod(d + (float(i + 1) * shine_width / float(num_segments)), 1.0)) {
            COLOR = mix(vec4(1.0,1.0,1.0,1.0 * COLOR.a), COLOR, 1.0 * float(i) / float(num_segments));
        }
    }

    vec4 tex_color = texture(TEXTURE, UV);
    if(tex_color.r < 0.001 && tex_color.g < 0.0001 && tex_color.b < 0.0001 && tex_color.a > 0.001) {
        COLOR = vec4(0.0,0.0,0.0,1.0);
    }
}