Shader "Unlit/Heatmap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 gles
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 colors[5];
            float pointRanges[5];

            float _positions[3 * 1000];
            int _positionCount = 0;

            void init()
            {
            }

            float distsq(float a,float b)
            {
                float d =pow(max(0.0, 1.0 - distance(a,b)),2);
                                
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                float uv = i.uv;

                float totalWeigth = 0;
                for(int i = 0; i < _positionCount; i++)
                {
                    float2 pt = float2(_positions[i*3], _positions[3*i+1]);
                    float pt_intensity = _positions[i*3 +2];

                    totalWeigth += 0.5 * distsq(uv,pt) * pt_intensity;
                }
                
                return col;
            }
            ENDCG
        }
    }
}
