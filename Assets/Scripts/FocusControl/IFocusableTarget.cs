public interface IFocusableTarget
{
    void Focus();
    void Unfocus();
    bool ProcessInput(GFGInputAction action);
}