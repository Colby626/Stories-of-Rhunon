using UnityEngine;

public class CursorOverlapCircle : MonoBehaviour
{
    [SerializeField]
    private float cursorRadius;

    private Collider2D[] colliders;
    private GameMaster gameMaster;
    private BattleMaster battleMaster;
    private PauseMenu pauseMenu;
    private CustomGrid grid;

    //Character variables
    private bool mouseExit = false;
    private bool characterFound = false;
    private bool characterHadBeenFound = false;
    private CharacterSheet character = null;
    private CharacterSheet oldCharacter = null;
    private MouseOver mouseOver = null;
    private MouseOver oldMouseOver = null;

    //Node variables
    private PathNode node = null;
    private PathNode previousNode = null;
    private bool nodeFound = false;
    private bool mouseExitNode = false;


    private void Start()
    {
        gameMaster = FindObjectOfType<GameMaster>().GetComponent<GameMaster>();
        battleMaster = FindObjectOfType<BattleMaster>().GetComponent<BattleMaster>();
        grid = FindObjectOfType<CustomGrid>().GetComponent<CustomGrid>();
        pauseMenu = FindObjectOfType<PauseMenu>();
    }

    private void Update()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //MouseOver on the characters stuff:
        if (battleMaster.battleStarted)
        {
            colliders = Physics2D.OverlapCircleAll(worldPosition, cursorRadius);

            for (int i = 0; i < colliders.Length; i++)
            {
                if (i == 0)
                {
                    characterFound = false;
                }
                if (colliders[i].GetComponent<CharacterSheet>())
                {
                    if (character != null && character != colliders[i].GetComponent<CharacterSheet>() && character != null) //Moved from one character to another
                    {
                        oldCharacter = character;
                        oldMouseOver = mouseOver;
                        mouseExit = true;
                    }
                    character = colliders[i].GetComponent<CharacterSheet>();
                    characterFound = true;
                    break;
                }
                if (i == colliders.Length - 1 && !characterFound)
                {
                    if (characterHadBeenFound && character != null) //Moved from a character to no character
                    {
                        oldCharacter = character;
                        oldMouseOver = mouseOver;
                        mouseExit = true;
                    }
                    character = null;
                }
            }

            //Thing to do when no longer hovering over a character that you were hovering over (OnMouseExit)
            if (mouseExit && !pauseMenu.gamePaused)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                mouseExit = false;
                characterHadBeenFound = false;
                if (oldCharacter.gameObject != battleMaster.currentCharacter && battleMaster.currentCharacter.GetComponent<CharacterSheet>().isPlayer)
                {
                    //Display status of current character
                    oldMouseOver.ActivateStatus(battleMaster.currentCharacter.GetComponent<CharacterSheet>());
                }
                oldCharacter.transform.GetChild(0).gameObject.SetActive(false);
                oldCharacter.GetComponent<SpriteRenderer>().material = oldMouseOver.characterMaterial;
                oldMouseOver.isHighlighted = false;
            }

            if (characterFound && !pauseMenu.gamePaused)
            {
                characterHadBeenFound = true;
                //Things to do when hovering over players (OnMouseOver)
                mouseOver = character.GetComponent<MouseOver>();
                if (!character.isDead && character.isPlayer)
                {
                    mouseOver.ActivateStatus(character);
                }

                if (!character.isPlayer && battleMaster.currentCharacter.isPlayer && !battleMaster.attackDone)
                {
                    if (!gameMaster.movedOnTurn) //Doing the raycast from the player's move speed
                    {
                        //The plus 2.5 is for the player being able to attack enemies 1 space away from the distance they can move to and the .5 is from the half of the node they are in
                        //Will be changed to circleCast when limitMovement works properly
                        RaycastHit2D[] boxCast = Physics2D.BoxCastAll(gameMaster.partyNode.transform.position, new Vector2(battleMaster.currentCharacter.characterStats.Speed/5 + 2.5f, battleMaster.currentCharacter.characterStats.Speed / 5 + 2.5f), 0, Vector2.zero);
                        foreach (RaycastHit2D hit in boxCast)
                        {
                            if (hit.transform.GetComponent<CharacterSheet>() == character)
                            {
                                Cursor.SetCursor(battleMaster.attackCursorTexture, Vector2.zero, CursorMode.Auto);
                            }
                        }
                    }
                    else //Doing the raycast from 1 space away from the player
                    {
                        //The 1.5 in size is for the half a node to get to the edge of the node you are at and 1 node further
                        //Will be changed to circleCast when limitMovement works properly
                        RaycastHit2D[] boxCast = Physics2D.BoxCastAll(gameMaster.partyNode.transform.position, new Vector2(1.5f, 1.5f), 0, Vector2.zero); 
                        foreach (RaycastHit2D hit in boxCast)
                        {
                            if (hit.transform.GetComponent<CharacterSheet>() == character)
                            {
                                Cursor.SetCursor(battleMaster.attackCursorTexture, Vector2.zero, CursorMode.Auto);
                            }
                        }
                    }
                }
                 

                if (!character.isPlayer && !character.isDead && !pauseMenu.gamePaused)
                {
                    character.transform.GetChild(0).gameObject.SetActive(true);
                    mouseOver.overheadHealthBar.SetBarMax(character.MaxHealth);
                    mouseOver.overheadHealthBar.SetBar(character.Health);
                    mouseOver.overheadNameText.text = character.Name;
                }

                if (!mouseOver.isHighlighted && !character.isDead && !pauseMenu.gamePaused)
                {
                    character.GetComponent<SpriteRenderer>().material = mouseOver.highlightMaterial;
                    mouseOver.isHighlighted = true;
                }

                if (character.isDead || pauseMenu.gamePaused)
                {
                    character.transform.GetChild(0).gameObject.SetActive(false);
                }

                if (!character.isDead && !pauseMenu.gamePaused && !character.isPlayer)
                {
                    character.transform.GetChild(0).gameObject.SetActive(true);
                }

                //Things to do when hovering over a character and the mouse is clicked (OnMouseDown)
                if (Input.GetMouseButtonUp(0) && battleMaster.currentCharacter.isPlayer) //Returns true on the frame the user releases the mouse button
                {
                    character.MouseDown();
                }
            }
        }

        //MouseOver on the nodes stuff:
        colliders = Physics2D.OverlapCircleAll(worldPosition, cursorRadius);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (i == 0)
            {
                nodeFound = false;
            }
            if (colliders[i].GetComponent<PathNode>())
            {
                if (node != null && node != colliders[i].GetComponent<PathNode>()) //If moving from one node to another
                {
                    previousNode = node;
                    mouseExitNode = true;
                }
                node = colliders[i].GetComponent<PathNode>();
                nodeFound = true;
                break;
            }
            if (i == colliders.Length - 1 && !nodeFound) //If not over a node
            {
                if (node != null)
                {
                    previousNode = node;
                    mouseExitNode = true;
                }
                node = null; 
            }
        }
        if (nodeFound)
        {
            if (!gameMaster.hoveringOverButton && !pauseMenu.gamePaused && !node.occupied)
            {
                node.transform.GetChild(0).gameObject.SetActive(true);
                if (grid.gridClicked) //Player movement
                {
                    if (!battleMaster.battleStarted)
                    {
                        gameMaster.targetNode = node;
                        node.destinationNode = true;
                    }
                    //Player movement in battle
                    if (battleMaster.battleStarted && node.validMovePosition && !gameMaster.movedOnTurn && battleMaster.currentCharacter.GetComponent<CharacterSheet>().isPlayer)
                    {
                        gameMaster.targetNode = node;
                        gameMaster.movedOnTurn = true;
                        gameMaster.movedOnTurnEvent.Invoke();
                    }
                    grid.gridClicked = false;
                }
            }
            else
            {
                node.transform.GetChild(0).gameObject.SetActive(false);
            }
        }

        //MouseExit for nodes
        if (mouseExitNode)
        {
            mouseExitNode = false;
            previousNode.transform.GetChild(0).gameObject.SetActive(false);
            previousNode = null;
        }
    }
}