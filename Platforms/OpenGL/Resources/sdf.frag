#version 330

uniform sampler2D u_texture;

in vec2 v_tex;
in vec3 v_color;

out vec4 o_color;

float width = 0.475;
float edge = 0.05;

void main(void)
{
	float dist = 1 - texture(u_texture, _tex).a;

	float alpha = 1 - smoothstep(width, width + edge, dist);

	o_color = vec4(v_color, alpha);
}
