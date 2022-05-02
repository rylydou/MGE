#version 330

uniform mat4 u_matrix;

uniform float lineLength;

in vec2 a_position;
in vec2 a_tex;
in vec4 a_color;
in vec3 a_type;

out vec2 v_tex;
out vec3 v_color;

void main(void)
{
	gl_Position = u_matrix * vec4(round(a_position), 0.0, 1.0);

	v_tex = a_tex;

	float lineColor = 0.8;
	if(lineLength > 0)
	{
		lineColor = smoothstep(0, lineLength, position.x);
	}

	passColor = vec3(lineColor, 0.4, 0.2);
}
