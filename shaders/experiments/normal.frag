#version 330 core

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;
out vec4 FragColor;

uniform sampler2D normalMap;

void main()
{
    vec3 normal = texture(normalMap, TexCoord / 640f).xyz;
    FragColor = vec4(normal, 1.0);
}
