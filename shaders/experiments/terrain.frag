﻿#version 330 core

struct DirectionalLight {
    vec3 direction;
  
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

in vec3 FragPos;
in vec2 TexCoord;
flat in vec3 Normal;
flat in vec3 OutColor;

out vec4 FragColor;

uniform DirectionalLight directionalLight;

void main()
{
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(-directionalLight.direction); 
    vec3 diff = max(dot(norm, lightDir), 0.0) * directionalLight.diffuse;

    vec4 groundColor = vec4(OutColor, 1.0);
    vec4 lightAmount = groundColor * vec4(diff, 1.0);
    FragColor = lightAmount;
}
