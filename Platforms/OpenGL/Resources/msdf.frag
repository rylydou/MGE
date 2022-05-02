#version 330

uniform sampler2D u_texture;
uniform float u_pxRange;

in vec2 v_tex;
in vec4 v_col;
in vec3 v_type;

out vec4 o_color;

float median(float r, float g, float b) {
	return max(min(r, g), min(max(r, g), b));
}

float screenPxRange() {
	vec2 unitRange = vec2(u_pxRange) / vec2(textureSize(u_texture, 0));
	vec2 screenTexSize = vec2(1.0) / fwidth(v_tex);

	return max(0.5 * dot(unitRange, screenTexSize), 1.0);
}

void main() {
	vec4 msd = texture(u_texture, v_tex);
	float sd = median(msd.r, msd.g, msd.b);
	float screenPxDistance = screenPxRange() * (sd - 0.5);
	float opacity = clamp(screenPxDistance + 0.5, 0.0, 1.0);

	vec4 color = mix(vec4(0.0), vec4(1.0), opacity);
	o_color =
		v_type.x * color * v_col +
		v_type.y * color.a * v_col +
		v_type.z * v_col;
}
