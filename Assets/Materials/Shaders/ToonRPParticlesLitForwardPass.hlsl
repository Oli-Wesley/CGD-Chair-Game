#ifndef TOON_RP_PARTICLES_LIT_FORWARD_PASS
#define TOON_RP_PARTICLES_LIT_FORWARD_PASS

// Same as Toon RP's ToonRPDefaultForwardPass.hlsl (lit path), plus particle vertex colour.
// Particle Start Color / Color over Lifetime etc. arrive as vertex colour (COLOR semantic).

#include "Packages/com.deltation.toon-rp/ShaderLibrary/Common.hlsl"
#include "Packages/com.deltation.toon-rp/ShaderLibrary/Fog.hlsl"
#include "Packages/com.deltation.toon-rp/ShaderLibrary/ToonLighting.hlsl"

#include "Packages/com.deltation.toon-rp/Shaders/ToonRPDefaultInput.hlsl"

// Use our own v2f (with colour) instead of ToonRPDefaultV2f.hlsl.
// Defining its include guard stops ToonRPDefaultLitOutput.hlsl from pulling in the default one.
#define TOON_RP_DEFAULT_V2F

struct v2f
{
    float2 uv : TEXCOORD0;
    half3 normalWs : NORMAL_WS;
    float3 positionWs : POSITION_WS;
    float4 color : COLOR;

    #ifdef REQUIRE_TANGENT_INTERPOLANT
    half3 tangentWs : TANGENT_WS;
    half3 bitangentWs : BITANGENT_WS;
    #endif // REQUIRE_TANGENT_INTERPOLANT

    #ifdef _TOON_RP_ADDITIONAL_LIGHTS_VERTEX
    float3 additionalLights : ADDITIONAL_LIGHTS;
    #endif // _TOON_RP_ADDITIONAL_LIGHTS_VERTEX

    TOON_RP_GI_INTERPOLANT
    TOON_RP_FOG_FACTOR_INTERPOLANT

    float4 positionCs : SV_POSITION;

    UNITY_VERTEX_OUTPUT_STEREO
};

#ifdef _TOON_RP_ADDITIONAL_LIGHTS_VERTEX
#define PER_VERTEX_ADDITIONAL_LIGHTS(IN) (IN.additionalLights)
#else // !_TOON_RP_ADDITIONAL_LIGHTS_VERTEX
#define PER_VERTEX_ADDITIONAL_LIGHTS(IN) (float3(0, 0, 0))
#endif // _TOON_RP_ADDITIONAL_LIGHTS_VERTEX

#include "Packages/com.deltation.toon-rp/Shaders/ToonRPDefaultLitOutput.hlsl"

struct appdata
{
    float3 vertex : POSITION;
    half3 normal : NORMAL;
    float2 uv : TEXCOORD0;
    float4 color : COLOR;

    #ifdef REQUIRE_TANGENT_INTERPOLANT
    half4 tangent : TANGENT;
    #endif // REQUIRE_TANGENT_INTERPOLANT

    TOON_RP_GI_ATTRIBUTE
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2f VS(const appdata IN)
{
    v2f OUT;

    UNITY_SETUP_INSTANCE_ID(IN);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

    OUT.uv = APPLY_TILING_OFFSET(IN.uv, _MainTexture);
    OUT.color = IN.color;

    const float3 positionWs = TransformObjectToWorld(IN.vertex);
    const half3 normalWs = TransformObjectToWorldNormal(IN.normal);
    OUT.normalWs = normalWs;
    OUT.positionWs = positionWs;

    const float4 positionCs = TransformWorldToHClip(positionWs);
    OUT.positionCs = positionCs;

    #ifdef REQUIRE_TANGENT_INTERPOLANT
    ComputeTangentsWs(IN.tangent, normalWs, OUT.tangentWs, OUT.bitangentWs);
    #endif // REQUIRE_TANGENT_INTERPOLANT

    #ifdef _TOON_RP_ADDITIONAL_LIGHTS_VERTEX
    LightComputationParameters lightComputationParameters = (LightComputationParameters) 0;
    lightComputationParameters.positionWs = positionWs;
    lightComputationParameters.positionCs = positionCs;
    lightComputationParameters.normalWs = normalWs;

    float3 additionalLightsSpecularUnused;
    ComputeAdditionalLightsDiffuseSpecular(lightComputationParameters, 1, OUT.additionalLights, additionalLightsSpecularUnused);
    #endif // _TOON_RP_ADDITIONAL_LIGHTS_VERTEX

    TOON_RP_GI_TRANSFER(IN, OUT);
    TOON_RP_FOG_FACTOR_TRANSFER(OUT, positionCs);

    return OUT;
}

float4 PS(const v2f IN) : SV_TARGET
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

    // Material colour * texture * PARTICLE colour
    float4 albedo = SampleAlbedo(IN.uv) * IN.color;
    AlphaClip(albedo);

    #ifdef _ALPHAPREMULTIPLY_ON
    albedo.rgb *= albedo.a;
    #endif // _ALPHAPREMULTIPLY_ON

    float3 outputColor = ComputeLitOutputColor(IN, albedo);

    TOON_RP_FOG_MIX(IN, outputColor);

    return float4(outputColor, albedo.a);
}

#endif // TOON_RP_PARTICLES_LIT_FORWARD_PASS
