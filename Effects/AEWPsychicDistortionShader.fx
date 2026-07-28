sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity : register(C0);
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float4 uShaderSpecificData;

float4 PixelShaderFunction(float4 position : SV_POSITION, float2 coords : TEXCOORD0) : COLOR0
{
    // Distortion nhẹ theo trục X
    coords.x = saturate(coords.x + sin(coords.y * 27.5 + uTime * 6.7) * lerp(0.1, 0.01, uOpacity));
    
    float2 centered = coords - 0.5;
    float dist = length(centered);
    
    // Tối ưu hóa: Dùng sincos thay vì gọi sin và cos rời rạc để tiết kiệm lệnh toán học
    float angle = dist * 16.2 - uTime * 6.0;
    float s, c;
    sincos(angle, s, c);
    
    float2 swirlCoords = float2(
        centered.x * c - centered.y * s,
        centered.x * s + centered.y * c
    ) + 0.5;
    
    float fade = saturate(dist * 3.0) / (uOpacity + 0.0001);
    
    // Tối ưu hóa: Thêm abs() để sửa cảnh báo X3571
    float3 baseColor = lerp(uColor, uSecondaryColor, pow(abs(fade), 0.3));
    
    float4 noise = tex2D(uImage0, swirlCoords);
    float intensity = saturate(noise.r) * (1.0 - fade);
    
    float4 endColor = float4(baseColor, 0.1) * (1.0 + (1.0 - fade) * 3.0);
    return endColor * intensity;
}

technique Technique1
{
    pass DistortionPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}