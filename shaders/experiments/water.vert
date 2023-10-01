#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec3 aNormal;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBitangent;

out vec2 TexCoord;
out vec3 Normal;
out vec3 FragPos;
out vec4 ProjectPos;
out mat3 TBN;

uniform mat4 view;
uniform mat4 projection;
uniform mat4 transform;
uniform float time;

void main()
{
    vec3 finalPosition = aPosition + vec3(0, (sin(aPosition.x*100+time*0.5)+cos(aPosition.z*100+time*0.5))*0.04, 0);
    vec4 projected = vec4(finalPosition, 1f) * transform * view * projection;

    gl_Position = projected;
    ProjectPos = projected;
    Normal = aNormal;
    FragPos = (vec4(finalPosition, 1f) * transform).xyz;
    TexCoord = aTexCoord;

    vec3 T = normalize(vec3(transform * vec4(aTangent, 0.0)));
    vec3 N = normalize(vec3(transform * vec4(aNormal, 0.0)));
    T = normalize(T - dot(T, N) * N);
    vec3 B = cross(N, T);
    
    TBN = mat3(T, B, N);
}