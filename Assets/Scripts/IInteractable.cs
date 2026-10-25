/// <summary>
/// 可交互物体。玩家准星对准后按 E 触发。
/// </summary>
public interface IInteractable
{
    string Prompt { get; }
    void Interact(PlayerInteractor interactor);
}
