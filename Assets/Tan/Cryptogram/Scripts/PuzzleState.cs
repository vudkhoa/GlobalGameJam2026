public abstract class PuzzleState
{
    protected PuzzleController _controller; 

    public PuzzleState(PuzzleController controller)
    {
        _controller = controller;
    }

    public virtual void Enter() { }  
    public virtual void Exit() { }    
    public virtual void Update() { }  
    
    // Hàm xử lý Input: Trả về True nếu State này cho phép bấm nút
    public virtual bool CanInteract() { return false; }
}