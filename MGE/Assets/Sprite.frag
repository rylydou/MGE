#version 330

uniform sampler2D u_texture;

out vec2 v_tex;
out vec4 v_col;
out vec4 v_type;

out vec4 o_color;

void main()
{
	vec4 color = texture(u_texture, v_tex);
	o_color =
		v_type.x * color * v_col +
		v_type.y * color.a * v_col +
		v_type.z * v_col;
}
