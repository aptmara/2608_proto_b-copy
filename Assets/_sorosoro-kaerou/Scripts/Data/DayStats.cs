namespace SoroSoro.Events
{
    /// <summary>
    /// 判定結果の集計器。GameManager内部でのみ使用します。
    /// 日単位（dayStats）と累計（totalStats）の2つのインスタンスを使い分け、
    /// 外部への通知時はResultDataに詰め替えます。
    ///
    /// JudgeResult.Missed は集計しません。
    /// allowGameOverがtrueなら即GameOverで、falseなDay0では記録する意味がないためです。
    /// </summary>
    public sealed class DayStats
    {
        // 撃退数（Anomalyを正しく撮影した回数）
        public int Repelled;

        // 誤射数（Ambientを撮影してしまった回数）
        public int Wasted;

        // 見送り成功数（Ambientを正しく無視した回数）
        public int Correct;

        // 電池残量（0.0〜1.0）
        public float BatteryRemaining;

        public void Reset()
        {
            Repelled = 0;
            Wasted = 0;
            Correct = 0;
            BatteryRemaining = 1f;
        }

        public void Add(JudgeResult result)
        {
            switch (result)
            {
                case JudgeResult.Repelled:
                    Repelled++;
                    break;
                case JudgeResult.Wasted:
                    Wasted++;
                    break;
                case JudgeResult.Correct:
                    Correct++;
                    break;
            }
        }
    }
}