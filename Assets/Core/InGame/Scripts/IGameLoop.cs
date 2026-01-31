using System;

public interface IGameLoop
{
    event Action OnGameStarted;
    event Action<int> OnGameCompleted;

    void StartGame();
    void EndGame();
}