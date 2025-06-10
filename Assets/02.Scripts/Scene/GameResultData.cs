public enum GameEndReason
{
    StageClear,
    GameOver
}

public static class GameResultData
{
    // 마지막 판에서 얻은 점수
    public static int LastScore;

    // 마지막 판의 종료 이유
    public static GameEndReason Reason;
}