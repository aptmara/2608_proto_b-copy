Shader "Hidden/Custom/DarkRoomPostProcess"
{
    Properties
    {
        _BaseColor ("Darkness Color", Color) = (0, 0, 0, 1)
        _LightColor ("Light Color", Color) = (1, 0.9, 0.7, 1)
        _LightRadius ("Light Radius (Max Distance)", Float) = 1.0
        _Softness ("Softness (ぼやけ具合)", Float) = 3.0
        _SpotAngle ("Spot Angle (半角, 度)", Range(1, 179)) = 30
        _ParticleIntensity ("Particle/Dust Intensity", Range(0, 1)) = 0.05
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "DarkRoomPass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            float3 _LightPos;
            float3 _LightDir;
            float4 _BaseColor;
            float4 _LightColor;
            float _LightRadius;
            float _Softness;
            float _SpotAngle;
            float _ParticleIntensity;

            // 標準Full Screen Pass用のテクスチャ変数
            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            // ワールド座標を種にした疑似乱数(0-1)
            float Hash13(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z);
            }

            // FullScreenPassRendererFeatureはメッシュを使わずDrawProceduralで
            // 3頂点を描画するため、SV_VertexIDから全画面三角形を再構成する
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 元の画面の色を取得
                half4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, input.uv);

                // 深度からワールド座標を計算
                float depth = SampleSceneDepth(input.uv);

                // スカイボックス(最遠)は対象外。深度も不安定でノイズの原因になるため、
                // ワールド座標を復元せず元の色をそのまま返す
                if (depth == UNITY_RAW_FAR_CLIP_VALUE)
                {
                    return col;
                }

                float3 worldPos = ComputeWorldSpacePosition(input.uv, depth, UNITY_MATRIX_I_VP);

                // 距離による減衰(懐中電灯の届く範囲。中心ほど明るく、なめらかにフェードアウト)
                float dist = distance(worldPos, _LightPos);
                float normalizedDist = saturate(dist / max(_LightRadius, 0.0001));
                float distAttenuation = pow(1.0 - normalizedDist, max(_Softness, 0.01));

                // 向き(カメラのforward)による減衰(懐中電灯のコーン)
                float3 toPixel = normalize(worldPos - _LightPos);
                float3 lightDir = normalize(_LightDir);
                float cosAngle = dot(toPixel, lightDir);
                float cosOuter = cos(radians(_SpotAngle));
                float cosInner = cos(radians(_SpotAngle * 0.5));
                float spotAttenuation = smoothstep(cosOuter, cosInner, cosAngle);

                float attenuation = distAttenuation * spotAttenuation;

                // 懐中電灯の光の中に浮かぶ塵のような粒子感
                // 空間を「スロット」に区切り、スロットごとに寿命10秒でごく稀に生まれては
                // なめらかに漂いフェードイン/アウトする(格子スナップによる瞬間移動を避けるため、
                // 存在判定と位相は固定スロット基準、実際の見た目位置だけを連続的に動かす)
                float slotFreq = 8.0; // スロットの大きさ(1/8 = 12.5cm四方に1粒まで)
                float3 slot = floor(worldPos * slotFreq);
                float3 slotCenter = (slot + 0.5) / slotFreq;

                // このスロットが今回の抽選で光る対象かどうか(_ParticleIntensityで頻度を調整)
                float spawnChance = saturate(_ParticleIntensity);
                float exists = step(1.0 - spawnChance, Hash13(slot + 99.9));

                // 寿命10秒、めったに出現しないロングサイクルでフェードイン/アウト
                float lifespan = 10.0;
                float cycleLength = 60.0; // 平均してこの秒数に1回だけ出現チャンスがある
                float phaseOffset = Hash13(slot + 55.5) * cycleLength;
                float cycleTime = fmod(_Time.y + phaseOffset, cycleLength);
                float aliveWindow = step(cycleTime, lifespan);
                float lifeFade = sin(PI * saturate(cycleTime / lifespan));
                float life = aliveWindow * lifeFade * exists;

                // スロット内でゆっくりふわふわ漂う(連続的な位置、格子スナップなし)
                float3 randDir = float3(
                    Hash13(slot + float3(11.1, 0.0, 0.0)),
                    Hash13(slot + float3(0.0, 22.2, 0.0)),
                    Hash13(slot + float3(0.0, 0.0, 33.3))) * 2.0 - 1.0;
                float speed = 0.03 + Hash13(slot + 7.77) * 0.05;
                float3 wander = float3(
                    sin(_Time.y * speed + randDir.x * 10.0),
                    sin(_Time.y * speed * 0.8 + randDir.y * 10.0),
                    sin(_Time.y * speed * 1.2 + randDir.z * 10.0)) * 0.35;

                float3 motePos = slotCenter + wander;
                float moteRadius = 0.05; // 粒の大きさ
                float dotMask = smoothstep(moteRadius, 0.0, distance(worldPos, motePos));

                float sparkle = dotMask * life * spotAttenuation;

                float finalMix = max(attenuation, sparkle);

                // 粒はBloomに引っかかるように別枠で明るくブーストして加算する
                half sparkleBoost = 5.0;
                half4 lit = col + (_LightColor * attenuation) + (_LightColor * sparkle * sparkleBoost);

                // 暗闇と元の色を合成
                return lerp(_BaseColor, lit, finalMix);
            }
            ENDHLSL
        }
    }
}