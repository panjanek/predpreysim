#version 330 core

uniform sampler2D inGreen;
uniform sampler2D inBlue;
uniform sampler2D inRed;

uniform vec2 uTexelSize;         // (1.0/width, 1.0/height)
uniform float uKernel[25];

layout(location = 0) out vec4 outPlants;
layout(location = 1) out vec4 outPray;
layout(location = 2) out vec4 outPred;

vec4 blur(sampler2D tex, vec2 uv)
{
    vec4 sum = vec4(0,0,0,0);
    float norm = 0;
    int k = 0;
    for (int j = -2; j <= 2; j++)
    {
        for (int i = -2; i <= 2; i++)
        {
            float dx = float(i);
            float dy = float(j);
            vec2 pixelOffset = vec2(dx, dy);
            vec2 offset = pixelOffset * uTexelSize;
            vec2 src = uv + offset;

            if (src.x < 0)
                src.x += 1.0;
            if (src.x > 1.0)
                src.x -= 1.0;
            if (src.y < 0)
                src.y += 1.0;
            if (src.y > 1.0)
                src.y -= 1.0;

            vec4 current = texture(tex, src);
            norm += uKernel[k];
            sum += current * uKernel[k++];
        }
    }

    vec4 result = sum;
    if (result.r < 0)
      result.r = 0;
    if (result.r > 1.0)
      result.r=1.0;

    return result;
}

void main()
{
    vec2 uv = gl_FragCoord.xy * uTexelSize;
    outPlants = blur(inGreen, uv);
    outPray = blur(inBlue, uv);
    outPred = blur(inRed, uv);
}