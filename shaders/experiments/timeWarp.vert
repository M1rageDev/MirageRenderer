﻿#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec3 aNormal;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBitangent;

out vec2 TexCoord;
out vec3 Normal;
out vec3 FragPos;

uniform mat4 view;
uniform mat4 projection;
uniform mat4 transform;
uniform float time;

void main()
{
    gl_Position = vec4(aPosition + vec3(sin(aPosition.y + time), 0, 0), 1f) * transform * view * projection;
    Normal = aNormal;
    FragPos = (transform * vec4(aPosition, 1f)).xyz;
    TexCoord = aTexCoord;
}