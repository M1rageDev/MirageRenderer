﻿#version 330 core

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;
out vec4 FragColor;

void main()
{
    FragColor = vec4(vec3(gl_FragCoord.z), 1.0);
}
