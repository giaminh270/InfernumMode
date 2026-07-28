float3 color;
float3 darkerColor;
float bloomSize;
float bloomMaxOpacity;
float bloomFadeStrenght;
float mainOpacity;
float laserAngle;
float laserWidth;
float laserLightStrenght;
float noiseOffset;

float2 Resolution;

texture sampleTexture;
sampler2D Texture1Sampler = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture sampleTexture2;
sampler2D NoiseMap = sampler_state { texture = <sampleTexture2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

// Ðã lo?i b? các hàm custom Mod, realCos, distanceFromLine do quá ng?n tài nguyên.

float4 main(float2 uv : TEXCOORD) : COLOR
{
    // Pixelate
    uv.x -= uv.x % (1 / (Resolution.x * 2));
    uv.y -= uv.y % (1 / (Resolution.y * 2));
    float2 mappedUv = float2(uv.x - 0.5, (1 - uv.y) - 0.5);
    
    float halfLaserWidth = laserWidth / 2;
    float distanceFromCenter = length(mappedUv) * 2;
    
    if (distanceFromCenter > 1)
        return float4(0, 0, 0, 0);
    
    // T?i uu hóa C?C M?NH b?ng Dot Product thay vì Atan2 và Modulo
    float2 laserDir;
    sincos(laserAngle, laserDir.y, laserDir.x);
    float2 laserNormal = float2(-laserDir.y, laserDir.x);
    
    // Kho?ng cách t? di?m d?n du?ng th?ng vô h?n
    float distFromLine = abs(dot(mappedUv, laserNormal));
    
    // C?t tia laser (ch? ch?y v? phía tru?c). Thay th? cho hàm distanceFromLineCropped
    float dotDir = dot(mappedUv, laserDir);
    if (dotDir < 0) 
    {
        distFromLine = length(mappedUv);
    }
    
    if (distFromLine > bloomSize + halfLaserWidth)
        return float4(0, 0, 0, 0);
    
    float4 noise = tex2D(NoiseMap, float2((distanceFromCenter + noiseOffset) % 1, distFromLine / (bloomSize + halfLaserWidth)));
    float3 laserColor = lerp(color, darkerColor, noise.r);
    float laserOpacity = (1.0 - pow(abs(distanceFromCenter), laserLightStrenght)) * mainOpacity;
    
    if (distFromLine <= halfLaserWidth)
        return float4(laserColor * laserOpacity, laserOpacity);
    
    float bloomBlendFactor = pow(abs(1.0 - (distFromLine - halfLaserWidth) / bloomSize), bloomFadeStrenght);
    float3 colour = lerp(float3(0, 0, 0), laserColor, bloomBlendFactor);
    float opacity = lerp(0, laserOpacity, bloomBlendFactor) * mainOpacity * bloomMaxOpacity;
    
    colour = colour * opacity;
    return float4(colour, opacity);
}

technique Technique1
{
    pass SightLinePass
    {
        PixelShader = compile ps_2_0 main();
    }
}