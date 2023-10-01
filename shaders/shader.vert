#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec3 aNormal;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBitangent;

out vec2 TexCoord;
out vec3 Normal;
out vec3 FragPos;
out mat3 TBN;

uniform mat4 view;
uniform mat4 projection;
uniform mat4 transform;

void main()
{
    gl_Position = vec4(aPosition, 1f) * transform * view * projection;
    Normal = aNormal * mat3(transpose(inverse(transform)));
    FragPos = (vec4(aPosition, 1f) * transform).xyz;
    TexCoord = aTexCoord;

    vec3 T = normalize(vec3(transform * vec4(aTangent, 0.0)));
    vec3 B = normalize(vec3(transform * vec4(aBitangent, 0.0)));
    vec3 N = normalize(vec3(transform * vec4(aNormal, 0.0)));
    TBN = mat3(T, B, N);
}