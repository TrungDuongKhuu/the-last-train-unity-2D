Shader "Custom/IrisFade"
{
    Properties{
        _FadeColor ("Fade Color", Color) = (0,0,0,1)
        _Center    ("Center (UV 0-1)", Vector) = (0.5,0.5,0,0)
        _Radius    ("Radius", Range(0,1)) = 1
        _Feather   ("Edge Feather", Range(0,0.5)) = 0.08
    }
    SubShader{
        Tags{ "Queue"="Overlay" "RenderType"="Transparent" "IgnoreProjector"="True" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            fixed4 _FadeColor;
            float2 _Center;
            float  _Radius;
            float  _Feather;

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f      { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };

            v2f vert(appdata v){ v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }

            fixed4 frag(v2f i):SV_Target
            {
                // ---- aspect fix: luôn tròn trên mọi tỷ lệ màn hình
                // _ScreenParams.xy = (width, height)
                float aspect = _ScreenParams.x / _ScreenParams.y;   // 16/9 = 1.777...
                float2 dUV    = i.uv - _Center;
                dUV.x *= aspect;                                    // bù theo chiều ngang

                float d = length(dUV);                              // khoảng cách đã sửa méo

                // mép mềm (đẹp hơn)
                float a = smoothstep(_Radius - _Feather, _Radius + _Feather, d);
                a = pow(a, 1.8);                                   // gamma nhẹ cho mép mượt

                fixed4 col = _FadeColor;
                col.a *= saturate(a);
                return col;
            }
            ENDCG
        }
    }
}
