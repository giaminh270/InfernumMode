sampler uImage0 : register(s0);

texture sampleTexture2;
sampler2D NoiseMap = sampler_state
{
    texture = <sampleTexture2>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

float3 mainColor;
float2 resolution;
float speed;
float zoom;
float dist;
float time;
float opacity;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 res = resolution * 2.0;
    coords -= coords % (1.0 / res);
    
    // Tối ưu hóa phép tính độ dài bằng cách loại bỏ phép trừ đảo ngược
    float2 mappedUv = coords - 0.5;
    float distFromCenter = length(mappedUv) * 2.0;
    
    if (distFromCenter > 1.0)
        return float4(0, 0, 0, 0);
    
    float mainOp = max(0.0, 1.0 - distFromCenter);
    
    // Thay thế pow() bằng cấp số nhân trực tiếp (x*x*x) giảm 8 lệnh toán học
    if (distFromCenter < 0.8) 
    {
        float v = distFromCenter * 1.25; 
        mainOp /= (v * v * v);
    }
    
    float2 d = abs(coords - 0.5);
    float2 bUV = d * d * d;
    float2 blownUpUV = float2(-bUV.y, -bUV.x) * 0.25 + coords * (1.0 + bUV * 0.5);

    // Gộp 4 tex2D thành 2 mẫu để ép số lượng lệnh xuống dưới 64
    float ts = time * speed;
    float n1 = tex2D(NoiseMap, blownUpUV + float2(0, ts)).r;
    float n2 = tex2D(NoiseMap, blownUpUV + float2(ts, 0)).r;
    
    mainOp = (mainOp * mainOp) / ((n1 + n2) * 0.5 + 0.001);

    if (distFromCenter > 0.6) 
    {
        float edge = 1.0 - ((distFromCenter - 0.6) * 2.5);
        edge *= edge;
        mainOp *= (edge * edge);
    }

    // Sử dụng sqrt() nội tại thay cho hàm pow(..., 0.5)
    float3 c = mainColor * min(sqrt(abs(mainOp)), 2.7);
    return float4(c, 1.0) * opacity;
}

technique Technique1
{
    pass FirePass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}