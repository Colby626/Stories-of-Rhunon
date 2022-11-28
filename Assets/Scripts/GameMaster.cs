using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameMaster : MonoBehaviour
{
    public int distanceToLookForParticipants;
    public List<GameObject> participants;
    public Material highlightMaterial;

    public GameObject party;
    public GameObject battleMaster;
    public CustomGrid grid;
    public PathNode partyNode;
    public PathNode targetNode;
    public bool startPositionDetermined = false;
    public bool movedOnTurn = false;
    public bool hoveringOverButton;

    private Collider2D[] colliders;

    public static GameMaster instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else //On new scene load when there is already a GameMaster
        {
            instance.grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<CustomGrid>();
            instance.battleMaster = GameObject.FindGameObjectWithTag("BattleMaster");
            instance.party = GameObject.FindGameObjectWithTag("Party");
            instance.hoveringOverButton = false;
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);

        grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<CustomGrid>();
        instance.battleMaster = GameObject.FindGameObjectWithTag("BattleMaster");
        instance.party = GameObject.FindGameObjectWithTag("Party");
    }

    private void Update()
    {
        if (party != null)
        {
            party.GetComponent<Movement>().occupyingNode = partyNode;
        }
    }

    public void LookForParticipants(GameObject caller)
    {
        if (participants.Contains(caller))
        {
            return;
        }
        //check tiles outward from contact with enemy and player with a distance of distanceToLookForPartcipants and put them in participants list then call StartBattle
        colliders = Physics2D.OverlapBoxAll(caller.transform.position, new Vector2(distanceToLookForParticipants, distanceToLookForParticipants), 0);

        foreach (Collider2D collider in colliders) //This should be redone for optimization and just set in the editor 
        {
            if (collider.transform.gameObject.CompareTag("Participant"))
            {
                participants.Add(collider.gameObject);

                if (collider.GetComponent<CharacterSheet>().isPlayer)
                {
                    collider.gameObject.AddComponent<MouseOver>();
                    collider.GetComponent<MouseOver>().battleMaster = battleMaster.GetComponent<BattleMaster>();
                    collider.GetComponent<MouseOver>().overheadHealthBar = collider.transform.GetChild(0).GetChild(0).GetComponent<StatBar>();
                    collider.GetComponent<MouseOver>().healthBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(1).GetComponent<StatBar>();
                    collider.GetComponent<MouseOver>().manaBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(2).GetComponent<StatBar>();
                    collider.GetComponent<MouseOver>().staminaBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(3).GetComponent<StatBar>();
                    collider.GetComponent<MouseOver>().portrait = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>();
                    collider.GetComponent<MouseOver>().nameText = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(4).GetComponent<Text>();
                    collider.GetComponent<MouseOver>().overheadNameText = collider.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
                    collider.GetComponent<MouseOver>().highlightMaterial = highlightMaterial;
                }
                else
                {
                    LookForParticipants(collider.gameObject);
                    collider.GetComponentInParent<Movement>().lookingForParticipants = true;
                    collider.transform.parent.gameObject.AddComponent<MouseOver>();
                    collider.transform.parent.gameObject.GetComponent<MouseOver>().battleMaster = battleMaster.GetComponent<BattleMaster>();
                    collider.transform.parent.gameObject.GetComponent<MouseOver>().overheadHealthBar = collider.transform.GetChild(0).GetChild(0).GetComponent<StatBar>();
                    collider.transform.parent.gameObject.GetComponent<MouseOver>().healthBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(1).GetComponent<StatBar>();
                    collider.transform.parent.gameObject.GetComponent<MouseOver>().manaBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(2).GetComponent<StatBar>();
                    collider.transform.parent.gameObject.GetComponent<MouseOver>().staminaBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(3).GetComponent<StatBar>();
                    collider.transform.parent.gameObject.GetComponent<MouseOver>().portrait = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>();
                    collider.transform.parent.gameObject.GetComponent<MouseOver>().nameText = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(4).GetComponent<Text>();
                    collider.transform.parent.gameObject.GetComponent<MouseOver>().overheadNameText = collider.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
                    collider.transform.parent.gameObject.GetComponent<MouseOver>().highlightMaterial = highlightMaterial;
                }
                collider.gameObject.AddComponent<StatusManager>();
                collider.gameObject.GetComponent<StatusManager>().overheadHealthBar = collider.transform.GetChild(0).GetChild(0).GetComponent<StatBar>();
                collider.gameObject.GetComponent<StatusManager>().healthBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(1).GetComponent<StatBar>();
                collider.gameObject.GetComponent<StatusManager>().magicBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(2).GetComponent<StatBar>();
                collider.gameObject.GetComponent<StatusManager>().staminaBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(3).GetComponent<StatBar>();
            }
        }

        StartBattle();
    }

    public void StartBattle()
    {
        foreach (GameObject participant in participants)
        {
            if (participant.GetComponentInParent<Movement>().vectorPath.Count > 0)
            {
                Vector3 finishMove = participant.GetComponentInParent<Movement>().vectorPath[0];
                participant.GetComponentInParent<Movement>().vectorPath.Clear();
                participant.GetComponentInParent<Movement>().vectorPath.Add(finishMove);
            }
        }
        AudioManager.instance.Stop("ExploringMusic");
        AudioManager.instance.Play("BattleMusic");
        battleMaster.GetComponent<BattleMaster>().StartBattle(participants);
    }

    public void EndBattle()
    {
        //Set movement active again
        AudioManager.instance.Stop("BattleMusic");
        AudioManager.instance.Play("ExploringMusic");
        partyNode = null;
        startPositionDetermined = false;
        movedOnTurn = false;
        battleMaster.GetComponent<BattleMaster>().Reset();
        participants.Clear();
    }
}