#version 430 core

struct Agent
{
    vec2 position;
    float angle;
    uint type;
};

layout(std430, binding = 1) buffer AgentsBuffer {
    Agent agents[];
};

uniform mat4 projection;

layout(location=0) out vec3 vColor;

void main()
{
    uint id = gl_VertexID;
    gl_Position = projection * vec4(agents[id].position, 0.0, 1.0);
    gl_PointSize = 5.0;
    vColor = vec3(0,1,0);
    if (agents[id].type == 1)
        vColor = vec3(0,0,1);
    else if (agents[id].type == 2)
        vColor = vec3(1,0,0);

}