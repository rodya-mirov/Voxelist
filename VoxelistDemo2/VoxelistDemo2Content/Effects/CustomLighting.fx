float4x4 World;
float4x4 View;
float4x4 Projection;

//for ambient lighting
float4 AmbientColor = float4(1, 1, 1, 1);
float AmbientIntensity = .5f;

//for diffuse lighting
float4x4 WorldInverseTranspose;

float4 DiffuseColor = float4(1, 1, 1, 1);
float3 DiffuseLightDirection = float3(1, -1, 1);
float DiffuseIntensity = .7f;

//for texturing
texture EffectTexture;
sampler2D textureSampler = sampler_state
{
	Texture = (EffectTexture);
	MagFilter = Linear;
	MinFilter = Linear;
	AddressU = Mirror;
	AddressV = Mirror;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 TextureCoordinate : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	float4 normal = mul(input.Normal, WorldInverseTranspose);
	float lightIntensity = -dot(normal, DiffuseLightDirection);
	output.Color = saturate(DiffuseColor * DiffuseIntensity * lightIntensity) + saturate(AmbientColor * AmbientIntensity);

	output.TextureCoordinate = input.TextureCoordinate;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);
	textureColor.a = 1;

	return saturate(textureColor * input.Color);
}

technique Lighting
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}