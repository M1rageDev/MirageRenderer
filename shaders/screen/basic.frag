
#version 330 core
out vec4 FragColor;
  
in vec2 TexCoords;

uniform sampler2D screenTexture;

void main()
{ 
    FragColor = vec4(vec3(texture(screenTexture, vec2(gl_FragCoord.x / 800, gl_FragCoord.y / 600))), 1.0);
}