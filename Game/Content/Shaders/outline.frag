#version 330

uniform sampler2D u_texture;
uniform float u_outline_thickness;
uniform vec4 u_outline_color;
uniform float u_outline_threshold;

in vec2 v_tex;
in vec4 v_col;
in vec3 v_type;

out vec4 o_color;

void main(void)
{
	o_color = texture(u_texture, v_tex);

	if (o_color.a <= u_outline_threshold)
	{
		ivec2 size = textureSize(u_texture, 0);

		float uv_x = v_tex.x * size.x;
		float uv_y = v_tex.y * size.y;

		float sum = 0.0;
		for (int n = 0; n < 9; ++n)
		{
			uv_y = (v_tex.y * size.y) + (u_outline_thickness * float(n - 4.5));
			float h_sum = 0.0;
			h_sum += texelFetch(u_texture, ivec2(uv_x - (4.0 * u_outline_thickness), uv_y), 0).a;
			h_sum += texelFetch(u_texture, ivec2(uv_x - (3.0 * u_outline_thickness), uv_y), 0).a;
			h_sum += texelFetch(u_texture, ivec2(uv_x - (2.0 * u_outline_thickness), uv_y), 0).a;
			h_sum += texelFetch(u_texture, ivec2(uv_x - u_outline_thickness, uv_y), 0).a;
			h_sum += texelFetch(u_texture, ivec2(uv_x, uv_y), 0).a;
			h_sum += texelFetch(u_texture, ivec2(uv_x + u_outline_thickness, uv_y), 0).a;
			h_sum += texelFetch(u_texture, ivec2(uv_x + (2.0 * u_outline_thickness), uv_y), 0).a;
			h_sum += texelFetch(u_texture, ivec2(uv_x + (3.0 * u_outline_thickness), uv_y), 0).a;
			h_sum += texelFetch(u_texture, ivec2(uv_x + (4.0 * u_outline_thickness), uv_y), 0).a;
			sum += h_sum / 9.0;
		}

		if (sum / 9.0 >= 0.0001)
		{
			o_color = vec4(u_outline_color, 1);
		}
	}

	float offset = 1.0 / 128.0;

	vec4 col = texture2D(u_texture, v_tex);
	if (col.a > 0.5)
	{
		o_color = col;
	}
	else
	{
		float a =
			texture2D(u_texture, vec2(v_tex.x + offset, v_tex.y)).a +
			texture2D(u_texture, vec2(v_tex.x, v_tex.y - offset)).a +
			texture2D(u_texture, vec2(v_tex.x - offset, v_tex.y)).a +
			texture2D(u_texture, vec2(v_tex.x, v_tex.y + offset)).a;

		if (col.a < 1.0 && a > 0.0)
		{
			o_color = vec4(0.0, 0.0, 0.0, 0.8);
		}
		else
		{
			o_color = col;
		}
	}
}
