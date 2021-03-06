using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int moveDirection = 1;
    public MovementController movement;
    public Vector2 edgeRayOffset = new Vector2(1.0f, 0.0f);
    public float edgeRayDistance = 1.5f;
    public SpriteRenderer sprite;
    public bool wasBlockedLastFrame = false;
    public LayerMask HazardLayerMask;
    public PlayerController playerController;
    public CharacterAnimator animator;
    public CharacterAudioController audioController;
    public PauseManager pauseManager;

    public void Start() {
        if (!pauseManager) pauseManager = FindObjectOfType<PauseManager>();
        if (!playerController) playerController = FindObjectOfType<PlayerController>();
    }

    public void FixedUpdate() {
        if (pauseManager.IsPaused) return;
        movement.Move(moveDirection, Time.deltaTime);
    }

    public void Update() {
        if (pauseManager.IsPaused) return;
        Vector2 edgeRayOrigin = (Vector2)transform.position + new Vector2(edgeRayOffset.x * moveDirection, edgeRayOffset.y);

        if (movement.IsHorizontalBlocked) {
            if (wasBlockedLastFrame == false) {
                moveDirection *= -1;
                wasBlockedLastFrame = true;
            }
        } else {
            wasBlockedLastFrame = false;
        }

        if (Physics2D.Raycast(edgeRayOrigin, Vector2.down, edgeRayDistance, movement.movementSettings.blockingLayer.value) == false) {
            moveDirection *= -1;
        }

        audioController.SetIsMoving(movement.IsGrounded && Mathf.Abs(movement.Velocity.x) > 0.5f);

        sprite.flipX = movement.LastDirection > 0f;
        animator.SetWalking(true);
    }

    public void OnDrawGizmos() {
        Vector2 edgeRayOrigin = (Vector2)transform.position + new Vector2(edgeRayOffset.x * moveDirection, edgeRayOffset.y);
        Gizmos.DrawRay(edgeRayOrigin, Vector2.down * edgeRayDistance);
    }

    public void OnCollisionEnter2D(Collision2D collision) {
        if (TestLayer(collision.gameObject.layer)) {
            playerController.OnEnemyHazard(this);
        }
    }

    private bool TestLayer(int otherLayer) {
        return HazardLayerMask == (HazardLayerMask.value | (1 << otherLayer));
    }
}
