#version 330 core

out vec4 FragColor;
in vec3 FragPos;
in vec2 TexCoord;

void main()
{
    vec4 dist = vec4(TexCoord.x, 0f, TexCoord.y, 1);
    FragColor = dist;
}
