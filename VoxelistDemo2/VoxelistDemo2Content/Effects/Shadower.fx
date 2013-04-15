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
	MipFilter = Linear;

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
	float4 Position : POSITION;
	float4 Color : COLOR0;
	float4 Position2 : TEXCOORD0;
	float2 TextureCoordinate : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.Position2 = output.Position;

	float4 normal = mul(input.Normal, WorldInverseTranspose);
	float lightIntensity = -dot(normal, DiffuseLightDirection);
	float4 diffuseContribution = DiffuseColor * DiffuseIntensity * lightIntensity;
	
	float4 ambientContribution = AmbientColor * AmbientIntensity;

	output.Color = saturate(diffuseContribution + ambientContribution);
	output.Color.a = 1;

	output.TextureCoordinate = input.TextureCoordinate;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);

	return saturate(textureColor * input.Color);
}

technique DrawScene
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

//for shadowing purposes ...
float4x4 xLightsWorldViewProjection;

struct SMapVertexToPixel
{
    float4 Position     : POSITION;
    float4 Position2D    : TEXCOORD0;
};

struct SMapPixelToFrame
{
    float4 Color : COLOR0;
};

SMapVertexToPixel ShadowMapVertexShader( float4 inPos : POSITION)
{
    SMapVertexToPixel Output;

    Output.Position = mul(inPos, xLightsWorldViewProjection);
    Output.Position2D = Output.Position;

    return Output;
}

SMapPixelToFrame ShadowMapPixelShader(SMapVertexToPixel PSIn)
{
    SMapPixelToFrame Output;

    Output.Color = PSIn.Position2D.z/PSIn.Position2D.w;

    return Output;
}

technique ShadowMap
{
	pass Pass0
	{
        VertexShader = compile vs_2_0 ShadowMapVertexShader();
        PixelShader = compile ps_2_0 ShadowMapPixelShader();
	}
}