struct VS_IN
{
	float3 pos : SV_Position;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float3 normal : NORMAL;
	float2 uv : UV;
	float4 viewPos : WORLD_POS;
};

struct PS_OUT 
{
	float4 diffuse : SV_TARGET0;
	float4 normal : SV_TARGET1;
	float4 ref_pos : SV_TARGET2;
};

float4x4 worldViewProj;
float4x4 worldView;
SamplerState textureSampler
{
    Filter = ANISOTROPIC;
    AddressU = Wrap;
    AddressV = Wrap;
};
Texture2D<float3> albedo;
PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;	
	output.pos = mul(float4(input.pos, 1), worldViewProj);
	//normalize for scale
	output.normal = float4(normalize(mul(float4(input.normal.xyz, 0), worldView).xyz), 0);
	output.uv = input.uv;
	output.viewPos = mul(float4(input.pos, 1), worldView);
	return output;
}

PS_OUT PS( PS_IN input ) 
{
	PS_OUT output = (PS_OUT)0;
	output.diffuse = float4(albedo.Sample(textureSampler, input.uv), 1);	
	output.normal = float4(input.normal, 1);
	output.ref_pos = input.viewPos;
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