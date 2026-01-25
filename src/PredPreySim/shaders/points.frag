#version 430

layout(location=0) in vec3 vColor;
out vec4 outputColor;

void main()
{
    vec2 uv = gl_PointCoord * 2.0 - 1.0; 
    float r = length(uv); 
    if (r > 1.0)
        discard;

    vec3 color = vColor;
    if (r > 0.8)
        color = vec3(0,0,0);
    else if (r > 0.6)
        color = vec3(1,1,1);

    outputColor = vec4(color, 1.0);
}