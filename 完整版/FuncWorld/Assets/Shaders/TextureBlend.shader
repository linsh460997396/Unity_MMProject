Shader "Custom/TextureBlend"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _OverlayTex ("Overlay Texture", 2D) = "black" {}
        _BlendFactor ("Blend Factor", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _OverlayTex;
            float _BlendFactor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the main texture
                fixed4 mainColor = tex2D(_MainTex, i.uv);
                
                // Sample the overlay texture
                fixed4 overlayColor = tex2D(_OverlayTex, i.uv);
                
                // Blend the two textures based on the blend factor
                fixed4 finalColor = lerp(mainColor, overlayColor, _BlendFactor);
                
                return finalColor;
            }
            ENDCG
        }
    }
}

// ‌Properties‌: 定义了三个属性:

// _MainTex 是我们的主纹理.
// _OverlayTex 是用于覆盖或修改主纹理的新纹理.
// _BlendFactor 是一个混合因子,用于控制新纹理对主纹理的影响程度.
// ‌SubShader‌: 定义了具体的渲染逻辑.

// ‌Pass‌ 部分包含了顶点着色器 (vert) 和片段着色器 (frag).
// ‌顶点着色器 (vert)‌:

// 负责将顶点从对象空间转换到裁剪空间,并传递 UV 坐标.
// ‌片段着色器 (frag)‌:

// 使用 tex2D 函数对主纹理和覆盖纹理进行采样.
// 使用 lerp 函数根据 _BlendFactor 混合两种纹理的颜色.
// 返回最终的颜色作为输出.
// ‌如何使用‌:

// 在 Unity 中创建一个新的 Shader,并将上述代码粘贴到 Shader 文件中.
// 创建一个材质,并将该材质的 Shader 设置为刚刚创建的 Shader.
// 将主纹理和覆盖纹理分别设置到材质的相应属性上.
// 调整混合因子以控制覆盖纹理对主纹理的影响程度.
// 这样,你就可以根据新纹理的像素信息去渲染主纹理的不同区域,并输出混合后的结果.