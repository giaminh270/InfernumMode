texture sampleTexture;
sampler2D NoiseMap = sampler_state
{
    texture = <sampleTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

texture sampleTexture2;
sampler2D NoiseMap2 = sampler_state
{
    texture = <sampleTexture2>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

float3 mainColor;
float3 secondaryColor;
float2 resolution;
float time;
float opacity;
float innerGlowAmount;
float innerGlowDistance;

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    // Pixelate. Rewritten from "uv -= uv % (1 / (resolution * 2))" (fmod, which expands to a
    // div + floor + mul + sub chain PER AXIS on ps_2_0) into an equivalent, cheaper,
    // vectorized floor-snap that does both axes in one shot.
    float2 pixelScale = resolution * 2.0;
    uv = floor(uv * pixelScale) / pixelScale;

    float distanceFromTargetPosition = distance(uv, float2(0.5, 0.5));
    float actualOpacity = opacity;
    if (distanceFromTargetPosition < innerGlowDistance)
        actualOpacity /= pow(abs(distanceFromTargetPosition / innerGlowDistance), innerGlowAmount);

    // Calculate the swirl coordinates.
    // Rewritten from two separate sin() calls (one of them faked as cos() via a "+1.57" phase
    // shift) into a single sincos() call. ps_2_0 has no native sin/cos, so each sin() call
    // macro-expands into a multi-instruction polynomial approximation; computing sin and cos
    // separately effectively pays that cost twice. sincos() computes both from shared work in
    // one go, and is exact instead of the old +1.57 (~pi/2) approximation for cosine.
    float2 centeredCoords = uv - 0.5;
    float swirlRotation = length(centeredCoords) * 19.0 - time * 5.0;
    float swirlSine, swirlCosine;
    sincos(swirlRotation, swirlSine, swirlCosine);

    // Manually inlined equivalent of mul(centeredCoords, float2x2(cos, -sin, sin, cos)) + 0.5,
    // avoids constructing a matrix object just to immediately multiply it away.
    float2 swirlCoordinates;
    swirlCoordinates.x = centeredCoords.x * swirlCosine + centeredCoords.y * swirlSine + 0.5;
    swirlCoordinates.y = centeredCoords.y * swirlCosine - centeredCoords.x * swirlSine + 0.5;

    // Calculate fade, swirl arm colors, and draw the portal to the screen.
    float swirlColorFade = clamp(distanceFromTargetPosition * 3.0, 0.0, 1.0) / 0.95;
    float3 swirlBaseColor = lerp(mainColor, secondaryColor, pow(abs(swirlColorFade), 0.63));

    // The original was lerp(tex2D(NoiseMap,...), tex2D(NoiseMap2,...), 0), which always
    // evaluates to just tex2D(NoiseMap,...) since the blend factor is a literal 0 — NoiseMap2
    // never actually contributed anything. Removed the dead lerp/second sample entirely;
    // the output is identical to before, just cheaper.
    float4 swirlNoiseColor = lerp(float4(mainColor, 1), tex2D(NoiseMap, swirlCoordinates) * (1.0 - swirlColorFade), 0.5);

    float4 endColor = lerp(float4(swirlBaseColor, 0.1), float4(0.0, 0.0, 0.0, 0.0), swirlColorFade);
    return lerp(float4(0.0, 0.0, 0.0, 0.0), endColor * (1.0 + (1.0 - swirlColorFade) * 2.0), clamp(swirlNoiseColor.r, 0.0, 1.0)) * actualOpacity;
}

technique Technique1
{
    pass PortalPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
