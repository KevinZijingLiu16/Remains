using UnityEngine;

public interface IStateFactory
{
    State CreateState(string stateName);
}