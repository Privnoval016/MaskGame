Shader "Custom/CyberspaceScroller"
{
    Properties
    {
        _LineColor ("Line Color", Color) = (0, 1, 1, 1)
        _ScrollSpeed ("Scroll Speed", Float) = 5.0
        _ScrollDirection ("Scroll Direction", Vector) = (1, 0, 0, 0)
        _Tiling ("Tiling (Density)", Float) = 10.0
        _LineThickness ("Line Thickness", Range(0.001, 0.1)) = 0.01
        _LineLength ("Line Length", Range(0.1, 2.0)) = 0.5
        _Brightness ("Brightness (for Bloom)", Range(1, 10)) = 3.0
        _FadeStart ("Fade Start", Range(0, 1)) = 0.3
        [Toggle] _Animate ("Animate in Editor", Float) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

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

            float4 _LineColor;
            float _ScrollSpeed;
            float4 _ScrollDirection;
            float _Tiling;
            float _LineThickness;
            float _LineLength;
            float _Brightness;
            float _FadeStart;
            float _Animate;

            // Hash function for pseudo-random values
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Normalize scroll direction
                float2 scrollDir = normalize(_ScrollDirection.xy + float2(0.0001, 0.0001));
                
                // Apply tiling
                float2 tiledUV = i.uv * _Tiling;
                
                // Calculate scrolling offset - only animate if toggle is on OR if in play mode
                float timeValue = _Animate > 0.5 ? _Time.y : 0.0;
                float scrollOffset = timeValue * _ScrollSpeed;
                
                // Scroll the UVs in the specified direction
                float2 scrolledUV = tiledUV - scrollDir * scrollOffset;
                
                // Get the cell ID for each line
                float2 cellID = floor(scrolledUV);
                
                // Get local coordinates within the cell
                float2 localUV = frac(scrolledUV);
                
                // Create pseudo-random offset for each line
                float randomOffset = hash(cellID);
                
                // Calculate position along the scroll direction
                float alongScroll = dot(scrolledUV, scrollDir);
                
                // Calculate position perpendicular to scroll direction
                float2 perpDir = float2(-scrollDir.y, scrollDir.x);
                float perpPos = dot(scrolledUV, perpDir);
                
                // Create offset perpendicular lines using cell ID
                float lineOffset = hash(cellID) * 100.0;
                float perpPosWithOffset = perpPos + lineOffset;
                
                // Get the line position within each row
                float lineRow = floor(perpPosWithOffset);
                float lineLocalPos = frac(perpPosWithOffset);
                
                // Random values for this specific line
                float lineRandom = hash(float2(lineRow, floor(alongScroll * 0.5)));
                float linePhase = hash(float2(lineRow * 2.5, 0));
                
                // Position along the line direction with phase offset
                float lineProgress = frac((alongScroll + linePhase * 10.0) * 0.5);
                
                // Create broken line segments (only show line in certain ranges)
                float segmentMask = step(lineProgress, _LineLength);
                
                // Line thickness (distance from center of line)
                float distFromLine = abs(lineLocalPos - 0.5) * 2.0;
                float lineValue = 1.0 - smoothstep(0.0, _LineThickness, distFromLine);
                
                // Combine line with segment mask
                float finalLine = lineValue * segmentMask;
                
                // Add fade along the line for streaking effect
                float fade = 1.0 - smoothstep(_FadeStart, 1.0, lineProgress / _LineLength);
                finalLine *= fade;
                
                // Vary brightness per line for more interest
                float brightnessMod = 0.7 + randomOffset * 0.3;
                
                // Apply color and brightness
                float3 finalColor = _LineColor.rgb * finalLine * _Brightness * brightnessMod;
                
                return fixed4(finalColor, 1.0);
            }
            ENDCG
        }
    }
    
    FallBack "Diffuse"
}