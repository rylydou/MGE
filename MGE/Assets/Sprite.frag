#version 330 core

uniform sampler2D texture0;

in vec4 vertColor;
in vec2 textureCoord;

out vec4 color;

void main()
{
	color = texture(texture0, textureCoord) * vertColor;
}
