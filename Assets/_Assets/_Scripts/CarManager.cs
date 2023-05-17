using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    [SerializeField] private Transform rayPosForward; // Transform for raycast in forward direction
    [SerializeField] private Transform rayPosBackward; // Transform for raycast in backward direction
    [SerializeField] private MeshCollider _meshCollider; // Car's mesh collider component
    [SerializeField] private float carSpeed; // Car's movement speed

    private bool isVertical = true; // Flag to indicate if the car is placed vertically
    private bool isReverse; // Flag to indicate if the car is facing the reverse direction
    private List<Transform> emptyHit; // List of transforms that the raycast hit with "Empty" tag
    private Transform rayPos; // Current raycast transform being used (either rayPosForward or rayPosBackward)
    private float movableGap; // Size of the gap the car should move

    private AudioManager audioManager; // Reference to the AudioManager script
    private LevelManager levelManager; // Reference to the LevelManager script

    private void Awake()
    {
        levelManager = FindObjectOfType<LevelManager>(); // Initialize the reference to LevelManager
        audioManager = FindObjectOfType<AudioManager>(); // Initialize the reference to AudioManager
        emptyHit = new List<Transform>(); // Initialize the emptyHit list

        SetFlags();
    }
    private void SetFlags()
    {
        // Set the isVertical and isReverse flags based on the car's orientation
        if (transform.forward == Vector3.right || transform.forward == Vector3.left)
            isVertical = false;
        if (transform.forward == Vector3.back || transform.forward == Vector3.left)
            isReverse = true;
    }

    public void MoveCar(bool isVertical, bool isPositive)
    {
        if (isVertical != this.isVertical) // If input direction doesn't match car orientation, return
            return;

        CheckPos(isPositive); // Set rayPos and movableGap based on input direction
        IsEmpty(rayPos); // Check if the raycast hit an emptyHit space or a road
        EnableCollider(); // Re-enable the box colliders for the hit gaps
    }

    private void CheckPos(bool isPositive)
    {
        // Set rayPos and movableGap based on input direction and car orientation
        if (isPositive)
        {
            rayPos = isReverse ? rayPosBackward : rayPosForward;
            movableGap = isReverse ? -2f : 2f;
        }
        else
        {
            rayPos = isReverse ? rayPosForward : rayPosBackward;
            movableGap = isReverse ? 2f : -2f;
        }
    }

    private void IsEmpty(Transform rayPos)
    {
        // If raycast hits an object
        if (Physics.Raycast(rayPos.position, rayPos.forward, out RaycastHit hit, 100.0f))
        {
            switch (hit.transform.tag)
            {
                case "Empty":
                    emptyHit.Add(hit.transform); // Add the hit transform to the emptyHit list
                    hit.transform.GetComponent<BoxCollider>().enabled = false; // Disable the box collider of the hit transform
                    IsEmpty(rayPos); // Continue checking if there are more emptyHit spaces
                    break;
                case "Road":
                    _meshCollider.enabled = false; // Disable the car's mesh collider
                    transform.DOMove(transform.position + transform.forward * movableGap * emptyHit.Count,
                        emptyHit.Count * (1 / carSpeed) * 1.5f) // Move the car to the last emptyHit gap, and then move to the road
                        .OnComplete(() => MoveAndRotate(hit));
                    break;
                default:
                    // Move the car to the last emptyHit gap and play the hit animation if the raycast hits a non-emptyHit and non-road object
                    transform.DOMove(transform.position + transform.forward * movableGap * emptyHit.Count,
                        emptyHit.Count * (1 / carSpeed) * 1.5f)
                        .OnComplete(() => HitAnimation());
                    break;
            }
        }
    }

    private void MoveAndRotate(RaycastHit hit)
    {
        // Move and rotate the car onto the road smoothly
        transform.DOMove(hit.transform.position + (hit.transform.forward * -1), (1 / carSpeed)).SetEase(Ease.Linear);
        transform.DORotate(hit.transform.rotation.eulerAngles, (1 / carSpeed)).SetEase(Ease.Linear).OnComplete(MoveToNextRoad);
    }

    private void MoveToNextRoad()
    {
        // Check if the car can move to the next road
        if (Physics.Raycast(rayPosForward.position, rayPosForward.forward, out RaycastHit hit, 100.0f) && hit.transform.CompareTag("Road"))
        {
            // Move and rotate the car to the next road
            transform.DOMove(hit.transform.position + (hit.transform.forward * -1), (1 / carSpeed)).SetEase(Ease.Linear);
            transform.DORotate(hit.transform.rotation.eulerAngles, (1 / carSpeed)).SetEase(Ease.Linear)
                .OnComplete(MoveToNextRoad);
        }
        else
        {
            // Move the car offscreen if there's no more road
            transform.DOMove(transform.position + transform.forward * 50f, (1 / carSpeed) * 50f);
            levelManager.CarList.RemoveAt(levelManager.CarList.Count - 1); // Remove the car from the CarList in LevelManager
            if (levelManager.CarList.Count == 0)
            {
                levelManager.LevelCompleted(); // Call LevelCompleted() in LevelManager if all cars have exited
            }
        }
    }

    private void EnableCollider()
    {
        // Re-enable the box colliders for the hit gaps
        foreach (Transform gapHit in emptyHit)
        {
            gapHit.GetComponent<BoxCollider>().enabled = true;
        }
        emptyHit.Clear(); // Clear the emptyHit list
    }

    private void HitAnimation()
    {
        float duration = 0.15f; // Duration for the hit animation
        Vector3 originalPos = transform.position; // Store the car's original position
        Vector3 targetPos = rayPos.position + (rayPos.forward * -1); // Calculate the target position for the animation
        float distance = Vector3.Distance(originalPos, targetPos); // Calculate the distance between the original and target positions

        // Animate the car moving to the target position and back to the original position
        DOTween.To(() => transform.position, pos => transform.position = pos,
            new Vector3(targetPos.x, originalPos.y, targetPos.z), duration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                transform.DOMove(originalPos, duration)
                    .SetEase(Ease.OutCubic)
                    .OnComplete(() =>
                    {
                        audioManager.PlayOneShotSFX("Collision"); // Play the collision sound effect
                    });
            });
    }
}