sampler2D tex0 : register(s0);
sampler2D tex1 : register(s1);
float4 uColor = float4(0.5, 1, 2.5, 1);
float4 uColor2 = float4(0.5, 1.15, 1.5, 1);
float uOpacity;
float uTime;

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 c = tex2D(tex0, uv.x + float2(0, uTime*-0.05));

    float4 c2 = tex2D(tex1, uv + float2(uTime*0.1, 0));
    
    c2 = c2 * tex2D(tex1, uv + float2(uTime*0.2, 0.2))*0.5;
    
    c2 = pow(c2, 2);
    
    float4 final = pow(c,2.5-c2*3)*2 * uv.y;
    
    return final * lerp(uColor2*1.25, uColor,uv.y) * uOpacity;
}

Technique techique1
{
    pass Sky
    {
        PixelShader = compile ps_3_0 main();
    }
}