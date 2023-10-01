#version 330 core

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;
out vec4 FragColor;

void main()
{
    FragColor = vec4(FragPos.y, FragPos.y, FragPos.y, 1.0);
}
