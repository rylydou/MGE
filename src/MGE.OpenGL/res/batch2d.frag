#version 330

uniform sampler2D u_texture;

in vec2 v_tex;
in vec4 v_col;
in vec3 v_type;

out vec4 o_color;

void main(void)
{
	vec4 texColor = texture(u_texture, v_tex);
	texColor = vec4(texColor.rgb * texColor.a, texColor.a);

	o_color =
		v_type.x * texColor * v_col +
		v_type.y * texColor.a * v_col +
		v_type.z * v_col;
}
