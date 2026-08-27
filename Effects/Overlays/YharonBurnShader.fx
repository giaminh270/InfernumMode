sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
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
float uTimeFactor;
float2 uZoomFactor;
float uZoomFactorSecondary;
float uSecondaryLavaPower;
float2 uNoiseReadZoomFactor;
float4 uNPCRectangle;
float2 uActualImageSize0;
float uLavaDetail;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 framedCoords = (coords * uActualImageSize0 - uNPCRectangle.xy) / uNPCRectangle.zw;
    float4 color = tex2D(uImage0, coords);
    
    if (uOpacity <= 0) 
        return color * sampleColor;
    
    float t = uTime * uTimeFactor;
    
    // Tối ưu hóa triệt để: Hủy bỏ vòng lặp for và thay thế bằng Dual-texture Coordinate Perturbation.
    // Việc này rút gọn từ 207 lệnh xuống chỉ còn khoảng 25 lệnh, hoàn toàn tương thích với ps_2_0
    
    // Flow 1
    float2 uv1 = framedCoords * uZoomFactor * uNoiseReadZoomFactor;
    float n1 = tex2D(uImage1, uv1 + t * 0.2).r;
    float flow1 = sin(n1 * 7.0) * 0.5 + 0.5;
    
    // Flow 2 (Tái sử dụng nhiễu từ Flow 1 để bóp méo tọa độ)
    float2 uv2 = framedCoords * uZoomFactor * uZoomFactorSecondary * uNoiseReadZoomFactor;
    float n2 = tex2D(uImage1, uv2 - t * 0.4 + (n1 * 0.1)).r; 
    float flow2 = sin(n2 * 7.0) * 0.5 + 0.5;
    
    float3 burn1 = uColor / (flow1 + 0.001);
    float3 burn2 = uSecondaryColor * pow(abs(flow2), uSecondaryLavaPower);
    
    float4 res = float4(burn1 + burn2, 1.0) * sampleColor;
    return lerp(color * sampleColor, res * color.a, uOpacity);
}

technique Technique1
{
    pass BurnPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}