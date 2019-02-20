using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the opening and closing of doors. Allows doors to start open/closed and to open upwards/downwards. Open, Close, and OpenClose functions allow you to choose how switches affect doors.
/// </summary>

public class DoorControl : MonoBehaviour
{
    [Tooltip("The amount of time the door takes to open.")]
    public float timeToOpen = 2f;
    [Tooltip("The amount of time the door takes to close.")]
    public float timeToClose = 2f;
    [Tooltip("Should the door start closed?")]
    public bool startClosed = true;
    [Tooltip("When the door opens, should it rise or lower?")]
    public bool riseToOpen = true;
    
    private float heightOfDoor; // Height of the door's collider to determine the distance it needs to move to open or close.
    private Vector3 closedPosition; // The location the door goes to when it closes.
    private Vector3 openPosition; // The location to go to when it opens.
    private bool open; // Whether or not the door is open/opening or closed/closing.

    private void Start()
    {
        heightOfDoor = GetComponent<BoxCollider2D>().size.y; // Determine height of the door.
        open = !startClosed; // Set whether the door is currently open or closed.

        if (startClosed && riseToOpen) // If the door starts closed and rises...
        {
            openPosition = transform.position + Vector3.up * heightOfDoor; // Set open position to be above it.
            closedPosition = transform.position; // Set closed position to current.
        }
        else if (startClosed && !riseToOpen) // If the door starts closed and lowers...
        {
            openPosition = transform.position - Vector3.up * heightOfDoor; // Set open position to be below it.
            closedPosition = transform.position; // Set closed position to current.
        }
        else if (!startClosed && riseToOpen) // If the door starts open and rises...
        {
            closedPosition = transform.position; // Set closed position to current.
            transform.position += Vector3.up * heightOfDoor; // Set current position to above since it starts open.
            openPosition = transform.position; // Set open position to the new current position.
        }
        else // If the door starts open and lowers...
        {
            closedPosition = transform.position; // Set closed position to current.
            transform.position -= Vector3.up * heightOfDoor; // Set current position to below since it starts open.
            openPosition = transform.position; // Set open position to the new current position.
        }
    }

    public void Open() // When the Open function is called...
    {
        StopCoroutine(CloseDoor()); // Stop closing the door.
        StartCoroutine(OpenDoor()); // Start opening the door.
    }

    public void Close() // When the Close function is called...
    {
        StopCoroutine(OpenDoor()); // Stop opening the door.
        StartCoroutine(CloseDoor()); // Stop closing the door.
    }

    public void OpenClose() // When the OpenClose function is called...
    {
        if (open) // If the door is open/opening...
        {
            StopCoroutine(OpenDoor()); // Stop opening the door.
            StartCoroutine(CloseDoor()); // Stop closing the door.
        }
        else
        {
            StopCoroutine(CloseDoor()); // Stop closing the door.
            StartCoroutine(OpenDoor()); // Start opening the door.
        }
    }

    private IEnumerator OpenDoor() // Coroutine that opens the door.
    {
        open = true; // Set that the door is open/opening.
        float elapsedTime = 0; // Create float variable to hold elapsed time and set it to 0.

        Vector3 startingPosition = transform.position; // Create Vector3 variable to hold the position the door starts opening at.

        while (elapsedTime < timeToOpen) // While elapsed time has not reached the time it should take to open...
        {
            if (!open) // If the door is closing/closed...
                break; // break from the loop, so the coroutine can end.

            transform.position = Vector3.Lerp(startingPosition, openPosition, (elapsedTime / timeToOpen)); // Set the position of the door to a distance between the starting position and the open position. The position between the points is determined by the step that goes from 0 - 1. A step of 0.35 is 35% of the way between the points.
            elapsedTime += Time.deltaTime; // Increase the elapsed time by the time it took to complete the last frame.
            yield return new WaitForEndOfFrame(); // Wait for end of frame before looping through again to ensure accurate time keeping.
        }
    }

    private IEnumerator CloseDoor() // Coroutine that closes the door.
    {
        open = false; // Set that the door is closed/closing
        float elapsedTime = 0; // Create float variable to hold elapsed time and set it to 0;

        Vector3 startingPosition = transform.position; // Create Vector3 variable to hold the position the door starts opening at.

        while (elapsedTime < timeToClose) // While elapsed time has not reached the time it should take to open...
        {
            if (open) // If the door is opening/open...
                break; // break from the loop, so the coroutine can end.

            transform.position = Vector3.Lerp(startingPosition, closedPosition, (elapsedTime / timeToClose));  // Set the position of the door to a distance between the starting position and the closed position. The position between the points is determined by the step that goes from 0 - 1. A step of 0.35 is 35% of the way between the points.
            elapsedTime += Time.deltaTime; // Increase the elapsed time by the time it took to complete the last frame.
            yield return new WaitForEndOfFrame(); // Wait for end of frame before looping through again to ensure accurate time keeping.
        }
    }
}
