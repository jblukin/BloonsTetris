using UnityEngine.InputSystem;

public interface IDraggable
{

    void OnDestroy();

    void SubscribeInputs();
    void UnsubscribeInputs();

    void OnDragStart( InputAction.CallbackContext ctx );
    void OnDragProccessing( InputAction.CallbackContext ctx );
    void OnDragEnd( InputAction.CallbackContext ctx );

}
