namespace SorosoroKaerou
{
    /// <summary>
    /// プレイヤーへのフィードバック（ライト・フラッシュ・振動等）を実行するインターフェース
    /// </summary>
    public interface IFeedbackPresenter
    {
        bool IsLightOn { get; }

        void SetLight(bool on);
        void Flash();
        void Vibrate();
    }
}