// SphereSurfaceCameraController.cs
using UnityEngine;

namespace SorosoroKaerou
{
    /// <summary>
    /// AndroidManagerが存在するとき、スマホの向いている方向を使って
    /// 球(中心 = sphereCenter, 半径 = sphereRadius)の中心からRayを飛ばし、
    /// その着弾点(球面上の点)にこのカメラ自身を配置し、球の法線方向(外向き)を向かせる。
    /// 振り返っている(IsTurnedBack)最中も見回し自体は止めない(前進の停止はPlayerForwardWalker側で行う)。
    /// Cameraコンポーネントと同じオブジェクトにアタッチして使用する。
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class SphereSurfaceCameraController : MonoBehaviour
    {
        [Header("対象")]
        [SerializeField] private Transform sphereCenter; // 球の中心にしたいオブジェクト

        [Header("球の設定")]
        [SerializeField] private float sphereRadius = 1f;

        [Header("ピッチ設定(仮。逆なら符号かfalseで反転)")]
        [SerializeField] private bool invertPitch = false;
        [SerializeField] private float maxPitchAngle = 80f;

        private void Update()
        {
            if (!AndroidManager.HasInstance) return;
            if (sphereCenter == null) return;

            // キャッシュせず毎フレーム取り直す(isBack判定等と必ず同じインスタンスを参照させるため)
            PhoneGyro phoneGyro = AndroidManager.Instance.GetComponent<PhoneGyro>();
            if (phoneGyro == null) return;

            // isBack判定で実際に正しく機能しているrelativeZ(0=正面, 180=背後の連続角度)を
            // そのまま体の向きとして使う。0度の基準はsphereCenterのforwardとし、
            // そこからsphereCenterのupを軸に回転させる
            float yaw = phoneGyro.relativeZ;
            Quaternion yawRotation = Quaternion.AngleAxis(yaw, sphereCenter.up);
            Vector3 yawedForward = yawRotation * sphereCenter.forward;
            Vector3 yawedRight = yawRotation * sphereCenter.right;

            // ピッチ(スマホの上下の傾き)。オイラー角ではなくAtan2ベースで
            // ジンバルロックしないように計算されたphoneGyro.pitchを使う
            float pitch = Mathf.Clamp(phoneGyro.pitch, -maxPitchAngle, maxPitchAngle);
            if (invertPitch) pitch = -pitch;

            Vector3 direction = Quaternion.AngleAxis(pitch, yawedRight) * yawedForward;
            if (direction.sqrMagnitude < 0.0001f) return;
            direction.Normalize();

            // 球の中心から飛ばしたRayは、必ず中心からちょうどsphereRadiusの位置で球面と交差するため
            // 交点は幾何学的に center + direction * radius で直接求まる(物理レイキャスト不要)
            Vector3 hitPoint = sphereCenter.position + direction * sphereRadius;
            Vector3 normal = direction; // 球面上のその点における法線 = 中心からの方向そのもの

            transform.position = hitPoint;
            transform.rotation = Quaternion.LookRotation(normal);
        }

        private void OnDrawGizmosSelected()
        {
            Transform center = sphereCenter != null ? sphereCenter : transform;
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            Gizmos.DrawWireSphere(center.position, sphereRadius);
        }
    }
}
