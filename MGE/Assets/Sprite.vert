#version 330

uniform	mat4 u_matrix;

in vec2 a_pos;
in vec2 a_tex;
in vec4 a_col;
in vec3 a_type;

out vec2 v_tex;
out vec4 v_col;

void main()
{
	gl_Position = u_matrix * vec4(a_pos, 0.0, 1.0);

	v_col = a_col;
	v_tex = a_tex;
	v_type = a_type;
}
