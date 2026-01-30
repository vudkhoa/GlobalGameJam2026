using UnityEngine;
using DG.Tweening; // Dùng cho delay

// 1. Trạng thái ĐANG CHƠI
public class StatePlaying : PuzzleState
{
    public StatePlaying(PuzzleController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("--- STATE: PLAYING ---");
        // Có thể bật nhạc nền game ở đây
    }

    public override bool CanInteract() 
    { 
        return true; // Cho phép bấm nút
    }
    
    // Logic kiểm tra thắng nằm ở đây (Controller sẽ gọi vào)
    public void CheckWinCondition()
    {
        // Controller lo việc đếm ô, State chỉ quyết định chuyển gì tiếp theo
        if (_controller.AreAllSlotsFilled())
        {
            _controller.SwitchState(new StateLevelComplete(_controller));
        }
    }
}

// 2. Trạng thái HOÀN THÀNH 1 LEVEL (Chờ chuyển cảnh)
public class StateLevelComplete : PuzzleState
{
    public StateLevelComplete(PuzzleController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("--- STATE: LEVEL COMPLETE (Delay) ---");
        
        // Chờ 1.5 giây rồi quyết định đi tiếp hay dừng
        DOVirtual.DelayedCall(1.5f, () => 
        {
            // Logic điều hướng
            if (_controller.HasMoreLevels())
            {
                _controller.LoadNextLevelData(); // Load data mới
                _controller.SwitchState(new StatePlaying(_controller)); // Chơi tiếp
            }
            else
            {
                _controller.SwitchState(new StateEndingChoice(_controller)); // Hết bài -> Chọn
            }
        });
    }

    public override bool CanInteract() 
    { 
        return false; // Đang chờ chuyển màn, KHÔNG cho bấm nút linh tinh
    }
}

// 3. Trạng thái LỰA CHỌN CUỐI CÙNG (Bad/Good)
public class StateEndingChoice : PuzzleState
{
    public StateEndingChoice(PuzzleController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("--- STATE: ENDING CHOICE ---");
        _controller.ShowChoiceUI(); // Bảo Controller hiện UI lên
    }
    
    public override bool CanInteract() { return false; } // Không chơi xếp chữ nữa
}