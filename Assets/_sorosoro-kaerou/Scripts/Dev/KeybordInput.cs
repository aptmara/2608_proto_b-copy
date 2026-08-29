using UnityEngine;

namespace SorosoroKaerou
{
    /// <summary>
    /// Unity Editor確認用のキーボード入力実装
    /// </summary>
    public sealed class KeyboardInput : MonoBehaviour, IPlayerInput
    {
        [SerializeField] private float turnInterpolationDuration = 0.2f;

        private float currentTurnAxis;

        public float TurnAxis => currentTurnAxis;

        public bool IsTurnedBack => Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);

        public bool ShutterDown => Input.GetKeyDown(KeyCode.Z);

        private void Update()
        {
            float targetAxis = IsTurnedBack ? 1.0f : 0.0f;
            float step = turnInterpolationDuration > 0f ? (1.0f / turnInterpolationDuration) * Time.deltaTime : 1.0f;
            currentTurnAxis = Mathf.MoveTowards(currentTurnAxis, targetAxis, step);
        }
    }
}