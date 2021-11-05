#version 330 core

uniform sampler2D texture0;

in vec4 color;
in vec2 textureCoord;

out vec4 outputColor;

void main()
{
	outputColor = (1 - texture(texture0, textureCoord)) * color;
}
