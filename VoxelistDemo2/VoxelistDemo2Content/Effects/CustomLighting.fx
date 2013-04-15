float4x4 WVP;

//for ambient lighting
float4 AmbientColor = float4(.7, .7, 1, 1);
float AmbientIntensity = .5f;

//for diffuse lighting
float4x4 WorldInverseTranspose;

float4 DiffuseColor = float4(1, 1, .7, 1);
float3 DiffuseLightDirection = float3(1, -1, 1);
float DiffuseIntensity = .7f;

//for texturing
texture EffectTexture;
sampler2D textureSampler = sampler_state
{
	Texture = (EffectTexture);

	MagFilter = Linear;
	MinFilter = Linear;
	MipFilter = Linear;

	AddressU = Mirror;
	AddressV = Mirror;
};

//for torch lighting
float4 TorchColor = float4(249 / 255.0, 92 / 255.0, 7 / 255.0, 1);

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION;
	float4 Color : COLOR0;
	float4 Position2 : TEXCOORD0;
	float2 TextureCoordinate : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.Position = mul(input.Position, WVP);
	output.Position2 = output.Position;

	float4 normal = mul(input.Normal, WorldInverseTranspose);
	float lightIntensity = -dot(normal, DiffuseLightDirection);
	float4 diffuseContribution = DiffuseColor * DiffuseIntensity * lightIntensity;
	
	float4 ambientContribution = AmbientColor * AmbientIntensity;

	output.Color = diffuseContribution + ambientContribution;
	output.Color.a = 1;

	output.TextureCoordinate = input.TextureCoordinate;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);

	float distScale = 50 / pow(dot(input.Position2, input.Position2), 1);
	float4 torchContribution = .5 * saturate(TorchColor * distScale * float4(1, 1, 1, 0));

	input.Color += torchContribution;

	return saturate(textureColor * input.Color);
}

technique Lighting
{
	pass Pass1
	{
		AlphaBlendEnable = TRUE;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;

		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}