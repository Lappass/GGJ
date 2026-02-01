Shader "Custom/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,0,1)
        _OutlineWidth ("Outline Width", Range(0, 10)) = 1
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // If alpha is high enough, just draw the sprite
                if (c.a > 0.1) return c;

                // Check neighbors for outline
                float2 texel = _MainTex_TexelSize.xy;
                float w = _OutlineWidth;

                // Sample 4 directions
                fixed4 up = tex2D(_MainTex, IN.texcoord + fixed2(0, w * texel.y));
                fixed4 down = tex2D(_MainTex, IN.texcoord - fixed2(0, w * texel.y));
                fixed4 left = tex2D(_MainTex, IN.texcoord - fixed2(w * texel.x, 0));
                fixed4 right = tex2D(_MainTex, IN.texcoord + fixed2(w * texel.x, 0));

                // If any neighbor has alpha, draw outline
                if (up.a + down.a + left.a + right.a > 0)
                {
                    return _OutlineColor * IN.color.a;
                }

                return c;
            }
        ENDCG
        }
    }
}
