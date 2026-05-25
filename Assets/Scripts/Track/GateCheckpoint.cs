using UnityEngine;

public class GateCheckpoint : Checkpoint
{
    [SerializeField] private Animator _gateAnimator;
    [SerializeField] private GameObject _colliderContainer;

    override protected bool ProcessPlayerEnter(Transform playerTransform)
    {
        Vector3 toPlayer = playerTransform.position - this.transform.position;
        bool enteredFromTheBack = Vector3.Dot(-this.transform.forward, toPlayer) > 0;
        return enteredFromTheBack;
    }

    protected override bool ProcessPlayerExit(Transform playerTransform)
    {
        Vector3 toPlayer = playerTransform.position - this.transform.position;
        bool exitFromFromt = Vector3.Dot(this.transform.forward, toPlayer) > 0;
        return exitFromFromt;
    }

    protected override void UpdateStatePresentation()
    {
        _gateAnimator.SetInteger("State", (int)_currentState);
        _colliderContainer.SetActive((int)_currentState < (int)CheckpointState.Open);
    }

}
