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
in vec4 ProjectPos;
out vec4 FragColor;

uniform vec3 viewPos;
uniform sampler2D tex;
uniform DirectionalLight directionalLight;
uniform float time;

float linearize_depth(float d,float zNear,float zFar)
{
    float z_n = 2.0 * d - 1.0;
    return 2.0 * zNear * zFar / (zFar + zNear - z_n * (zFar - zNear));
}

void main()
{
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(-directionalLight.direction); 
    vec4 diff = max(dot(norm, lightDir), 0.0) * vec4(directionalLight.diffuse, 1f);

    // blinn phong
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 halfwayDir = normalize(lightDir + viewDir);
    vec4 spec = pow(max(dot(norm, halfwayDir), 0.0), 2048) * vec4(directionalLight.specular, 1f);

    float depth = linearize_depth(ProjectPos.z, 0.1, 1000);

    vec4 lightAmount = vec4(texture2D(tex, TexCoord * 100f + time * 0.1f).xyz, 0.7) * (diff) + spec;
    FragColor = vec4(vec3(depth), 1);
}
