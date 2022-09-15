using System.Collections.Generic;
using UnityEngine;

public abstract class StepsRecorder
{
    public event System.Action StepRecorded;

    protected List<Transform> IslandsTransforms = new List<Transform>();

    public const float IslandAnimationDuration = 0.1f;

    protected StepsRecorder(List<Island> islands){
        IslandsTransforms = IslandsContainer.GetIslandsTransforms(islands);
    }

    protected StepsRecorder(List<Transform> islandsTransforms){
        IslandsTransforms = islandsTransforms;
    }

    public abstract void RecordStep();

    public abstract void MoveToPreviousStep();

    public abstract bool CanMoveToPrevStep();

    protected void InvokeStepRecorded(){
        StepRecorded?.Invoke();
    }
    
    protected struct IslandsState{
        public IslandsState(List<Vector3> islandsPositions, List<Vector3> islandsRotations){
            IslandsPositions = islandsPositions;
            IslandsRotations = islandsRotations;
        }

        public List<Vector3> IslandsPositions;
        public List<Vector3> IslandsRotations;
    }
}