#version 430

layout(location=0) in vec3 vColor;
layout(location=1) flat in int flag;
layout(location=2) flat in float pointSize;
out vec4 outputColor;

void main()
{
    if ((vColor.r == 0 && vColor.g == 0 && vColor.b == 0) || flag == 0)
        discard;

    vec2 uv = gl_PointCoord * 2.0 - 1.0; 
    float r = length(uv); 
    float w = fwidth(r);

    if (flag == 1 || flag == 2) // one circle
    {

        // Outer circle alpha (soft discard)
        float alpha = 1.0 - smoothstep(1.0 - w, 1.0 + w, r);

        // White rim near the edge
        float rim = smoothstep(0.85 - w, 0.85 + w, r);

        vec4 baseColor = flag == 1 ? vec4(vColor, 1.0) :  vec4(0);
        vec4 rimColor  = vec4(1.0, 1.0, 1.0, 1.0);

        outputColor = mix(baseColor, rimColor, rim);
        outputColor.a *= alpha;
    }
    else if (flag == 3) // best performer - two rings
    {
        float r1 = 0.2;   // inner circle
        float r2 = 0.9;   // outer circle

        // Ring thickness (in radius units)
        //float t = 0.1;
        float t = 2 * (1.0 / pointSize);

        // Anti-aliased rings
        float ring1 = smoothstep(r1 - t - w, r1 - t + w, r) - smoothstep(r1 + t - w, r1 + t + w, r);
        float ring2 = smoothstep(r2 - t - w, r2 - t + w, r) - smoothstep(r2 + t - w, r2 + t + w, r);

        // Combine rings
        float alpha = clamp(ring1 + ring2, 0.0, 1.0);

        // Fully transparent outside the rings
        outputColor = vec4(1.0, 1.0, 1.0, alpha);
    }
}