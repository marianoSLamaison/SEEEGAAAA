// Variables globales
float4x4 World;
float4x4 View;
float4x4 Projection;

// Texturas
Texture2D BoxTexture;


// Sampler para las texturas
SamplerState SamplerType
{
    Filter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

// Entrada del vértice
struct VertexShaderInput
{
    float3 Position : POSITION;   // La posición inicial del vértice
    float3 Normal : NORMAL;       // La normal del vértice
    float2 TexCoord : TEXCOORD0;  // Coordenadas de textura
};

// Salida del vértice
struct VertexShaderOutput
{
    float4 Position : SV_POSITION;    // Posición final que necesita el rasterizador
    float3 WorldPosition : TEXCOORD1; // Posición en espacio del mundo
    float3 WorldNormal : TEXCOORD2;   // Normal transformada en espacio del mundo
    float2 TexCoord : TEXCOORD0;      // Coordenadas de textura
};

// Vertex Shader
VertexShaderOutput VS(VertexShaderInput input)
{
    VertexShaderOutput output;

    // Transformar la posición del vértice a espacio del mundo y luego a espacio de clip
    float4 worldPosition = mul(float4(input.Position, 1.0), World);
    output.Position = mul(mul(worldPosition, View), Projection);

    // Pasar otros valores a la salida
    output.WorldPosition = worldPosition.xyz;
    output.WorldNormal = mul(input.Normal, (float3x3)World); // Solo una rotación para normales
    output.TexCoord = input.TexCoord;

    return output;
}

float4 PS(VertexShaderOutput input) : SV_TARGET
{
    // Obtener el color base de la textura
    float4 textureBox = BoxTexture.Sample(SamplerType, input.TexCoord);
    
    // Combinar difusa y especular
    float ambientLight = 10000.0; // Ajusta este valor
    float3 lighting = ambientLight + textureBox.rgb;// + (baseColor.rgb * ao);



    // Devuelve el color final
    //return float4(lighting, baseColor.a);
    return float4(textureBox.rgb, textureBox.a);
}


// Técnica
technique VehicleTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}
