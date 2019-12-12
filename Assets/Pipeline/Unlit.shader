Shader "My Pipeline/Unlit" 
{
	Properties { }

	SubShader 
    {
		Pass 
        {
            HLSLPROGRAM

            #pragma vertex UnlitPassVertex
            #pragma vertex UnlitPassFragment
            
            #include "Unlit.hlsl"
            
            ENDHLSL
		}
	}
}