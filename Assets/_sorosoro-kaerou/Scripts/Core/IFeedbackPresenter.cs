namespace SorosoroKaerou
{
    /// <summary>
    /// プレイヤーへのフィードバック（ライト・フラッシュ・振動等）を実行するインターフェース
    /// </summary>
    public interface IFeedbackPresenter
    {
        /// <summary>ライトの点灯・消灯を切り替える</summary>
        /// <param name="on">点灯状態にする場合はtrue</param>
        void SetLight(bool on);

        /// <summary>撮影時のフラッシュ発光を実行する</summary>
        void Flash();

        /// <summary>端末の振動を実行する</summary>
        void Vibrate();
    }
}