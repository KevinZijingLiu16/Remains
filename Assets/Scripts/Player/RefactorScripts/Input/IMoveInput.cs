using UnityEngine;

public interface IMoveInput
{
 
    float MoveAxis { get; }

  
    bool ConsumeJumpPressed();
}
