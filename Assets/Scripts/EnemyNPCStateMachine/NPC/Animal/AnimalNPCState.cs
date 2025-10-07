using UnityEngine;
using System.Collections.Generic;

public abstract class AnimalNPCState : NPCBaseState
{
    protected AnimalNPCStateMachine animalStateMachine;

    protected AnimalNPCState(AnimalNPCStateMachine stateMachine) : base(stateMachine)
    {
        this.animalStateMachine = stateMachine;
    }
}