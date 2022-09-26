using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PositionManager : MonoBehaviour
{
    public GameObject positionImage;
    public GameObject outOfBoundsImage;
    public UnityEvent finishedPositioning;
    public float animationTime = 2f;

    private GameObject player;
    private GameObject[] allCharacters;
    private bool buttonClicked = false;
    private Vector3 mousePosition;
    private bool instantiated = false;
    private GameObject imageInstance;
    private bool doTracking = true;

    private void Start()
    {
        allCharacters = GameObject.FindGameObjectsWithTag("Participant");

        if (finishedPositioning == null)
        {
            finishedPositioning = new UnityEvent();
        }

        //confines the cursor to the bounds of the screen
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        if (buttonClicked)
        {
            //Gets the position of the mouse on the screen and converts it world space
            mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;

            player = FindObjectOfType<BattleMaster>().currentCharacter;

            //turns off the status popup when hovering over characters while positioning
            foreach (GameObject character in allCharacters)
            {
                character.GetComponent<MouseOver>().doMouseOver = false;
            }

            //if there is not a position image already created, make one then set the cursor to invisible
            if (!instantiated)
            {
                imageInstance = Instantiate(positionImage, mousePosition, Quaternion.identity);
                Cursor.visible = false;
                instantiated = true;
            }

            //moves the position image in accordance with the mouse position
            if (doTracking)
                imageInstance.transform.position = mousePosition;

            //after clicking a location, stops tracking the cursor and starts moving the last destination
            if (Input.GetMouseButtonDown(0))
            {
                if (imageInstance.GetComponent<BoundaryCheck>().canClick)
                {
                    doTracking = false;
                    player.transform.LeanMove(mousePosition, animationTime).setEaseInOutQuart().setOnComplete(FinishedMovement);
                    Destroy(imageInstance);
                }
            }
        }

    }

    public void StartTracking(int test)
    {
        Debug.Log("Test");
        buttonClicked = true;
    }

    void FinishedMovement()
    {
        finishedPositioning.Invoke();
    }

    public void OnFinish()
    {
        buttonClicked = false;
        doTracking = true;
        instantiated = false;
        Cursor.visible = true;

        //turns off the status popup when hovering over characters while positioning
        foreach (GameObject character in allCharacters)
        {
            character.GetComponent<MouseOver>().doMouseOver = true;
        }
    }
}
