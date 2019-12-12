#ifndef MYRP_UNLIT_INCLUDED
#define MYRP_UNLIT_INCLUDED

struct VertexInput 
{
    float4 pos : POSITION;
};

struct VertexOutput
{
    float4 clipPos : SV_POSITION;
};

float4x4 unity_MatrixVP;
float4x4 unity_ObjectToWorld;

VertexOutput UnlitPassVertex(VertexInput input)
{
    VertexOutput output;
    float4 worldpos = mul(unity_ObjectToWorld, input.pos);
    output.clipPos = mul(unity_MatrixVP, worldpos);
    return output;
};

float4 UnlitPassFragment(VertexOutput input) : SV_Target
{
    return 1;
}

#endif // MYRP_UNLIT_INCLUDED