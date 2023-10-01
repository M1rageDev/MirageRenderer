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

uniform sampler2D sandTex;
uniform sampler2D grassTex;

uniform DirectionalLight directionalLight;

void main()
{
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(-directionalLight.direction); 
    vec3 diff = max(dot(norm, lightDir), 0.0) * directionalLight.diffuse;

    vec4 groundColor = mix(texture(sandTex, TexCoord), texture(grassTex, TexCoord), FragPos.y);
    vec4 lightAmount = groundColor * vec4(diff, 1.0);
    FragColor = vec4(lightAmount.xyz, 1.0);
}
