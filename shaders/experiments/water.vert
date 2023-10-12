#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec3 aNormal;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBitangent;

const float PI = 3.1415926535897932384626433832795;

out vec2 TexCoord;
out vec3 FragPos;
out vec3 ViewPos;
flat out vec3 Normal;
flat out vec3 Color;

flat out vec3 Specular;
flat out vec3 Diffuse;

uniform vec3 lightDir;
uniform vec3 viewPos;
uniform float time;

uniform mat4 view;
uniform mat4 projection;
uniform mat4 transform;

float mod289(float x){return x - floor(x * (1.0 / 289.0)) * 289.0;}
vec4 mod289(vec4 x){return x - floor(x * (1.0 / 289.0)) * 289.0;}
vec4 perm(vec4 x){return mod289(((x * 34.0) + 1.0) * x);}

float noise(vec3 p){
    vec3 a = floor(p);
    vec3 d = p - a;
    d = d * d * (3.0 - 2.0 * d);

    vec4 b = a.xxyy + vec4(0.0, 1.0, 0.0, 1.0);
    vec4 k1 = perm(b.xyxy);
    vec4 k2 = perm(k1.xyxy + b.zzww);

    vec4 c = k2 + a.zzzz;
    vec4 k3 = perm(c);
    vec4 k4 = perm(c + 1.0);

    vec4 o1 = fract(k3 * (1.0 / 41.0));
    vec4 o2 = fract(k4 * (1.0 / 41.0));

    vec4 o3 = o2 * d.z + o1 * (1.0 - d.z);
    vec2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);

    return o4.y * d.y + o4.x * (1.0 - d.y);
}

float genWave(float x, float z, float wave, float amplitude) 
{
    float radX = (x / wave + time * 0.5) * 2.0 * PI;
    float radZ = (z / wave + time * 0.5) * 2.0 * PI;

    return amplitude * 0.5 * (sin(radX) + cos(radZ));
}

vec3 applyWaves(vec3 vert, float wave, float amplitude)
{
    float distortion = genWave(vert.x, vert.z, wave, amplitude);
    return vert + vec3(0.0, distortion, 0.0);
}

float calculateSpec(vec3 viewPos, vec3 lightDir, vec3 norm)
{
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 halfwayDir = normalize(lightDir + viewDir);
    return pow(max(dot(norm, halfwayDir), 0.0), 512);
}

float calculateDiff(vec3 lightDir, vec3 norm)
{
    return max(dot(norm, lightDir), 0.0);
}

void main()
{
    vec3 finalPosition = applyWaves(aPosition, 0.03, 0.04);
    vec4 projected = vec4(finalPosition, 1f) * transform * view * projection;

    gl_Position = projected;
    Normal = aNormal;
    FragPos = (vec4(finalPosition, 1f) * transform).xyz;
    TexCoord = aTexCoord;
    ViewPos = viewPos;

    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(-lightDir);
    float spec = calculateSpec(viewPos, lightDir, norm);
    float diff = calculateDiff(lightDir, norm);
    vec3 color = (vec3(133.0, 216.0, 229.0) / vec3(255.0)) - noise(vec3(finalPosition.x, 0f, finalPosition.z) / 10.0) * 0.1;
    Color = color * diff + spec;
}