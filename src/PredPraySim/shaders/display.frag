#version 330 core

in vec2 uv;

uniform sampler2D uPlantsImage;
uniform sampler2D uPrayImage;
uniform sampler2D uPredImage;

out vec4 fragColor;

float amplify(float x, int pow)
{
    float a = 1;
    for(int i=0; i<pow; i++)
        a = a * (1-x);

    return 1-a;
}

void main()
{
    float plant = texture(uPlantsImage, uv).r;
    float pray = texture(uPrayImage, uv).r;
    float pred = texture(uPredImage, uv).r;
    
    float r = amplify(pred, 3);
    float g = amplify(plant, 3);
    float b = amplify(pray, 3);
    
    fragColor = vec4(r, g, b ,1);
}