#version 430

out vec2 uv;

uniform vec2 texSize;
uniform mat4 projection;

void main()
{
    vec2 verts[6] = vec2[](
        vec2(0.0, 0.0),
        vec2(texSize.x, 0.0),
        vec2(texSize.x, texSize.y),

        vec2(0.0, 0.0),
        vec2(texSize.x, texSize.y),
        vec2(0.0, texSize.y)
    );

    vec2 p = verts[gl_VertexID];

    uv = p / texSize;              // Proper UVs
    gl_Position = projection * vec4(p, 0.0, 1.0);
}
