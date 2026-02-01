// RibbonGuide.shader
// Holographic guide ribbon for rhythm game note paths.
// Designed to be applied to a strip of quads (or a TrailRenderer/custom ribbon mesh)
// where UV.x runs along the ribbon length [0,1] and UV.y runs across the width [0,1].
//
// Mesh expectations:
//   UV0.x = longitudinal position along ribbon (0 = start, 1 = end / hit plane)
//   UV0.y = transverse position across ribbon width (0 = edge, 1 = center, or 0->1 edge-to-edge)
//   NORMAL = facing camera (or use Two Sided)
//
// If your ribbon mesh uses UV.y as 0-to-1 edge-to-edge, enable the
// "Symmetric Width" toggle so the shader remaps it to [-1, 1] centered on 0.

Shader "Rhythm/RibbonGuide"
{
    Properties
    {
        // ─── Core Colors ────────────────────────────────────────────────
        _BaseColor("Base Color (tint over near-black)", Color) = (0.02, 0.025, 0.03, 1.0)
        _CoreColor("Core Neon Color", Color) = (0.2, 0.9, 1.0, 1.0)
        _GlowColor("Glow / Halo Color", Color) = (0.15, 0.7, 0.9, 1.0)

        // ─── Core & Glow Shape ──────────────────────────────────────────
        _CoreWidth("Core Half-Width (0-1 of ribbon half)", Float) = 0.12
        _CoreSharpness("Core Edge Sharpness", Float) = 6.0
        _GlowWidth("Glow Half-Width (0-1 of ribbon half)", Float) = 0.25
        _GlowFalloff("Glow Falloff Exponent", Float) = 3.5
        _CoreIntensity("Core Brightness", Float) = 1.4
        _GlowIntensity("Glow Brightness", Float) = 0.25
        _BaseIntensity("Base (dark body) Brightness", Float) = 0.15

        // ─── Flow Animation ─────────────────────────────────────────────
        _FlowSpeed("Flow Pattern Speed", Float) = 0.6
        _FlowScale("Flow Pattern Scale (repeats along length)", Float) = 4.0
        _FlowContrast("Flow Contrast (0=subtle, 1=strong)", Float) = 0.08
        _FlowOffset("Flow UV Offset (for staggering multiple ribbons)", Float) = 0.0

        // ─── Beat Pulse ─────────────────────────────────────────────────
        // Set _BeatPhase from C# every beat: ramp 0->1 over one beat period.
        // The shader maps this through a smooth pulse envelope.
        _BeatPhase("Beat Phase (0->1 per beat, set from C#)", Float) = 0.0
        _PulseIntensityAdd("Pulse Peak Brightness Add", Float) = 0.25
        _PulseWidenCore("Pulse Core Widen Amount", Float) = 0.02
        _PulseDecaySharp("Pulse Decay Sharpness (higher = snappier decay)", Float) = 3.0

        // ─── Intensity / Logic-State Modulation ─────────────────────────
        // _IntensityLevel: 0 = calm, 1 = intense. Boosts glow and core.
        _IntensityLevel("Section Intensity (0=calm, 1=intense)", Float) = 0.0
        _IntensityGlowBoost("Intensity Glow Boost", Float) = 0.25
        _IntensityCoreBoost("Intensity Core Boost", Float) = 0.2

        // ─── Logic-State Hue Shift ──────────────────────────────────────
        // _LogicStateA and _LogicStateB are the two colors to lerp between.
        // _LogicStateBlend is set from C# (0 or 1, or smoothly transitioned).
        _LogicStateA("Logic State A Color", Color) = (0.2, 0.9, 1.0, 1.0)
        _LogicStateB("Logic State B Color", Color) = (0.9, 0.3, 0.8, 1.0)
        _LogicStateBlend("Logic State Blend (0=A, 1=B, set from C#)", Float) = 0.0
        _LogicStateGlowShift("Logic State Glow Shift Strength", Float) = 0.6

        // ─── Global Alpha ───────────────────────────────────────────────
        // Master opacity for the entire ribbon. Set from C# to fade
        // the ribbon in/out or keep it semi-transparent at all times.
        _GlobalAlpha("Global Alpha", Float) = 0.7

        // ─── Lane Edges ─────────────────────────────────────────────────
        // Hard vertical cutoff at the ribbon edges to clearly delineate lanes.
        // EdgeSharpness controls how abrupt the falloff is at the sides:
        //   higher = sharper / more rectangular, lower = slightly softer.
        _EdgeSharpness("Edge Sharpness", Float) = 30.0

        // ─── Depth & Fade ───────────────────────────────────────────────
        // Fade out the ribbon near the hit plane (UV.x approaching 1)
        // so it doesn't visually compete with the hit zone.
        _HitPlaneFadeStart("Hit Plane Fade Start (UV.x)", Float) = 0.85
        _HitPlaneFadeEnd("Hit Plane Fade End (UV.x)", Float) = 1.0

        // Fade in near the spawn (UV.x near 0) for a clean entrance.
        _SpawnFadeEnd("Spawn Fade End (UV.x)", Float) = 0.04

        // ─── UV Layout ──────────────────────────────────────────────────
        // Enable this if the flow pattern moves across the ribbon width
        // instead of along its length. Swaps UV.x and UV.y so the shader
        // treats your mesh's U as the width axis and V as the length axis.
        [Toggle] _SwapUVs("Swap UVs (U=width, V=length)", Float) = 0.0

        // ─── Symmetry ───────────────────────────────────────────────────
        [Toggle] _SymmetricWidth("Symmetric Width (remap UV.y 0->1 to -1->1)", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent-1"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }

        LOD 100

        Pass
        {
            Name "RibbonGuide"

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off  // visible from both sides

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // ── Properties ──────────────────────────────────────────────
            // Core Colors
            uniform float4 _BaseColor;
            uniform float4 _CoreColor;
            uniform float4 _GlowColor;

            // Core & Glow Shape
            uniform float _CoreWidth;
            uniform float _CoreSharpness;
            uniform float _GlowWidth;
            uniform float _GlowFalloff;
            uniform float _CoreIntensity;
            uniform float _GlowIntensity;
            uniform float _BaseIntensity;

            // Flow
            uniform float _FlowSpeed;
            uniform float _FlowScale;
            uniform float _FlowContrast;
            uniform float _FlowOffset;

            // Beat Pulse
            uniform float _BeatPhase;
            uniform float _PulseIntensityAdd;
            uniform float _PulseWidenCore;
            uniform float _PulseDecaySharp;

            // Intensity
            uniform float _IntensityLevel;
            uniform float _IntensityGlowBoost;
            uniform float _IntensityCoreBoost;

            // Logic State
            uniform float4 _LogicStateA;
            uniform float4 _LogicStateB;
            uniform float _LogicStateBlend;
            uniform float _LogicStateGlowShift;

            // Depth & Fade
            uniform float _HitPlaneFadeStart;
            uniform float _HitPlaneFadeEnd;
            uniform float _SpawnFadeEnd;

            // Global Alpha & Lane Edges
            uniform float _GlobalAlpha;
            uniform float _EdgeSharpness;

            // Symmetry
            uniform float _SymmetricWidth;

            // UV Layout
            uniform float _SwapUVs;

            // ── Vertex shader ───────────────────────────────────────────
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // ── Utility functions ───────────────────────────────────────

            // Attempt to make a smooth pulse from a sawtooth phase [0,1].
            // Sharp initial hit, then exponential decay. Feels percussive.
            float PulseEnvelope(float phase)
            {
                // phase 0 = beat hit, ramps to 1 over the beat.
                // We want peak at phase=0, decay toward phase=1.
                // Invert so the peak is at the start of the beat.
                float t = 1.0 - phase; // t=1 at beat hit, t=0 at end of beat
                // Exponential decay from the hit
                return pow(t, _PulseDecaySharp);
            }

            // Generates thin discrete scrolling lines along the ribbon.
            // _FlowScale controls repeats (fewer = more spaced out).
            // The output is a narrow bright pulse per cycle, mostly dark between them.
            float FlowPattern(float uvX, float time)
            {
                float coord = uvX * _FlowScale - time * _FlowSpeed + _FlowOffset;
                // 0->1 sawtooth per repeat
                float saw = frac(coord);
                // Thin bright line near saw==0, dark everywhere else.
                // saw==0 is the leading edge of each cycle; ramp it to 0 over 0.08 of the cycle.
                float flowPulse = saturate(1.0 - saw / 0.08);
                // Blend back toward 1.0 (neutral) using contrast param
                return 1.0 + _FlowContrast * flowPulse;
            }

            // ── Fragment shader ─────────────────────────────────────────
            float4 frag(v2f i) : SV_Target
            {
                float uvX = i.uv.x; // along ribbon (0=spawn, 1=hit plane)
                float uvY = i.uv.y; // across ribbon width

                // ── UV swap ─────────────────────────────────────────────
                if (_SwapUVs > 0.5)
                {
                    float tmp = uvX;
                    uvX = uvY;
                    uvY = tmp;
                }

                // ── Symmetry remap ──────────────────────────────────────
                // Convert uvY to a distance-from-center value [0, 1]
                // where 0 = dead center, 1 = edge.
                float widthDist;
                if (_SymmetricWidth > 0.5)
                {
                    // Input is 0->1 edge-to-edge. Remap to distance from 0.5.
                    widthDist = abs(uvY * 2.0 - 1.0); // 0 at center, 1 at edges
                }
                else
                {
                    // Input is already 0 (center) -> 1 (edge), or similar.
                    widthDist = uvY;
                }

                // ── Longitudinal fade (spawn & hit plane) ───────────────
                float spawnFade = smoothstep(0.0, _SpawnFadeEnd, uvX);
                float hitFade = 1.0 - smoothstep(_HitPlaneFadeStart, _HitPlaneFadeEnd, uvX);
                float lengthFade = spawnFade * hitFade;

                // ── Beat pulse envelope ─────────────────────────────────
                float beatPulse = PulseEnvelope(_BeatPhase);

                // ── Logic state color blend ─────────────────────────────
                // Lerp between the two logic-state colors
                float3 logicColor = lerp(_LogicStateA.rgb, _LogicStateB.rgb, _LogicStateBlend);

                // Blend the logic color into core and glow based on strength param
                float3 effectiveCoreColor = lerp(_CoreColor.rgb, logicColor, _LogicStateBlend * _LogicStateGlowShift);
                float3 effectiveGlowColor = lerp(_GlowColor.rgb, logicColor, _LogicStateBlend * _LogicStateGlowShift * 0.7);

                // ── Intensity modulation ────────────────────────────────
                float intensityCoreMult = 1.0 + _IntensityLevel * _IntensityCoreBoost;
                float intensityGlowMult = 1.0 + _IntensityLevel * _IntensityGlowBoost;

                // ── Flow pattern ────────────────────────────────────────
                float flow = FlowPattern(uvX, _Time.y);

                // ── Core layer ──────────────────────────────────────────
                // Effective core width widens slightly on pulse
                float coreWidthEff = _CoreWidth + _PulseWidenCore * beatPulse;
                // Smooth falloff: 1 at center, 0 at coreWidthEff
                float coreMask = 1.0 - smoothstep(0.0, coreWidthEff, widthDist);
                // Sharpen the core edges for a crisp neon line
                coreMask = pow(coreMask, 1.0 / max(_CoreSharpness, 0.01));

                float coreIntensityEff = _CoreIntensity * intensityCoreMult * flow;
                // Pulse adds brightness
                coreIntensityEff += _PulseIntensityAdd * beatPulse;

                float3 coreLayer = effectiveCoreColor * coreMask * coreIntensityEff;

                // ── Glow layer ──────────────────────────────────────────
                // Smooth falloff from center. Wider than core.
                float glowMask = 1.0 - smoothstep(0.0, _GlowWidth, widthDist);
                glowMask = pow(glowMask, _GlowFalloff);

                float glowIntensityEff = _GlowIntensity * intensityGlowMult * flow;
                // Pulse also subtly boosts glow
                glowIntensityEff += _PulseIntensityAdd * 0.3 * beatPulse;

                float3 glowLayer = effectiveGlowColor * glowMask * glowIntensityEff;

                // ── Base dark body ──────────────────────────────────────
                // The near-black body that fills the full ribbon width.
                // Gets a very faint tint from the base color.
                float3 baseLayer = _BaseColor.rgb * _BaseIntensity * flow;

                // ── Combine layers ──────────────────────────────────────
                // Additive layering: base + glow + core
                float3 finalColor = baseLayer + glowLayer + coreLayer;

                // ── Alpha ───────────────────────────────────────────────
                // Hard edge cutoff at ribbon sides using widthDist.
                // widthDist is 0 at center, 1 at edges. We want full opacity
                // up until the very edge, then a sharp drop. Clamp widthDist
                // to just under 1 and apply a steep smoothstep right at the boundary.
                float edgeMask = 1.0 - smoothstep(0.95, 1.0, widthDist * _EdgeSharpness / 30.0);

                // Combine: longitudinal fade * edge mask * global alpha
                float alpha = lengthFade * edgeMask * _GlobalAlpha;

                return float4(finalColor, alpha);
            }
            ENDHLSL
        }
    }

    // Fallback for systems that can't run this shader
    Fallback "Transparent/VertexLit"

    // ── C# runtime control reference ────────────────────────────────────
    // Material mat = ribbonRenderer.material;
    //
    // Per-beat:
    //   mat.SetFloat("_BeatPhase", currentBeatPhase);  // 0->1 sawtooth per beat
    //
    // On section/intensity change:
    //   mat.SetFloat("_IntensityLevel", normalizedIntensity);  // 0.0 to 1.0
    //
    // On logic-state change:
    //   mat.SetFloat("_LogicStateBlend", targetState);  // 0.0 or 1.0 (lerp in C# for smooth transition)
    //
    // Optionally override the logic-state colors per lane:
    //   mat.SetColor("_LogicStateA", stateAColor);
    //   mat.SetColor("_LogicStateB", stateBColor);
    // ────────────────────────────────────────────────────────────────────
}