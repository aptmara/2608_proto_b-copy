// AmbientBgmController.cs
using UnityEngine;
using SoroSoro.Events;

namespace SorosoroKaerou
{
    /// <summary>
    /// 歩行中は足音BGM、常時は住宅街BGMを重ねて再生する。
    /// </summary>
    public sealed class AmbientBgmController : MonoBehaviour
    {
        [Header("住宅街BGM（常時ループ）")]
        [SerializeField] AudioSource townBgmSource;

        [Header("歩行BGM（歩行中のみループ）")]
        [SerializeField] AudioSource walkBgmSource;

        [SerializeField] float fadeSpeed = 2f;

        float walkTargetVolume;

        void OnEnable()
        {
            GameEvents.OnWalkingChanged += HandleWalkingChanged;
        }

        void OnDisable()
        {
            GameEvents.OnWalkingChanged -= HandleWalkingChanged;
        }

        void Start()
        {
            if (townBgmSource != null)
            {
                townBgmSource.loop = true;
                townBgmSource.Play();
            }

            if (walkBgmSource != null)
            {
                walkBgmSource.loop = true;
                walkBgmSource.volume = 0f;
                walkBgmSource.Play();
            }
        }

        void Update()
        {
            if (walkBgmSource == null) return;
            walkBgmSource.volume = Mathf.MoveTowards(
                walkBgmSource.volume,
                walkTargetVolume,
                fadeSpeed * Time.deltaTime
            );
        }

        void HandleWalkingChanged(bool isWalking)
        {
            walkTargetVolume = isWalking ? 1f : 0f;
        }
    }
}
