using System;
using UnityEngine;

public class CameraHolderMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float smoothness = 8.0f;

    [Header("Direction Settings")]
    [SerializeField] private Vector3 moveDirection = Vector3.forward;

    // 現在の移動状態
    public bool IsMoving { get; private set; }

    // 演出拡張用イベント（演出用コンポーネントが購読して同期する）
    public event Action OnMoveStarted;
    public event Action OnMoveStopped;
    public event Action<float> OnStepUpdate; // 移動速度の割合(0.0〜1.0)を渡す

    private Vector3 currentVelocity;

    private void Start()
    {
        // 開始時に移動を自動スタート
        StartMove();
    }

    private void Update()
    {
        HandleMovement();
    }

    /// <summary>
    /// 移動を開始します
    /// </summary>
    public void StartMove()
    {
        if (IsMoving) return;

        IsMoving = true;
        OnMoveStarted?.Invoke();
    }

    /// <summary>
    /// 移動を停止します
    /// </summary>
    public void StopMove()
    {
        if (!IsMoving) return;

        IsMoving = false;
        OnMoveStopped?.Invoke();
    }

    /// <summary>
    /// 直進移動の処理となめらかな補間
    /// </summary>
    private void HandleMovement()
    {
        // 目標速度の算出（Stop時はゼロに減速）
        Vector3 targetVelocity = IsMoving ? moveDirection.normalized * moveSpeed : Vector3.zero;

        // なめらかな加減速処理 (Lerpによる感度調整)
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.deltaTime * smoothness);

        // 自身のローカル向き基準で移動を適用
        transform.position += transform.TransformDirection(currentVelocity) * Time.deltaTime;

        // 移動中、または慣性移動中に演出用イベントを発火
        if (currentVelocity.sqrMagnitude > 0.001f)
        {
            float currentSpeedRatio = currentVelocity.magnitude / moveSpeed;
            OnStepUpdate?.Invoke(currentSpeedRatio);
        }
    }
}