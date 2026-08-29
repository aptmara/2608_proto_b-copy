namespace SoroSoro.Events
{
    /// <summary>
    /// 到達日数・撃退数・ゲームオーバー理由などの結果情報を保持するデータ型。
    /// ゲームオーバー時やリザルト画面へのデータ受け渡しに使用します。
    /// </summary>
    public sealed class ResultData
    {
        // 到達日数
        public int ReachedDay { get; set; }

        // 撃退数
        public int RepelledCount { get; set; }

        // ゲームオーバー理由
        public GameOverReason Reason { get; set; }
    }
}