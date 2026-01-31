#version 430 core

struct Agent
{
    vec2 position;
    float angle;
    uint type;
    float energy;
    uint age;
    int state;
    int nnOffset;
    int meals;
    int deaths;
    float energySpent;
    int flag;
    ivec2 currPixel;
    ivec2 prevPixel;
    float memory0;
    float memory1;
    float nearPrey;
    uint age2;
};

layout(std430, binding = 1) buffer AgentsBuffer {
    Agent agents[];
};

uniform mat4 projection;

layout(location=0) out vec3 vColor;

void main()
{
    uint id = gl_VertexID;
    Agent agent = agents[id];
    gl_Position = projection * vec4(agent.position, 0.0, 1.0);
    gl_PointSize = 5.0;
    vColor = vec3(0.3,1,0.3);
    if (agent.type == 1)
        vColor = vec3(0.3,0.3,1);
    else if (agent.type == 2)
        vColor = vec3(1,0.3,0.3);

    if (agent.flag == 1)
    {
        gl_PointSize = 15;
    }

    if (agent.state == 1)
    {
        gl_PointSize = 0;
        vColor = vec3(0,0,0);
    }

}