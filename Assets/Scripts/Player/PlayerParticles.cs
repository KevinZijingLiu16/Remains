using UnityEngine;

public class PlayerParticles : MonoBehaviour
{
    [Header("References")]
    public SplineRunnerRB splineRunner; // Reference to your movement script
    public ParticleSystem moveParticles;

    [Header("Settings")]
    public float minMoveSpeed = 0.1f; // Threshold to start particles

    void Start()
    {
        if (splineRunner == null)
            splineRunner = GetComponent<SplineRunnerRB>();

        if (moveParticles != null)
            moveParticles.Stop();
    }

    void Update()
    {
        if (splineRunner == null || moveParticles == null) return;

        // Check if player is moving
        bool isMoving = Mathf.Abs(splineRunner.moveSpeed * splineRunner.moveAction.action.ReadValue<float>()) > minMoveSpeed;

        if (isMoving)
        {
            if (!moveParticles.isPlaying)
                moveParticles.Play();
        }
        else
        {
            if (moveParticles.isPlaying)
                moveParticles.Stop();
        }
    }
}
