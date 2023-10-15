#version 330 core

in vec3 ModelPos;
in vec3 Normal;
in vec2 TexCoord;
out vec4 FragColor;

void main()
{
    FragColor = vec4(ModelPos.y, ModelPos.y, ModelPos.y, 1.0);
}
