﻿#version 330 core

struct DirectionalLight {
    vec3 direction;
  
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

in vec3 FragPos;
in vec3 Normal;
out vec4 FragColor;

uniform vec3 color;
uniform DirectionalLight directionalLight;

void main()
{
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(-directionalLight.direction); 
    vec3 diff = max(dot(norm, lightDir), 0.0) * directionalLight.diffuse;

    vec3 lightAmount = color * diff;
    FragColor = vec4(lightAmount, 1f);
}
