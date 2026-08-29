namespace SoroSoro.Events
{
    /// <summary>
    /// 到達日数・撃退数・ゲームオーバー理由などの結果情報を保持するデータ型。
    /// ゲームオーバー時やリザルト画面へのデータ受け渡しに使用します。
    ///
    /// 【重要】このクラスは2つの文脈で使い回されます。
    ///   ・OnDayCleared  … その日単位の値（Day1で撃退2体なら RepelledCount = 2）
    ///   ・OnGameOver    … ゲーム開始からの累計値
    /// どちらが入っているかは型からは判別できないため、
    /// 表示側は必ず購読しているイベントで呼び分けてください。
    /// （GameClearResultHandler.DisplayDayResult / DisplayResult がその呼び分けにあたります）
    /// </summary>
    public sealed class ResultData
    {
        // 到達日数
        public int ReachedDay { get; set; }

        // 撃退数（Anomalyを正しく撮影した回数）
        public int RepelledCount { get; set; }

        // 誤射数（Ambientを撮影してしまった回数）
        public int TotalWasted { get; set; }

        // 見送り成功数（Ambientを正しく無視した回数）
        public int TotalCorrect { get; set; }

        // 電池残量（0.0〜1.0）。累計を取らず、その時点の残量をそのまま入れます。
        public float BatteryRemaining { get; set; }

        // ゲームオーバー理由（日クリア時は未使用）
        public GameOverReason Reason { get; set; }
    }
}