#version 430

layout(location=0) in vec3 vColor;
layout(location=1) flat in int flag;
out vec4 outputColor;

void main()
{
    if ((vColor.r == 0 && vColor.g == 0 && vColor.b == 0) || flag == 0)
        discard;

    vec2 uv = gl_PointCoord * 2.0 - 1.0; 
    float r = length(uv); 
    float w = fwidth(r);

    // Outer circle alpha (soft discard)
    float alpha = 1.0 - smoothstep(1.0 - w, 1.0 + w, r);

    // White rim near the edge
    float rim = smoothstep(0.85 - w, 0.85 + w, r);

    vec4 baseColor = flag == 1 ? vec4(vColor, 1.0) :  vec4(vColor, 0.1);
    vec4 rimColor  = vec4(1.0, 1.0, 1.0, 1.0);

    outputColor = mix(baseColor, rimColor, rim);
    outputColor.a *= alpha;
}