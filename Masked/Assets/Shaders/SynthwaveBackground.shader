Shader "Custom/SynthwaveBackground"
{
    Properties
    {
        // ─── Color ──────────────────────────────────────────────────────
        _Color ("Primary Color", Color) = (0.0, 0.85, 1.0, 1.0)
        _ColorB ("Secondary Color", Color) = (0.9, 0.2, 0.7, 1.0)
        _ColorMix ("Color Mix (0=Primary, 1=Secondary)", Range(0, 1)) = 0.3

        // ─── Seed & Speed ───────────────────────────────────────────────
        _Seed ("Generation Seed", Float) = 0.0
        _Speed ("Animation Speed", Float) = 1.0

        // ─── Perspective Grid ───────────────────────────────────────────
        _GridEnabled ("Grid Enabled", Range(0, 1)) = 1.0
        _GridDensity ("Grid Density", Float) = 8.0
        _GridBrightness ("Grid Brightness", Range(0, 3)) = 0.35
        _GridHorizon ("Horizon Position (0=bottom, 1=top)", Range(0, 1)) = 0.45
        _GridWarp ("Grid Lateral Warp Strength", Range(0, 0.15)) = 0.04

        // ─── Floating Shapes ────────────────────────────────────────────
        _ShapeCount ("Shape Count", Range(1, 8)) = 5.0
        _ShapeBrightness ("Shape Brightness", Range(0, 3)) = 0.7
        _ShapeSpeed ("Shape Drift Speed", Float) = 0.15
        _ShapeSize ("Shape Size", Range(0.01, 0.15)) = 0.06

        // ─── Scan Lines ─────────────────────────────────────────────────
        _ScanEnabled ("Scan Lines Enabled", Range(0, 1)) = 1.0
        _ScanSpeed ("Scan Line Speed", Float) = 0.4
        _ScanBrightness ("Scan Line Brightness", Range(0, 1)) = 0.12
        _ScanFrequency ("Scan Line Frequency", Float) = 3.0

        // ─── Glow ───────────────────────────────────────────────────────
        _HorizonGlow ("Horizon Glow Intensity", Range(0, 2)) = 0.3
        _HorizonGlowSpread ("Horizon Glow Spread", Range(0.01, 0.5)) = 0.08
    }

    SubShader
    {
        // Queue=Background renders before Geometry (1000). ZWrite On still writes
        // depth, but Background queue = 100, so it draws first. We then force
        // the fragment shader to output depth = 1.0 (farthest), so every opaque
        // or transparent object in front will pass the depth test.
        Tags { "RenderType"="Opaque" "Queue"="Background" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Cull Off
            ZWrite On
            ZTest Always  // always write; we're the background

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // ── Properties ──────────────────────────────────────────────
            float4 _Color;
            float4 _ColorB;
            float _ColorMix;
            float _Seed;
            float _Speed;

            float _GridEnabled;
            float _GridDensity;
            float _GridBrightness;
            float _GridHorizon;
            float _GridWarp;

            float _ShapeCount;
            float _ShapeBrightness;
            float _ShapeSpeed;
            float _ShapeSize;

            float _ScanEnabled;
            float _ScanSpeed;
            float _ScanBrightness;
            float _ScanFrequency;

            float _HorizonGlow;
            float _HorizonGlowSpread;

            // ── Structs ─────────────────────────────────────────────────
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // ── Utility ─────────────────────────────────────────────────
            float hash1(float2 p)
            {
                p = frac(p * float2(443.897, 829.065));
                p += dot(p.yx, p.xy + 33.33);
                return frac(p.x * p.y);
            }

            float2 hash2(float2 p)
            {
                p = frac(p * float2(443.897, 829.065));
                p += dot(p.yx, p.xy + 33.33);
                return frac(float2(p.x * p.y, p.y * p.x + 1.0));
            }

            // Smooth noise: bilinear interpolation of hashed grid corners
            float smoothNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f); // smoothstep

                float a = hash1(i);
                float b = hash1(i + float2(1.0, 0.0));
                float c = hash1(i + float2(0.0, 1.0));
                float d = hash1(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float hexRing(float2 p, float2 c, float r, float thickness)
            {
                float2 d = p - c;
                float2 a1 = float2(1.0, 0.0);
                float2 a2 = float2(0.5, 0.866025);
                float2 a3 = float2(-0.5, 0.866025);
                float hexDist = max(max(abs(dot(d, a1)), abs(dot(d, a2))), abs(dot(d, a3)));
                float ring = 1.0 - smoothstep(0.0, thickness, abs(hexDist - r));
                return ring;
            }

            float triangleRing(float2 p, float2 c, float r, float thickness, float rotation)
            {
                float2 d = p - c;
                float cs = cos(rotation);
                float sn = sin(rotation);
                d = float2(d.x * cs - d.y * sn, d.x * sn + d.y * cs);

                float2 v0 = float2(0.0, r);
                float2 v1 = float2(-r * 0.866025, -r * 0.5);
                float2 v2 = float2(r * 0.866025, -r * 0.5);

                float2 e0 = v1 - v0;
                float t0 = saturate(dot(d - v0, e0) / dot(e0, e0));
                float d0 = length(d - (v0 + e0 * t0));

                float2 e1 = v2 - v1;
                float t1 = saturate(dot(d - v1, e1) / dot(e1, e1));
                float d1 = length(d - (v1 + e1 * t1));

                float2 e2 = v0 - v2;
                float t2 = saturate(dot(d - v2, e2) / dot(e2, e2));
                float d2 = length(d - (v2 + e2 * t2));

                float minDist = min(min(d0, d1), d2);
                return 1.0 - smoothstep(0.0, thickness, minDist);
            }

            float diamondRing(float2 p, float2 c, float r, float thickness, float rotation)
            {
                float2 d = p - c;
                float cs = cos(rotation);
                float sn = sin(rotation);
                d = float2(d.x * cs - d.y * sn, d.x * sn + d.y * cs);
                float diamDist = abs(d.x) + abs(d.y);
                return 1.0 - smoothstep(0.0, thickness, abs(diamDist - r));
            }

            // ── Vertex ──────────────────────────────────────────────────
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            // ── Fragment ────────────────────────────────────────────────
            // We output both color AND depth. Depth is forced to 1.0 so this
            // fragment is always "behind" everything else.
            struct FragOutput
            {
                half4 col : SV_Target;
                float dep : SV_Depth;
            };

            FragOutput frag(Varyings input)
            {
                float2 uv = input.uv;
                float time = _Time.y * _Speed;

                half3 col = (half3)lerp(_Color.rgb, _ColorB.rgb, _ColorMix);

                half3 finalColor = half3(0.0, 0.0, 0.0);

                // ── Perspective grid ────────────────────────────────────
                if (_GridEnabled > 0.5)
                {
                    float horizon = _GridHorizon;
                    float belowHorizon = step(uv.y, horizon);

                    // t=0 at horizon, t=1 at bottom
                    float t = 1.0 - (uv.y / max(horizon, 0.001));
                    t = saturate(t);
                    float tPersp = pow(t, 2.5);

                    // ── Lateral warp ────────────────────────────────────
                    // Each horizontal line gets a unique lateral displacement
                    // driven by smooth noise keyed to its scroll position.
                    // This breaks the rigid grid into something organic.
                    float scrollPos = tPersp * _GridDensity - time * 0.5;
                    float warpOffset = (smoothNoise(float2(scrollPos * 0.7 + _Seed, 0.0)) - 0.5) * 2.0 * _GridWarp;

                    // Warped horizontal coordinate for vertical line placement
                    float warpedX = uv.x + warpOffset;

                    // Horizontal lines: evenly spaced in perspective t
                    float hLines = frac(scrollPos);
                    float hLine = 1.0 - smoothstep(0.0, 0.025 * (1.0 - tPersp + 0.1), hLines);

                    // Vertical lines: converge toward center, but with per-line warp
                    float spread = lerp(0.0, 1.0, tPersp);
                    float vCoord = (warpedX - 0.5) / max(spread, 0.001) + 0.5;
                    float vLines = frac(vCoord * _GridDensity);
                    float vLine = 1.0 - smoothstep(0.0, 0.015 * (1.0 - tPersp + 0.1), vLines);

                    float grid = max(hLine, vLine) * belowHorizon;
                    grid *= smoothstep(0.0, 0.15, t);

                    finalColor += col * grid * _GridBrightness;
                }

                // ── Horizon glow ────────────────────────────────────────
                {
                    float distToHorizon = abs(uv.y - _GridHorizon);
                    // Animate the glow intensity with a slow breathe
                    float breathe = smoothNoise(float2(time * 0.08 + _Seed, 0.5)) * 0.3 + 0.85;
                    float glow = exp(-distToHorizon / max(_HorizonGlowSpread, 0.001));
                    finalColor += col * glow * _HorizonGlow * breathe;
                }

                // ── Floating geometric shapes ───────────────────────────
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if ((float)i >= _ShapeCount) break;

                        float2 seed = float2((float)i + _Seed, (float)i * 7.3 + _Seed * 3.1);
                        float2 rnd = hash2(seed);
                        float rnd3 = hash1(seed + 1.0);

                        // Each shape has an independent lifecycle: it fades in,
                        // grows, holds, then shrinks and fades out before restarting.
                        float lifecycleSpeed = 0.08 + rnd.x * 0.06; // unique period per shape
                        float lifecyclePhase = frac(time * lifecycleSpeed + rnd.y); // 0->1 over one cycle

                        // Fade envelope: fade in over first 15%, hold, fade out over last 25%
                        float fadeIn = smoothstep(0.0, 0.15, lifecyclePhase);
                        float fadeOut = 1.0 - smoothstep(0.75, 1.0, lifecyclePhase);
                        float lifeFade = fadeIn * fadeOut;

                        // Size pulses: small at birth, grows to full at 30%, holds, shrinks back
                        float sizePulse = smoothstep(0.0, 0.3, lifecyclePhase) * smoothstep(1.0, 0.85, lifecyclePhase);

                        // Position: each shape spawns at a new random location each cycle.
                        // We derive spawn pos from the integer cycle count so it jumps
                        // to a new spot each time rather than drifting continuously.
                        float cycleCount = floor(time * lifecycleSpeed + rnd.y);
                        float2 spawnPos = hash2(seed + cycleCount * 137.0);

                        // Gentle drift within the cycle (doesn't accumulate across cycles)
                        float driftAngle = hash1(seed + cycleCount * 53.0) * 6.2831853;
                        float driftMag = 0.04 * lifecyclePhase; // drifts outward over life
                        float2 pos = spawnPos + float2(cos(driftAngle), sin(driftAngle)) * driftMag;

                        // Rotation speeds up slightly as shape ages
                        float rot = time * (0.3 + rnd.x * 0.5) + rnd.y * 6.2831853;

                        // Size: base size scaled by lifecycle pulse
                        float size = _ShapeSize * (0.5 + rnd.y * 0.8) * (0.3 + sizePulse * 0.7);

                        float shapeType = floor(rnd3 * 3.0);

                        // Brightness combines the lifecycle fade with a subtle shimmer
                        float shimmer = smoothNoise(float2(time * 2.0 + (float)i * 5.7, rnd.x * 10.0)) * 0.15 + 0.85;
                        float brightness = lifeFade * shimmer * _ShapeBrightness;

                        // Thickness scales with size so small shapes stay crisp
                        float shapeThickness = 0.002 + size * 0.02;

                        half3 shapeCol = (half3)lerp(_Color.rgb, _ColorB.rgb, saturate(_ColorMix + rnd.x * 0.3 - 0.1));

                        float shape = 0.0;
                        if (shapeType < 0.5)
                            shape = hexRing(uv, pos, size, shapeThickness);
                        else if (shapeType < 1.5)
                            shape = triangleRing(uv, pos, size, shapeThickness, rot);
                        else
                            shape = diamondRing(uv, pos, size, shapeThickness, rot);

                        finalColor += shapeCol * shape * brightness;
                    }
                }

                // ── Scan lines ──────────────────────────────────────────
                if (_ScanEnabled > 0.5)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float fi = (float)i;

                        // Each scan line has a unique speed derived from seed+index.
                        // Base speed plus a noise-driven variation gives irregular cadence.
                        float speedVar = hash1(float2(fi + _Seed * 7.1, 42.0));
                        float lineSpeed = _ScanSpeed * (0.7 + speedVar * 0.8);

                        // Staggered phase so they don't all start together
                        float phase = fi / 3.0 + _Seed * 0.1;

                        // Occasional "skip": when noise crosses a threshold mid-travel,
                        // the line jumps forward in its cycle. We do this by adding a
                        // discrete offset based on a slow noise sample.
                        float skipNoise = smoothNoise(float2(time * 0.3 + fi * 3.7, 99.0));
                        float skipOffset = step(0.85, skipNoise) * 0.4; // 15% chance, jumps 40% of cycle

                        float scanY = frac(time * lineSpeed + phase + skipOffset);
                        scanY = 1.0 - scanY; // top to bottom

                        float dist = abs(uv.y - scanY);

                        // Asymmetric trail: sharper leading edge, longer soft tail behind
                        // "Behind" = above the line (higher uv.y), since it moves downward
                        float ahead = step(uv.y, scanY); // 1 if we're above (ahead of) the line
                        float sharpEdge = exp(-dist * 120.0);
                        float softTail = exp(-dist * 18.0) * ahead; // tail only trails behind

                        float scanLine = sharpEdge * 0.5 + softTail * 0.4;
                        scanLine *= _ScanBrightness;

                        // Vary brightness per line so they don't all look identical
                        float lineBright = 0.6 + speedVar * 0.4;
                        finalColor += col * scanLine * lineBright;
                    }
                }

                FragOutput result;
                result.col = half4(finalColor, 1.0);
                result.dep = 1.0; // farthest possible — everything renders in front of us
                return result;
            }
            ENDHLSL
        }
    }
}
