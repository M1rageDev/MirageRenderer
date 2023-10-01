#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec3 aNormal;

out vec2 TexCoord;
out vec3 Normal;
out vec3 FragPos;

uniform mat4 view;
uniform mat4 projection;
uniform mat4 transform;
uniform float time;

void main()
{
    //vec3 finalPosition = aPosition + vec3(0, sin(time * 0.5 * (aPosition.x+aPosition.z))*0.02, 0);
    vec3 finalPosition = aPosition + vec3(0, sin((aPosition.x+aPosition.z)/1000f*sin(time*0.5f))*0.02, 0);
    gl_Position = vec4(finalPosition, 1f) * transform * view * projection;
    Normal = aNormal;
    FragPos = (vec4(finalPosition, 1f) * transform).xyz;
    TexCoord = aTexCoord;
}