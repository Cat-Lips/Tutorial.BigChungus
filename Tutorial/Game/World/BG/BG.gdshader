shader_type spatial;
render_mode specular_disabled;

// UNIFORMS

uniform vec3 color_grid : source_color = vec3(0.8, 0.8, 0.8);
uniform vec3 color_tile : source_color = vec3(0.102, 0.102, 0.102);
uniform float small_grid_size : hint_range(1.0, 1000.0) = 60.0;
uniform float large_grid_size : hint_range(1.0, 200.0) = 10.0;
uniform float line_thickness : hint_range(0.001, 0.1) = 0.02;
uniform float fade_start_distance : hint_range(0.0, 200.0) = 25.0;
uniform float fade_end_distance : hint_range(0.0, 500.0) = 30.0;

varying vec3 world_pos;

// FUNCTIONS

float grid_line(vec2 uv, float grid_size, float thickness) {
    vec2 grid_uv = uv * grid_size;
    vec2 f = fract(grid_uv);
    float dist_to_line = min(min(f.x, 1.0 - f.x), min(f.y, 1.0 - f.y));
    return 1.0 - smoothstep(0.0, thickness, dist_to_line);
}

// SHADERS

void vertex() {
    world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    float small_line = grid_line(UV, small_grid_size, line_thickness);
    float large_line = grid_line(UV, large_grid_size, line_thickness);
    float distance_to_camera = length(CAMERA_POSITION_WORLD - world_pos);
    float fade = smoothstep(fade_start_distance, fade_end_distance, distance_to_camera);
    float line = max(small_line * (1.0 - fade), large_line * fade);

    ALBEDO = mix(color_tile, color_grid, clamp(line, 0.0, 1.0));
    ROUGHNESS = 1.0;
}