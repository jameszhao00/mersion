struct VS_IN
{
	float3 pos : POSITION;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
};

struct PS_OUT 
{
	float4 color : SV_TARGET;
};
struct DirectionalLight 
{
	float4 viewspaceIncidentDirection;
	float4 value;
};
cbuffer lights
{
	DirectionalLight directionalLights[16];
	int numLights;
};
Texture2D<float4> diffuse;
Texture2D<float4> normal;
Texture2D<float> depth;
Texture2D<float> shadowMap;
//Texture2D<float4> ref_pos;
float4x4 invProj;
float4x4 invViewProj;

float4x4 lightWorldView;
float4x4 lightWorldViewProj;

int ssaaMultiplier;

SamplerComparisonState ShadowSampler
{
   // sampler state
   Filter = COMPARISON_MIN_MAG_LINEAR_MIP_POINT;
   AddressU = MIRROR;
   AddressV = MIRROR;
   // sampler comparison state
   ComparisonFunc = LESS;
};

SamplerState textureSampler
{
    Filter = ANISOTROPIC;
    AddressU = Wrap;
    AddressV = Wrap;
};
PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;	
	output.pos = float4(input.pos, 1);
	return output;
}
float3 light(DirectionalLight light, float3 n)
{
	return dot(-light.viewspaceIncidentDirection.xyz, n) * light.value.xyz;
}
float3 lightAll(float3 n)
{
	float3 sum = 0;
	for(int i = 0; i < numLights; i++)
	{
		sum += light(directionalLights[i], n); 
	}
	return sum;
}
float4 shade(int2 texXY)
{	
	float4 n = normal.Load(int3(texXY.x, texXY.y, 0));
	float4 a = diffuse.Load(int3(texXY.x, texXY.y, 0));	
	return a * float4(lightAll(n.xyz), 1);
}
float sum(float4 v)
{
	return v.x + v.y + v.z + v.w;
}
float4 shadowed(int2 texXY)
{
	int normalW, normalH;
	normal.GetDimensions(normalW, normalH);

	float d = depth.Load(int3(texXY, 0));
	float4 ndc = float4(((texXY / float2(normalW, normalH)) - 0.5) * int2(1,-1) * 2, d, 1);
	float4 worldPos = mul(ndc, invViewProj);
	worldPos /= worldPos.w;

	
	float4 lightNdc = mul(worldPos, lightWorldViewProj);

	//no perspective divide b/c it's a directional light
	float2 lightUv = lightNdc.xy * float2(0.5, -0.5) + 0.5;
	
	float4 shadowDepth = shadowMap.GatherRed(textureSampler, lightUv, int2(0,0));
	const float epsilon = 0.001;
	return sum(shadowDepth < (lightNdc.z - epsilon)) / 4;
}
PS_OUT PS( PS_IN input )
{	
	PS_OUT output = (PS_OUT)0;
	float4 n = normal.Load(int3(input.pos.x, input.pos.y, 0));
	float4 a = diffuse.Load(int3(input.pos.x, input.pos.y, 0));
	//float4 ref_pos_value = ref_pos.Load(int3(input.pos.x, input.pos.y, 0));
	int2 screenXY = int2(input.pos.x, input.pos.y);
	
	float4 sum = 0;
	for(int i = 0; i < ssaaMultiplier; i++)
	{
		for(int j = 0; j < ssaaMultiplier; j++)
		{
			sum += shade(screenXY * ssaaMultiplier + int2(i,j)) * 
				clamp(1 - shadowed(screenXY * ssaaMultiplier + int2(i,j)), 0.2, 1);
		}
	}
	output.color = sum / ((ssaaMultiplier) * (ssaaMultiplier));	
	return output;
}

technique 
{
	pass 
	{
		Profile = 11.0;
		PixelShader = PS;
		VertexShader = VS;
	}
}