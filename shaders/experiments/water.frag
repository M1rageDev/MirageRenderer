#version 330 core

struct DirectionalLight {
    vec3 direction;
  
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

const float edgeSoftness = 0.6;

in vec3 FragPos;
flat in vec3 Normal;
flat in vec3 Color;
in vec2 TexCoord;
in vec3 ViewPos;

out vec4 FragColor;

uniform sampler2D tex;
uniform sampler2D depthTex;
uniform DirectionalLight directionalLight;

float linearizeDepth(float d,float near,float far)
{
    return (2.0 * near * far) / (far + near - (d * 2.0 - 1.0) * (far - near));
}

float waterDepth(vec2 coord) 
{
    float depth = texture(depthTex, coord).x;
    float floorDist = linearizeDepth(depth, 0.1f, 1000.0f);
    depth = gl_FragCoord.z;
    float waterDist = linearizeDepth(depth, 0.1f, 1000.0f);
    return floorDist - waterDist;
}

void main()
{
    float depth = waterDepth(gl_FragCoord.xy / vec2(800, 600));
    depth = clamp(depth / edgeSoftness, 0.0, 1.0);

    float normalDot = dot(normalize(Normal), vec3(0, 1, 0));

    vec4 lightAmount = vec4(Color - depth * 0.1, 0.8);
    FragColor = lightAmount;
}
