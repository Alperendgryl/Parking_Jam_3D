using UnityEngine;

public class InputManager : MonoBehaviour
{
    private Vector2 mousePosFirst; // Initial mouse position when mouse button is pressed
    private Vector2 mouseVector; // Vector representing the mouse movement

    private bool isVertical; // Flag to indicate if the swipe is in the vertical direction
    private bool isPositive; // Flag to indicate if the swipe is in the positive direction

    private GameObject car; // Selected car GameObject

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseDown(); // Handle mouse button down event
        }

        if (Input.GetMouseButtonUp(0))
        {
            HandleMouseUp(); // Handle mouse button up event
        }
    }

    private void HandleMouseDown()
    {
        Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Create a ray from the camera to the mouse position
        if (Physics.Raycast(_ray, out RaycastHit hit, 100.0f) && hit.transform.CompareTag("Car")) // Check if the raycast hits a car
        {
            car = hit.transform.gameObject; // Store the selected car GameObject
            mousePosFirst = Input.mousePosition; // Store the initial mouse position
        }
    }

    private void HandleMouseUp()
    {
        mouseVector.x = Input.mousePosition.x - mousePosFirst.x; // Calculate the horizontal mouse movement
        mouseVector.y = Input.mousePosition.y - mousePosFirst.y; // Calculate the vertical mouse movement
        DirectionController(mouseVector.x, mouseVector.y); // Determine the direction of the swipe
        MoveCar(); // Move the selected car based on the swipe direction
    }

    private void DirectionController(float x, float y)
    {
        isVertical = Mathf.Abs(x) <= Mathf.Abs(y); // Check if the swipe is vertical
        isPositive = (isVertical ? y : x) >= 0; // Check if the swipe is in the positive direction
    }

    private void MoveCar()
    {
        if (car != null)
        {
            // Call MoveCar method in CarManager with the determined direction
            car.GetComponent<CarManager>().MoveCar(isVertical, isPositive);
            car = null; // Reset the selected car
        }
    }
}