#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec3 aNormal;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBitangent;

const float seaLevel = 0.3;

out vec2 TexCoord;
flat out vec3 Normal;
out vec3 FragPos;
flat out vec3 OutColor;

uniform mat4 view;
uniform mat4 projection;
uniform mat4 transform;

uniform vec3 terrainColors[4];
uniform float terrainHeights[4];

void main()
{
    gl_Position = vec4(aPosition, 1f) * transform * view * projection;
    Normal = aNormal * mat3(transpose(inverse(transform)));
    FragPos = (vec4(aPosition, 1f) * transform).xyz;
    TexCoord = aTexCoord;

    float yPos = FragPos.y;
    vec3 color1 = terrainColors[0];
    vec3 color2 = terrainColors[0];
    float height1 = terrainHeights[0];
    float height2 = terrainHeights[0];

    for (int i = 0; i < 3; i++) {
        if (yPos >= terrainHeights[i] && yPos < terrainHeights[i+1]) {
            color1 = terrainColors[i];
            color2 = terrainColors[i+1];
            height1 = terrainHeights[i];
            height2 = terrainHeights[i+1];
            break;
        }
    }

    if (yPos > terrainHeights[3]) {
        color2 = terrainColors[3];
        height2 = terrainHeights[3];
    }

    float t = (yPos - height1) / (height2 - height1);
    OutColor = mix(color1, color2, t);

    if (yPos <= seaLevel) {
        OutColor = terrainColors[0];
    }
}