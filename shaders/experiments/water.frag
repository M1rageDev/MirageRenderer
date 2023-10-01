#version 330 core

struct DirectionalLight {
    vec3 direction;
  
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;
out vec4 FragColor;

uniform float shininess;
uniform vec3 viewPos;
uniform sampler2D tex;
uniform DirectionalLight directionalLight;
uniform float time;

void main()
{
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(-directionalLight.direction); 
    vec4 diff = max(dot(norm, lightDir), 0.0) * vec4(directionalLight.diffuse, 1f);

    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    vec4 spec = pow(max(dot(viewDir, reflectDir), 0.0), shininess) * vec4(directionalLight.specular, 1f);

    vec4 lightAmount = texture2D(tex, TexCoord * 100f + time * 0.1f) * (diff+spec);
    FragColor = lightAmount;
}
