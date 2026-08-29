namespace SorosoroKaerou
{
    /// <summary>
    /// プレイヤーからの入力状態を提供するインターフェース
    /// </summary>
    public interface IPlayerInput
    {
        /// <summary>背後を向いた度合い（-1.0〜1.0）</summary>
        float TurnAxis { get; }

        /// <summary>閾値を超えて背後を向いているか（進行ゲート兼ライト点灯状態）</summary>
        bool IsTurnedBack { get; }

        /// <summary>撮影ボタンを押した瞬間か（判定入力）</summary>
        bool ShutterDown { get; }
    }
}