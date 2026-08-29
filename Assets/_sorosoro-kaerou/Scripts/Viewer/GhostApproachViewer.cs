using System.Collections;
using UnityEngine;

namespace SoroSoro.Events
{
    /// <summary>
    /// 音イベント発火時（種別問わず）にプレイヤー後方へ黒い靄のPlaneを出現させ、
    /// 判定確定まで近づけ続ける演出。
    /// 撮影が成立すると正体画像（SoundEventDefinition.ghostSprite）に差し替わり、
    /// Missed以外の結果が出ると一定時間後に消える。
    /// Missedのときだけ消さずに残す（見逃した危険は放置しない、というゲームデザイン上の意図）。
    /// </summary>
    public class GhostApproachViewer : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField] private Transform player;
        [SerializeField] private SpriteRenderer prefab;
        [SerializeField] private Sprite mistSprite; // 正体不明時（出現直後）の見た目

        [Header("出現位置")]
        [SerializeField] private float spawnDistance = 8f;
        [SerializeField] private float spawnHeight = 1.5f;

        [Header("接近")]
        [SerializeField] private float approachSpeed = 0.8f;
        [SerializeField] private float stopDistance = 1.5f; // これ以上は近づかない

        [Header("消滅")]
        [Tooltip("撮影後、正体画像を見せてから消えるまでの時間（秒）")]
        [SerializeField] private float despawnDelay = 0.8f;

        private SpriteRenderer current;
        private Coroutine approachRoutine;
        private Coroutine despawnRoutine;

        private void OnEnable()
        {
            GameEvents.OnGhostAppeared += HandleAppeared;
            GameEvents.OnPhotoCaptured += HandleCaptured;
            GameEvents.OnJudged += HandleJudged;
        }

        private void OnDisable()
        {
            GameEvents.OnGhostAppeared -= HandleAppeared;
            GameEvents.OnPhotoCaptured -= HandleCaptured;
            GameEvents.OnJudged -= HandleJudged;
        }

        private void HandleAppeared()
        {
            if (player == null || prefab == null) return;

            // 前の個体がMissedで残っている状態で次のイベントが来た場合は入れ替える
            if (current != null) DespawnImmediate();

            Vector3 spawnPos = player.position - player.forward * spawnDistance + Vector3.up * spawnHeight;
            current = Instantiate(prefab, spawnPos, Quaternion.identity);
            current.sprite = mistSprite;

            approachRoutine = StartCoroutine(ApproachRoutine());
        }

        private IEnumerator ApproachRoutine()
        {
            while (current != null)
            {
                Vector3 toPlayer = player.position - current.transform.position;
                float distance = toPlayer.magnitude;

                if (distance > stopDistance)
                {
                    current.transform.position += toPlayer.normalized * approachSpeed * Time.deltaTime;
                }

                // ビルボード：常にプレイヤー側を向かせる。カメラ構成によっては調整が必要。
                if (toPlayer.sqrMagnitude > 0.0001f)
                {
                    current.transform.rotation = Quaternion.LookRotation(-toPlayer.normalized);
                }

                yield return null;
            }
        }

        private void HandleCaptured(Sprite sprite)
        {
            if (current == null) return;
            // sprite が null（環境音で個別画像が未設定）の場合は靄のまま＝「何も写らなかった」の表現
            if (sprite != null) current.sprite = sprite;
        }

        private void HandleJudged(JudgeResult result)
        {
            if (result == JudgeResult.Missed) return; // 見逃した危険は消さず近づき続ける
            if (current == null) return;

            if (despawnRoutine != null) StopCoroutine(despawnRoutine);
            despawnRoutine = StartCoroutine(DespawnRoutine());
        }

        private IEnumerator DespawnRoutine()
        {
            yield return new WaitForSeconds(despawnDelay);
            DespawnImmediate();
        }

        private void DespawnImmediate()
        {
            if (approachRoutine != null) StopCoroutine(approachRoutine);
            if (current != null) Destroy(current.gameObject);
            current = null;
        }
    }
}
