using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplaySinglePlayer : MonoBehaviour
{
    [SerializeField]
    public bool customizeMapOverride = false;
    
    [SerializeField]
    public bool continentBonusOverride = false;

    [SerializeField]
    private MapDataHolder[] AllTerritories;

    [SerializeField]
    private MapDataHolder AttackingTerritory, SendingTerritory;

    [SerializeField]
    private MapDataHolder DefendingTerritory, ReceivingTerritory;

    [SerializeField]
    private int PlayerTurnHolder;

    [SerializeField]
    private PlayerDataHandlerForSingleplayer[] allPlayers;

    public int MainMenuIndex = 3;

    public GameObject YouWonPanel;
    
    public GameObject YouLostPanel;

    [Header("UI Elements For Player Data Having Turn \n")]
    public Image AvatarBG;
    public Image TroopsBG;
    public Text phaseText;
    public Text messageText;

    public Text TotalTroopsText;
    public Button nextPhaseButton;

    [Header("\n Ai Configuration Values \n")]
    public float AiResponseTime = 1f;


    [Header("\n Gameplay Starting Values \n")]
    public int InitialTroops = 30;
    public string phase = "";

    public int DefaultDiceForAi = 2;

    public bool isClaimActiveForPlayer = false;
    public bool canPlayerDeploy = false;
    public bool canPlayerAttack = false;
    public bool isNeighboursActive = false;

    //public GameObject DeployPanel;

    [Header("\n Dice Elements \n")]
    public Text DiceText;
    public GameObject[] allDicesParentAttacker;
    public GameObject[] allDicesParentDefender;
    public DiceDataHandler[] AttackerDices;
    public DiceDataHandler[] DefenderDices;
    public GameObject AttackPanel;


    [Header("\n Card Data Holder")]
    public CardDataHolder[] ShowCaseCardDecks;

    public CardDataHolder[] SelectionCardDecks;

    public TextMeshProUGUI TotalCardsText;

    private void Start()
    {
        if(customizeMapOverride)
        {
            int val = 0;

            val = PlayerPrefs.GetInt("IsMapCustomized");

            if(val == 1)
            {
                SetTerritories();
            }
        }
        if (PlayerPrefs.GetInt("CanResumeGame") != 1)
        {
            string tempColor;
            Color OutputColor;

            for (int i = 0; i < AllTerritories.Length; i++)
            {
                AllTerritories[i].AttackTerritoryImage.SetActive(false);
            }

            int totalplayers = PlayerPrefs.GetInt("allplayers");

            for (int i = 0; i < totalplayers; i++)
            {
                allPlayers[i].parent.SetActive(true);

                allPlayers[i].player_id = PlayerPrefs.GetInt("idSP" + i);
                allPlayers[i].name.text = PlayerPrefs.GetString("usernameSP" + i);
                allPlayers[i].color = PlayerPrefs.GetString("colorSP" + i);
                allPlayers[i].AiDifficultyValue = PlayerPrefs.GetString("aiSP" + i);

                allPlayers[i].TroopsInPossesion = InitialTroops;

                tempColor = allPlayers[i].color;

                ColorUtility.TryParseHtmlString("#" + tempColor, out OutputColor);

                allPlayers[i].bg.color = OutputColor;
            }

            ChangePlayerTurn();

            tempColor = allPlayers[PlayerTurnHolder-1].color;

            ColorUtility.TryParseHtmlString("#" + tempColor, out OutputColor);

            AvatarBG.color = OutputColor;
            TroopsBG.color = OutputColor;

            phase = "claim";

            StartCoroutine(SynchPlayerDataHavingTurn());
            StartCoroutine(AiSlot2());
            StartCoroutine(AiSlot3());
            StartCoroutine(AiSlot4());
            StartCoroutine(AiSlot5());
            StartCoroutine(AiSlot6());
        }
    }

    public void SetTerritories()
    {
        for (int i = 0; i < AllTerritories.Length; i++)
        {
            int state = PlayerPrefs.GetInt("Tstate" + i);

            if (state == 1)
            {
                AllTerritories[i].Territory_image.color = Color.white;
                AllTerritories[i].Territory_button.interactable = true;

                AllTerritories[i].TerritoryActiveState = true;
            }

            else
            {
                AllTerritories[i].Territory_button.interactable = false;

                AllTerritories[i].TerritoryActiveState = false;

                string tempColor = "044954";

                Color OutputColor;
                ColorUtility.TryParseHtmlString("#" + tempColor, out OutputColor);

                AllTerritories[i].Territory_image.color = OutputColor;
            }
        }
    }

    public void goBack()
    {
        SceneManager.LoadScene(MainMenuIndex);
    }

    public bool IsClaimPhaseStillOn()
    {
        if (phase == "claim")
        {
            int totalplayers = PlayerPrefs.GetInt("allplayers");

            for (int i = 0; i < totalplayers; i++)
            {
                if (allPlayers[i].TroopsInPossesion != 0)
                    return true;
            }

            phase = "deploy";
        }
        return false;
    }

    public void ClaimTerritory(int TerritoryId)
    {
        if (!isClaimActiveForPlayer)
        {
            Debug.Log("Not Your Turn");
            return;
        }

        if (IsClaimPhaseStillOn())
        {
            for (int i = 0; i < AllTerritories.Length; i++)
            {
                if (AllTerritories[i].Territory_id == TerritoryId)
                {
                    if (AllTerritories[i].CapturedBy == 0)
                    {
                        AllTerritories[i].CapturedBy = PlayerTurnHolder;

                        string tempColor = allPlayers[PlayerTurnHolder-1].color;

                        Color OutputColor;
                        ColorUtility.TryParseHtmlString("#" + tempColor, out OutputColor);

                        AllTerritories[i].Territory_image.color = OutputColor;

                        AllTerritories[i].totalTroopsText.text = 1 + "";
                        AllTerritories[i].totalTroopsText.gameObject.SetActive(true);

                        allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                        ChangePlayerTurn();
                    }
                    else if (AllTerritories[i].CapturedBy == PlayerTurnHolder)
                    {
                        List<int> uncapturedTerritories = UnCapturedTerritories();

                        if(uncapturedTerritories.Count < 1)
                        {
                            int troops = int.Parse(AllTerritories[i].totalTroopsText.text);

                            troops++;

                            AllTerritories[i].totalTroopsText.text = troops + "";

                            allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                            ChangePlayerTurn();
                        }
                    }
                }
            }
        }

    }

    public void ClaimTerritoryForAi(int TerritoryId)
    {
        if (IsClaimPhaseStillOn())
        {
            for (int i = 0; i < AllTerritories.Length; i++)
            {
                if (AllTerritories[i].Territory_id == TerritoryId)
                {
                    if (AllTerritories[i].CapturedBy == 0)
                    {
                        AllTerritories[i].CapturedBy = PlayerTurnHolder;

                        string tempColor = allPlayers[PlayerTurnHolder - 1].color;

                        Color OutputColor;
                        ColorUtility.TryParseHtmlString("#" + tempColor, out OutputColor);

                        AllTerritories[i].Territory_image.color = OutputColor;

                        AllTerritories[i].totalTroopsText.text = 1 + "";
                        AllTerritories[i].totalTroopsText.gameObject.SetActive(true);

                    }
                }
            }
        }
    }

    public void SetDeployTroops()
    {
        int AwardedTroops = TroopsAwardedForPlayer(PlayerTurnHolder);

        Debug.Log("Awarded Troops Are : " + AwardedTroops);

        allPlayers[PlayerTurnHolder - 1].TroopsInPossesion = AwardedTroops;
    }

    public void DeployTroops(int TerritoryId)
    {
        if (!phase.Equals("deploy"))
            return;

        if (!canPlayerDeploy)
        {
            return;
        }

        if (AllTerritories[TerritoryId - 1].CapturedBy != PlayerTurnHolder)
        {
            return;
        }

        if (allPlayers[PlayerTurnHolder - 1].TroopsInPossesion > 0)
        {
            int troops = int.Parse(AllTerritories[TerritoryId - 1].totalTroopsText.text);

            troops++;
            allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

            AllTerritories[TerritoryId - 1].totalTroopsText.text = troops + "";

            if (allPlayers[PlayerTurnHolder - 1].TroopsInPossesion <= 0)
            {
                allPlayers[PlayerTurnHolder - 1].TroopsInPossesion = 0;
                ChangePhase();
                return;
            }
        }
        else
            ChangePhase();
    }

    public int TroopsAwardedForPlayer(int playerID)
    {
        int AwardedTroops = 3;

        if (continentBonusOverride)
        {
            // Continent Check Code
            bool isNACapturedCompletely = true;
            bool isSACapturedCompletely = true;
            bool isEUCapturedCompletely = true;
            bool isAFCapturedCompletely = true;
            bool isASCapturedCompletely = true;
            bool isAUCapturedCompletely = true;

            // Continent North America
            for (int i = 0; i < 9; i++)
            {
                if (AllTerritories[i].CapturedBy != playerID)
                {
                    isNACapturedCompletely = false;
                    break;
                }
            }

            // Continent South America
            for (int i = 9; i < 13; i++)
            {
                if (AllTerritories[i].CapturedBy != playerID)
                {
                    isSACapturedCompletely = false;
                    break;
                }
            }

            // Continent Europe
            for (int i = 13; i < 20; i++)
            {
                if (AllTerritories[i].CapturedBy != playerID)
                {
                    isEUCapturedCompletely = false;
                    break;
                }
            }

            // Continent Africa
            for (int i = 20; i < 26; i++)
            {
                if (AllTerritories[i].CapturedBy != playerID)
                {
                    isAFCapturedCompletely = false;
                    break;
                }
            }

            // Continent Asia
            for (int i = 26; i < 38; i++)
            {
                if (AllTerritories[i].CapturedBy != playerID)
                {
                    isASCapturedCompletely = false;
                    break;
                }
            }

            for (int i = 38; i < 42; i++)
            {
                if (AllTerritories[i].CapturedBy != playerID)
                {
                    isAUCapturedCompletely = false;
                    break;
                }
            }

            if (isNACapturedCompletely)
                AwardedTroops += 5;

            if (isSACapturedCompletely)
                AwardedTroops += 2;

            if (isEUCapturedCompletely)
                AwardedTroops += 5;

            if (isAFCapturedCompletely)
                AwardedTroops += 3;

            if (isASCapturedCompletely)
                AwardedTroops += 7;

            if (isAUCapturedCompletely)
                AwardedTroops += 2;

        }

        return AwardedTroops;

    }

    public void ChangePlayerTurn()
    {
        ResestAttackImageOnTerritories();

        if (phase.Equals("claim") && !IsClaimPhaseStillOn()) 
            phase = "deploy";

        if (PlayerTurnHolder == 0)
        {
            PlayerTurnHolder = allPlayers[0].player_id;

            if (phase.Equals("claim"))
            {
                isClaimActiveForPlayer = true;

                nextPhaseButton.gameObject.SetActive(false);
            }
            else if (phase.Equals("deploy") || phase.Equals("attack") || phase.Equals("fortify"))
            {
                phase = "deploy";

                nextPhaseButton.gameObject.SetActive(true);

                canPlayerDeploy = true;
                SetDeployTroops();

            }

            return;
        }

        int totalplayers = PlayerPrefs.GetInt("allplayers");

        for (int i = 0; i < totalplayers; i++)
        {
            if(allPlayers[i].player_id == PlayerTurnHolder)
            {
                if ((i + 1) < totalplayers) 
                {
                    PlayerTurnHolder = allPlayers[i + 1].player_id;
                    
                    isClaimActiveForPlayer = false;
                    canPlayerDeploy = false;

                    if (phase.Equals("deploy") || phase.Equals("attack") || phase.Equals("fortify"))
                    {
                        phase = "deploy";
                        SetDeployTroops();
                    }

                    return;
                }
                else
                {
                    PlayerTurnHolder = allPlayers[0].player_id;

                    if (phase.Equals("claim"))
                    {
                        isClaimActiveForPlayer = true;

                        nextPhaseButton.gameObject.SetActive(false);
                    }
                    else if (phase.Equals("deploy") || phase.Equals("attack") || phase.Equals("fortify"))
                    {

                        nextPhaseButton.gameObject.SetActive(true);

                        phase = "deploy";

                        canPlayerDeploy = true;
                        SetDeployTroops();
                    }

                    return;
                }
            }
        }
    }

    public void ChangePhase()
    {
        Debug.Log("Changing Phase");

        ResestAttackImageOnTerritories();

        if(phase.Equals("deploy"))
        {
            isClaimActiveForPlayer = false;

            canPlayerDeploy = false;

            canPlayerAttack = true;

            phase = "attack";
        }
        else if(phase.Equals("attack"))
        {
            canPlayerDeploy = false;

            canPlayerAttack = false;

            phase = "fortify";
        }
        else if(phase.Equals("fortify"))
        {
            phase = "deploy";

            ChangePlayerTurn();
        }
    }

    public void ChangeDice(string direction)
    {
        string dice = DiceText.text;

        if (direction.Equals("right"))
        {
            if (dice.Equals("1"))
            {
                DiceText.text = 2 + "";

                for (int i = 0; i < allDicesParentAttacker.Length; i++)
                    allDicesParentAttacker[i].SetActive(false);

                allDicesParentAttacker[0].SetActive(true);
                allDicesParentAttacker[1].SetActive(true);

            }
            else if (dice.Equals("2"))
            {

                DiceText.text = 3 + "";

                for (int i = 0; i < allDicesParentAttacker.Length; i++)
                    allDicesParentAttacker[i].SetActive(true);

                allDicesParentAttacker[0].SetActive(true);
                allDicesParentAttacker[1].SetActive(true);
                allDicesParentAttacker[2].SetActive(true);
            }
            else if (dice.Equals("3"))
            {

                DiceText.text = 1 + "";

                for (int i = 0; i < allDicesParentAttacker.Length; i++)
                    allDicesParentAttacker[i].SetActive(false);

                allDicesParentAttacker[0].SetActive(true);
            }
        }
        else if (direction.Equals("left"))
        {
            if (dice.Equals("1"))
            {
                DiceText.text = 3 + "";

                for (int i = 0; i < allDicesParentAttacker.Length; i++)
                    allDicesParentAttacker[i].SetActive(true);

            }
            else if (dice.Equals("2"))
            {
                DiceText.text = 1 + "";

                for (int i = 0; i < allDicesParentAttacker.Length; i++)
                    allDicesParentAttacker[i].SetActive(false);

                allDicesParentAttacker[0].SetActive(true);
            }
            else if (dice.Equals("3"))
            {
                DiceText.text = 2 + "";

                for (int i = 0; i < allDicesParentAttacker.Length; i++)
                    allDicesParentAttacker[i].SetActive(false);

                allDicesParentAttacker[0].SetActive(true);
                allDicesParentAttacker[1].SetActive(true);
            }
        }
    }

    public void ActivateNeighboursForAttack(int territoryId)
    {
        if (!phase.Equals("attack"))
        {
            return;
        }

        Debug.Log("Attack Layer 0");

        // Attacker
        if (!AllTerritories[territoryId - 1].AttackTerritoryImage.activeSelf)
        {
            isNeighboursActive = false;

            for (int i = 0; i < AllTerritories.Length; i++)
                AllTerritories[i].AttackTerritoryImage.SetActive(false);

            if (AllTerritories[territoryId - 1].CapturedBy != PlayerTurnHolder)
                return;

            if((int.Parse(AllTerritories[territoryId - 1].totalTroopsText.text)) <= 1)
                return;
            

            for (int i = 0; i < AllTerritories[territoryId - 1].Neigbhors.Length; i++)
            {
                for (int j = 0; j < AllTerritories.Length; j++)
                {
                    if (AllTerritories[territoryId - 1].Neigbhors[i].name.Equals(AllTerritories[j].Territory_name))
                    {
                        Debug.Log("Attack Layer 2");

                        if (AllTerritories[j].CapturedBy != PlayerTurnHolder && AllTerritories[j].TerritoryActiveState)
                        {
                            Debug.Log("Attack Layer 3");

                            AttackingTerritory = AllTerritories[territoryId - 1];

                            AllTerritories[j].AttackTerritoryImage.SetActive(true);

                            isNeighboursActive = true;
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            DefendingTerritory = AllTerritories[territoryId - 1];

            AttackPanel.SetActive(true);
        }
    }
    
    public void ActivateNeighboursForFortify(int territoryId)
    {
        Debug.Log("Territory Id Fortify is " + territoryId + " With Active State = " + AllTerritories[territoryId - 1].AttackTerritoryImage.activeInHierarchy + " Having Name " + AllTerritories[territoryId - 1].Territory_name);

        if (!phase.Equals("fortify"))
            return;

        if (!AllTerritories[territoryId - 1].AttackTerritoryImage.activeInHierarchy) 
        {
            isNeighboursActive = false;

            for (int i = 0; i < AllTerritories.Length; i++)
                AllTerritories[i].AttackTerritoryImage.SetActive(false);

            for (int i = 0; i < AllTerritories[territoryId - 1].Neigbhors.Length; i++)
            {
                for (int j = 0; j < AllTerritories.Length; j++)
                {
                    if (AllTerritories[territoryId - 1].Neigbhors[i].name.Equals(AllTerritories[j].Territory_name))
                    {
                        if (AllTerritories[j].CapturedBy == PlayerTurnHolder && AllTerritories[j].TerritoryActiveState)
                        {
                            Debug.Log("Sending End");

                            SendingTerritory = AllTerritories[territoryId - 1];

                            AllTerritories[j].AttackTerritoryImage.SetActive(true);

                            isNeighboursActive = true;
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            Debug.Log("Recieving End");

            ReceivingTerritory = AllTerritories[territoryId - 1];

            FortifyTroops();
        }
    }

    public void FortifyTroops()
    {
        int troopsToSend = int.Parse(SendingTerritory.totalTroopsText.text);

        SendingTerritory.totalTroopsText.text = ReceivingTerritory.totalTroopsText.text;

        ReceivingTerritory.totalTroopsText.text = troopsToSend + "";

        if(PlayerTurnHolder == allPlayers[0].player_id)
            ChangePlayerTurn();

        ResestAttackImageOnTerritories();
    }

    public int a, a2, a3, d1, d2, Amax, Amin, Asmax, Asmin, Dmax,Dsmax, Dmin;
    
    public void Attack()
    {
        if (!phase.Equals("attack"))
            return;
        string dice = "";

        if (PlayerTurnHolder == allPlayers[0].player_id)
            dice = DiceText.text;
        else
            dice = DefaultDiceForAi+"";

        int AttackTroop, DefendTroop;


        AttackTroop = int.Parse(AttackingTerritory.totalTroopsText.text);

        if(AttackTroop < 2)
        {
            AttackPanel.SetActive(false);
            return;
        }    

        switch (dice)
        {
            case "1":
                a = Random.Range(1, 7);

                d1 = Random.Range(1, 7);
                d2 = Random.Range(1, 7);

                AttackerDices[0].parent.SetActive(true);

                for (int i = 0; i < 6; i++)
                {
                    if (AttackerDices[0].allDices[i].name.Equals(a + ""))
                    {
                        AttackerDices[0].allDices[i].SetActive(true);
                    }
                    else
                        AttackerDices[0].allDices[i].SetActive(false);

                }

                DefenderDices[0].parent.SetActive(true);

                for (int i = 0; i < 6; i++)
                {
                    if (DefenderDices[0].allDices[i].name.Equals(d1 + ""))
                    {
                        DefenderDices[0].allDices[i].SetActive(true);
                    }
                    else
                        DefenderDices[0].allDices[i].SetActive(false);

                }

                DefenderDices[1].parent.SetActive(true);

                for (int i = 0; i < 6; i++)
                {
                    if (DefenderDices[1].allDices[i].name.Equals(d2 + ""))
                    {
                        DefenderDices[1].allDices[i].SetActive(true);
                    }
                    else
                        DefenderDices[1].allDices[i].SetActive(false);

                }

                // Updating Troops

                if (a > d1)
                {
                    if (a > d2)
                    {
                        DefendingTerritory.totalTroopsText.text = (int.Parse(DefendingTerritory.totalTroopsText.text) - 1) + "";

                        DefendTroop = int.Parse(DefendingTerritory.totalTroopsText.text);

                        AttackTroop = int.Parse(AttackingTerritory.totalTroopsText.text);

                        if (DefendTroop < 1)
                        {
                            // Capturing Territory
                            DefendingTerritory.CapturedBy = PlayerTurnHolder;

                            string tempColor = allPlayers[PlayerTurnHolder - 1].color;

                            Color OutputColor;
                            ColorUtility.TryParseHtmlString("#" + tempColor, out OutputColor);

                            DefendingTerritory.Territory_image.color = OutputColor;

                            DefendingTerritory.totalTroopsText.text = AttackTroop + "";
                            AttackingTerritory.totalTroopsText.text = 1 + "";

                            AttackPanel.SetActive(false);

                            for (int i = 0; i < AllTerritories.Length; i++)
                                AllTerritories[i].AttackTerritoryImage.SetActive(false);



                        }

                    }
                    else
                    {
                        AttackingTerritory.totalTroopsText.text = (int.Parse(AttackingTerritory.totalTroopsText.text) - 1) + "";

                        AttackTroop = int.Parse(AttackingTerritory.totalTroopsText.text);

                        if(AttackTroop < 1)

                            AttackingTerritory.totalTroopsText.text = 1 + "";
                    }
                }
                else
                {
                    AttackingTerritory.totalTroopsText.text = (int.Parse(AttackingTerritory.totalTroopsText.text) - 1) + "";

                    AttackTroop = int.Parse(AttackingTerritory.totalTroopsText.text);

                    if (AttackTroop < 1)

                        AttackingTerritory.totalTroopsText.text = 1 + "";
                }

                for (int i = 0; i < AllTerritories.Length; i++)
                    AllTerritories[i].AttackTerritoryImage.SetActive(false);

                break;

            case "2":

                a = Random.Range(1, 7);
                a2 = Random.Range(1, 7);

                d1 = Random.Range(1, 7);
                d2 = Random.Range(1, 7);

                AttackerDices[0].parent.SetActive(true);

                for (int i = 0; i < 6; i++)
                {
                    if (AttackerDices[0].allDices[i].name.Equals(a + ""))
                    {
                        AttackerDices[0].allDices[i].SetActive(true);
                    }
                    else
                        AttackerDices[0].allDices[i].SetActive(false);

                }

                AttackerDices[1].parent.SetActive(true);

                for (int i = 0; i < 6; i++)
                {
                    if (AttackerDices[1].allDices[i].name.Equals(a2 + ""))
                    {
                        AttackerDices[1].allDices[i].SetActive(true);
                    }
                    else
                        AttackerDices[1].allDices[i].SetActive(false);

                }

                DefenderDices[0].parent.SetActive(true);

                for (int i = 0; i < 6; i++)
                {
                    if (DefenderDices[0].allDices[i].name.Equals(d1 + ""))
                    {
                        DefenderDices[0].allDices[i].SetActive(true);
                    }
                    else
                        DefenderDices[0].allDices[i].SetActive(false);

                }

                DefenderDices[1].parent.SetActive(true);

                for (int i = 0; i < 6; i++)
                {
                    if (DefenderDices[1].allDices[i].name.Equals(d2 + ""))
                    {
                        DefenderDices[1].allDices[i].SetActive(true);
                    }
                    else
                        DefenderDices[1].allDices[i].SetActive(false);

                }

                // Updating Troops

                if (a > a2)
                {
                    Amax = a;
                    Amin = a2;
                }
                else
                {
                    Amax = a2;
                    Amin = a;
                }

                if (d1 > d2)
                {
                    Dmax = d1;
                    Dmin = d2;
                }
                else
                {
                    Dmax = d2;
                    Dmin = d1;
                }

                // For Max Dice
                if (Amax > Dmax)
                {
                    DefendingTerritory.totalTroopsText.text = (int.Parse(DefendingTerritory.totalTroopsText.text) - 1) + "";
                }
                else
                {
                    AttackingTerritory.totalTroopsText.text = (int.Parse(AttackingTerritory.totalTroopsText.text) - 1) + "";
                }

                // For Min Dice
                if (Amin > Dmin)
                {
                    DefendingTerritory.totalTroopsText.text = (int.Parse(DefendingTerritory.totalTroopsText.text) - 1) + "";
                }
                else
                {
                    AttackingTerritory.totalTroopsText.text = (int.Parse(AttackingTerritory.totalTroopsText.text) - 1) + "";
                }


                DefendTroop = int.Parse(DefendingTerritory.totalTroopsText.text);
                AttackTroop = int.Parse(AttackingTerritory.totalTroopsText.text);

                if (AttackTroop < 1)
                {
                    AttackingTerritory.totalTroopsText.text = 1 + "";
                }

                if (DefendTroop < 1)
                {
                    // Capturing Territory
                    DefendingTerritory.CapturedBy = PlayerTurnHolder;

                    string tempColor = allPlayers[PlayerTurnHolder - 1].color;

                    Color OutputColor;
                    ColorUtility.TryParseHtmlString("#" + tempColor, out OutputColor);

                    DefendingTerritory.Territory_image.color = OutputColor;

                    DefendingTerritory.totalTroopsText.text = AttackTroop + "";
                    AttackingTerritory.totalTroopsText.text = 1 + "";

                    AttackPanel.SetActive(false);

                    for (int i = 0; i < AllTerritories.Length; i++)
                        AllTerritories[i].AttackTerritoryImage.SetActive(false);
                }

                // Resetting Neigbhorss
                for (int i = 0; i < AllTerritories.Length; i++)
                    AllTerritories[i].AttackTerritoryImage.SetActive(false);

                break;

            case "3":

                a = Random.Range(1, 7);
                a2 = Random.Range(1, 7);
                a3 = Random.Range(1, 7);

                d1 = Random.Range(1, 7);
                d2 = Random.Range(1, 7);

                AttackerDices[0].parent.SetActive(true);

                for (int i = 0; i < 6; i++)
                {
                    if (AttackerDices[0].allDices[i].name.Equals(a + ""))
                    {
                        AttackerDices[0].allDices[i].SetActive(true);
                    }
                    else
                        AttackerDices[0].allDices[i].SetActive(false);

                }

                AttackerDices[1].parent.SetActive(true);

                for (int i = 0; i < 6; i++)
                {
                    if (AttackerDices[1].allDices[i].name.Equals(a2 + ""))
                    {
                        AttackerDices[1].allDices[i].SetActive(true);
                    }
                    else
                        AttackerDices[1].allDices[i].SetActive(false);

                }

                AttackerDices[1].parent.SetActive(true);

                for (int i = 0; i < 6; i++)
                {
                    if (AttackerDices[1].allDices[i].name.Equals(a2 + ""))
                    {
                        AttackerDices[1].allDices[i].SetActive(true);
                    }
                    else
                        AttackerDices[1].allDices[i].SetActive(false);

                }

                AttackerDices[2].parent.SetActive(true);

                for (int i = 0; i < 6; i++)
                {
                    if (AttackerDices[2].allDices[i].name.Equals(a3 + ""))
                    {
                        AttackerDices[2].allDices[i].SetActive(true);
                    }
                    else
                        AttackerDices[2].allDices[i].SetActive(false);

                }

                DefenderDices[0].parent.SetActive(true);

                for (int i = 0; i < 6; i++)
                {
                    if (DefenderDices[0].allDices[i].name.Equals(d1 + ""))
                    {
                        DefenderDices[0].allDices[i].SetActive(true);
                    }
                    else
                        DefenderDices[0].allDices[i].SetActive(false);

                }

                DefenderDices[1].parent.SetActive(true);

                for (int i = 0; i < 6; i++)
                {
                    if (DefenderDices[1].allDices[i].name.Equals(d2 + ""))
                    {
                        DefenderDices[1].allDices[i].SetActive(true);
                    }
                    else
                        DefenderDices[1].allDices[i].SetActive(false);

                }


                // Finding Max

                if (a > a2 && a > a3)
                {
                    Amax = a;
                }
                else if (a2 > a && a2 > a3)
                {
                    Amax = a2;
                }
                else
                {
                    Amax = a3;
                }

                // Finding S Max

                if ((a < Amax && a > a2) || (a < Amax && a > a3))
                {
                    Asmax = a;
                }
                else if ((a3 < Amax && a3 > a) || (a3 < Amax && a3 > a2))
                {
                    Asmax = a3;
                }
                else
                {
                    Asmax = a2;
                }


                // Finding Max & S Max (Defender)
                if (d1 > d2)
                {
                    Dmax = d1;
                    Dsmax = d2;
                }
                else
                {
                    Dmax = d2;
                    Dsmax = d1;
                }


                // For Max Dice
                if (Amax > Dmax)
                {
                    DefendingTerritory.totalTroopsText.text = (int.Parse(DefendingTerritory.totalTroopsText.text) - 1) + "";
                }
                else
                {
                    AttackingTerritory.totalTroopsText.text = (int.Parse(AttackingTerritory.totalTroopsText.text) - 1) + "";
                }

                // For S Max Dice
                if (Asmax > Dsmax)
                {
                    DefendingTerritory.totalTroopsText.text = (int.Parse(DefendingTerritory.totalTroopsText.text) - 1) + "";
                }
                else
                {
                    AttackingTerritory.totalTroopsText.text = (int.Parse(AttackingTerritory.totalTroopsText.text) - 1) + "";
                }


                DefendTroop = int.Parse(DefendingTerritory.totalTroopsText.text);
                AttackTroop = int.Parse(AttackingTerritory.totalTroopsText.text);

                if (AttackTroop < 1)
                {
                    AttackingTerritory.totalTroopsText.text = 1 + "";
                }

                if (DefendTroop < 1)
                {
                    // Capturing Territory
                    DefendingTerritory.CapturedBy = PlayerTurnHolder;

                    string tempColor = allPlayers[PlayerTurnHolder - 1].color;

                    Color OutputColor;
                    ColorUtility.TryParseHtmlString("#" + tempColor, out OutputColor);

                    DefendingTerritory.Territory_image.color = OutputColor;

                    DefendingTerritory.totalTroopsText.text = AttackTroop + "";
                    AttackingTerritory.totalTroopsText.text = 1 + "";


                    AttackPanel.SetActive(false);

                    for (int i = 0; i < AllTerritories.Length; i++)
                        AllTerritories[i].AttackTerritoryImage.SetActive(false);

                }

                // Resetting Neigbhorss
                for (int i = 0; i < AllTerritories.Length; i++)
                    AllTerritories[i].AttackTerritoryImage.SetActive(false);

                break;
        }

    }

    public void ResestAttackImageOnTerritories()
    {
        for (int i = 0; i < AllTerritories.Length; i++)
            AllTerritories[i].AttackTerritoryImage.SetActive(false);
    }

    IEnumerator AggressiveAttackStrategy()
    {
        yield return null;

        // Attack Logic

        Debug.Log("Activating Aggressive Attack Strategy");

        List<int> BorderTerritoriesIDs = BorderTerritories(PlayerTurnHolder);

        int CapturedBorderTerritories = -1;

        int max = 0;
        int cnt = 0;
        int tempId = 1;

        if (BorderTerritoriesIDs.Count > 0)
        {
            foreach (int id in BorderTerritoriesIDs)
            {
                if (max <= (int.Parse(AllTerritories[id - 1].totalTroopsText.text)))
                {
                    max = int.Parse(AllTerritories[id - 1].totalTroopsText.text);

                    CapturedBorderTerritories = id;
                }
            }

            ActivateNeighboursForAttack(CapturedBorderTerritories);

            List<int> ActivatedCountryIDs = new List<int>();

            for (int i = 0; i < AllTerritories.Length; i++)
                if (AllTerritories[i].AttackTerritoryImage.activeSelf)
                    ActivatedCountryIDs.Add(AllTerritories[i].Territory_id);


            int r = Random.Range(0, ActivatedCountryIDs.Count);
            cnt = 0;
            tempId = 1;

            foreach (int id in ActivatedCountryIDs)
            {
                if (cnt == r)
                {
                    tempId = id;
                    break;
                }

                cnt++;
                tempId = id;
            }

            AttackingTerritory = AllTerritories[CapturedBorderTerritories - 1];

            DefendingTerritory = AllTerritories[tempId - 1];

            Debug.Log("Attacking Territory = " + AttackingTerritory.Territory_name);
            Debug.Log("Defending Territory = " + DefendingTerritory.Territory_name);

            while (DefendingTerritory.CapturedBy != PlayerTurnHolder)
            {
                yield return new WaitForSeconds(AiResponseTime);

                Debug.Log("Attack Called From AS");

                Attack();

                if (DefendingTerritory.CapturedBy != PlayerTurnHolder)
                {
                    if ((int.Parse(AttackingTerritory.totalTroopsText.text)) <= 1)
                    {
                        BorderTerritoriesIDs = BorderTerritories(PlayerTurnHolder);

                        CapturedBorderTerritories = -1;

                        max = 0;

                        foreach (int id in BorderTerritoriesIDs)
                        {
                            if (max <= (int.Parse(AllTerritories[id - 1].totalTroopsText.text)))
                            {
                                max = int.Parse(AllTerritories[id - 1].totalTroopsText.text);

                                CapturedBorderTerritories = id;
                            }
                        }

                        if (int.Parse(AllTerritories[CapturedBorderTerritories - 1].totalTroopsText.text) < 2)
                        {
                            break;
                        }
                        else
                        {
                            ActivateNeighboursForAttack(CapturedBorderTerritories);

                            yield return new WaitForSeconds(AiResponseTime * 2f);

                            ActivatedCountryIDs = new List<int>();

                            for (int i = 0; i < AllTerritories.Length; i++)
                                if (AllTerritories[i].AttackTerritoryImage.activeSelf)
                                    ActivatedCountryIDs.Add(AllTerritories[i].Territory_id);


                            r = Random.Range(0, ActivatedCountryIDs.Count);
                            cnt = 0;
                            tempId = 1;

                            foreach (int id in ActivatedCountryIDs)
                            {
                                if (cnt == r)
                                {
                                    tempId = id;
                                    break;
                                }

                                cnt++;
                                tempId = id;
                            }

                            AttackingTerritory = AllTerritories[CapturedBorderTerritories - 1];

                            DefendingTerritory = AllTerritories[tempId - 1];
                        }
                    }
                }
                else
                    break;
            }
        }
        
        ChangePhase();

        // Fortify Logic
        {
            ResestAttackImageOnTerritories();

            List<int> OccupiedTerritories = CapturedTerritories(PlayerTurnHolder);

            max = 0;
            cnt = 0;
            tempId = 1;

            CapturedBorderTerritories = 0;

            foreach (int id in OccupiedTerritories)
            {
                if (max <= (int.Parse(AllTerritories[id - 1].totalTroopsText.text)))
                {
                    max = int.Parse(AllTerritories[id - 1].totalTroopsText.text);

                    CapturedBorderTerritories = id;
                }

                cnt++;

                tempId = id;
            }

            if (max > 1)
            {
                yield return new WaitForSeconds(AiResponseTime);

                ActivateNeighboursForFortify(CapturedBorderTerritories);

                List<int> ActivatedCountryIDs = new List<int>();

                for (int i = 0; i < AllTerritories.Length; i++)
                    if (AllTerritories[i].AttackTerritoryImage.activeSelf)
                        ActivatedCountryIDs.Add(AllTerritories[i].Territory_id);

                int r = Random.Range(0, ActivatedCountryIDs.Count);
                cnt = 0;
                tempId = 1;

                foreach (int id in ActivatedCountryIDs)
                {
                    if (cnt == r)
                    {
                        tempId = id;
                        break;
                    }

                    cnt++;
                    tempId = id;
                }

                SendingTerritory = AllTerritories[CapturedBorderTerritories - 1];

                ReceivingTerritory = AllTerritories[tempId - 1];

                FortifyTroops();
            }

            yield return new WaitForSeconds(AiResponseTime );
            
        }

        ChangePlayerTurn();
    }

    int count = 0;  

    int countTroops = 0;

    IEnumerator SynchPlayerDataHavingTurn()
    {
        while (true)
        {
            string tempColor = allPlayers[PlayerTurnHolder - 1].color;

            Color OutputColor;
            ColorUtility.TryParseHtmlString("#" + tempColor, out OutputColor);

            AvatarBG.color = OutputColor;
            TroopsBG.color = OutputColor;

            phaseText.text = phase + "";

            switch (phase)
            {
                case "claim":
                    messageText.text = "Place Your Troops";
                    break;

                case "deploy":
                    messageText.text = "Deploy Your Troops";
                    break;

                case "attack":
                    messageText.text = "Attack Territories";
                    break;

                case "fortify":
                    messageText.text = "Fortify Territories";
                    break;
            }


            TotalTroopsText.text = allPlayers[PlayerTurnHolder - 1].TroopsInPossesion + "";

            List<int> count = CapturedTerritories(allPlayers[0].player_id);

            int count2 = 0;

            for (int i = 0; i < AllTerritories.Length; i++) 
            {
                if (AllTerritories[i].TerritoryActiveState)
                {
                    count2++;
                }
                else
                {
                    string tempColor2 = "044954";

                    Color OutputColor2;
                    ColorUtility.TryParseHtmlString("#" + tempColor2, out OutputColor2);

                    AllTerritories[i].Territory_image.color = OutputColor2;

                    AllTerritories[i].Territory_id = 0;

                }
            }

            Debug.Log("Count 2 = " + count2);

            if (!IsClaimPhaseStillOn())
            {
                if (count.Count <= 0)
                {
                    YouLostPanel.SetActive(true);

                    Invoke("LoadMainMenuAfterDelay", 3.5f);
                }
                else if (count.Count >= count2)
                {
                    YouWonPanel.SetActive(true);

                    Invoke("LoadMainMenuAfterDelay", 3.5f);
                }
            }

            if (allPlayers[PlayerTurnHolder - 1].CardNumbers.Count >= 0)
                TotalCardsText.text = allPlayers[PlayerTurnHolder - 1].CardNumbers.Count + "";
            else
                TotalCardsText.text = "0";

            yield return new WaitForSeconds(.1f);
        }
    }

    IEnumerator AiSlot2()
    {
        Debug.Log("Ai Slot 2 Is Started ");

        int totalplayers = PlayerPrefs.GetInt("allplayers");

        if (totalplayers >= 2)
        {
            while (true)
            { 
                yield return new WaitForSeconds(AiResponseTime);

                if (PlayerTurnHolder == 2)
                {
                    List<int> CapturedTerritoriesIDs2 = CapturedTerritories(PlayerTurnHolder);

                    if (!IsClaimPhaseStillOn())
                    {
                        if (CapturedTerritoriesIDs2.Count <= 0)
                        {
                            ChangePlayerTurn();
                            continue;
                        }
                    }

                    Debug.Log("Ai Slot 2 Is Working");

                    // Claim Phase Code
                    if (IsClaimPhaseStillOn())
                    {
                        List<int> AvailAbleTerritories = UnCapturedTerritories();

                        yield return new WaitForSeconds(AiResponseTime);

                        int TerritoryIdToCapture = -1;

                        // If List is Not Empty
                        if (AvailAbleTerritories.Count > 0)
                        {
                            int numberT = Random.Range(0, AvailAbleTerritories.Count);

                            count = 0;

                            // Getting Random Territory From Given List
                            foreach (int id in AvailAbleTerritories)
                            {
                                if (count == numberT)
                                {
                                    TerritoryIdToCapture = id;
                                    break;
                                }

                                count++;
                            }
                        }


                        if (TerritoryIdToCapture != -1)
                        {
                            ClaimTerritoryForAi(TerritoryIdToCapture);

                            allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                            ChangePlayerTurn();

                            yield return new WaitForSeconds(AiResponseTime);

                            continue;
                        }


                        // Incase Claiming Is Complete But Updating Troops On Claim Remains
                        List<int> CapturedTerritoriesIDs = CapturedTerritories(PlayerTurnHolder);

                        int CapturedTerritoryId = -1;

                        // If List is Not Empty
                        if (CapturedTerritoriesIDs.Count > 0)
                        {
                            int numberT = Random.Range(0, CapturedTerritoriesIDs.Count);

                            count = 0;

                            // Getting Random Territory From Given List
                            foreach (int id in CapturedTerritoriesIDs)
                            {
                                if (count == numberT)
                                {
                                    CapturedTerritoryId = id;
                                    break;
                                }

                                count++;
                            }
                        }


                        if (CapturedTerritoryId != -1)
                        {

                            int troops = int.Parse(AllTerritories[CapturedTerritoryId - 1].totalTroopsText.text);
                            troops += 1;
                            AllTerritories[CapturedTerritoryId - 1].totalTroopsText.text = troops + "";

                            allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                            yield return new WaitForSeconds(AiResponseTime);

                            ChangePlayerTurn();

                            continue;
                        }
                    }

                    //Deploy Troops Code
                    else
                    {
                        countTroops = allPlayers[PlayerTurnHolder - 1].TroopsInPossesion;

                        for (; countTroops > 0; countTroops--)
                        {
                            List<int> BorderTerritoriesIDs = BorderTerritories(PlayerTurnHolder);

                            int CapturedBorderTerritories = -1;

                            if (BorderTerritoriesIDs.Count > 0)
                            {
                                int numberT = Random.Range(0, BorderTerritoriesIDs.Count);

                                count = 0;

                                // Getting Random Territory From Given List
                                foreach (int id in BorderTerritoriesIDs)
                                {
                                    if (count == numberT)
                                    {
                                        CapturedBorderTerritories = id;
                                        break;
                                    }

                                    count++;
                                }

                                if (CapturedBorderTerritories != -1)
                                {

                                    int troops = int.Parse(AllTerritories[CapturedBorderTerritories - 1].totalTroopsText.text);
                                    troops += 1;
                                    AllTerritories[CapturedBorderTerritories - 1].totalTroopsText.text = troops + "";

                                    allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                                    yield return new WaitForSeconds(AiResponseTime);
                                }
                            }

                            else
                            {
                                List<int> CapturedTerritoriesIDs = CapturedTerritories(PlayerTurnHolder);

                                int CapturedTerritoryId = -1;

                                if (CapturedTerritoriesIDs.Count > 0)
                                {
                                    int numberT = Random.Range(0, CapturedTerritoriesIDs.Count);

                                    count = 0;

                                    // Getting Random Territory From Given List
                                    foreach (int id in CapturedTerritoriesIDs)
                                    {
                                        if (count == numberT)
                                        {
                                            CapturedTerritoryId = id;
                                            break;
                                        }

                                        count++;
                                    }
                                }


                                if (CapturedTerritoryId != -1)
                                {

                                    int troops = int.Parse(AllTerritories[CapturedTerritoryId - 1].totalTroopsText.text);
                                    troops += 1;
                                    AllTerritories[CapturedTerritoryId - 1].totalTroopsText.text = troops + "";

                                    allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                                    yield return new WaitForSeconds(AiResponseTime);
                                }
                            }
                        }

                        ChangePhase();

                        Debug.Log("Ai Checking");

                        // Attack Code
                        if (phase.Equals("attack"))
                        {
                            if (allPlayers[PlayerTurnHolder - 1].TroopsInPossesion < 3 && (allPlayers[PlayerTurnHolder - 1].TroopsInPossesion > 0))
                                allPlayers[PlayerTurnHolder - 1].TroopsInPossesion = 3;
                            /*
                             
                             if (allPlayers[PlayerTurnHolder - 1].AiDifficultyValue.Equals("Aggressive"))
                                StartCoroutine(AggressiveAttackStrategy());
                            else if (allPlayers[PlayerTurnHolder - 1].AiDifficultyValue.Equals("choatic"))
                                StartCoroutine(AggressiveAttackStrategy());
                            else if (allPlayers[PlayerTurnHolder - 1].AiDifficultyValue.Equals("passive"))
                                StartCoroutine(AggressiveAttackStrategy());
                            
                             */
                            StartCoroutine(AggressiveAttackStrategy());

                            int tempTurn = PlayerTurnHolder;

                            while (tempTurn == PlayerTurnHolder)
                                yield return new WaitForSeconds(AiResponseTime);
                        }
                    }
                }
            }
        }

        Debug.Log("Ai Slot 2 Is Exiting ");
    }

    IEnumerator AiSlot3()
    {
        Debug.Log("Ai Slot 3 Is Started ");

        int totalplayers = PlayerPrefs.GetInt("allplayers");

        if (totalplayers >= 3)
        {
            while (true)
            {
                yield return new WaitForSeconds(AiResponseTime);

                if (PlayerTurnHolder == 3)
                {
                    List<int> CapturedTerritoriesIDs2 = CapturedTerritories(PlayerTurnHolder);

                    if (!IsClaimPhaseStillOn())
                    {
                        if (CapturedTerritoriesIDs2.Count <= 0)
                        {
                            ChangePlayerTurn();
                            continue;
                        }
                    }

                    Debug.Log("Ai Slot 3 Is Working");

                    // Claim Phase Code
                    if (IsClaimPhaseStillOn())
                    {
                        List<int> AvailAbleTerritories = UnCapturedTerritories();

                        yield return new WaitForSeconds(AiResponseTime);

                        int TerritoryIdToCapture = -1;

                        // If List is Not Empty
                        if (AvailAbleTerritories.Count > 0)
                        {
                            int numberT = Random.Range(0, AvailAbleTerritories.Count);

                            count = 0;

                            // Getting Random Territory From Given List
                            foreach (int id in AvailAbleTerritories)
                            {
                                if (count == numberT)
                                {
                                    TerritoryIdToCapture = id;
                                    break;
                                }

                                count++;
                            }
                        }


                        if (TerritoryIdToCapture != -1)
                        {
                            ClaimTerritoryForAi(TerritoryIdToCapture);

                            allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                            ChangePlayerTurn();

                            yield return new WaitForSeconds(AiResponseTime);

                            continue;
                        }


                        // Incase Claiming Is Complete But Updating Troops On Claim Remains
                        List<int> CapturedTerritoriesIDs = CapturedTerritories(PlayerTurnHolder);

                        int CapturedTerritoryId = -1;

                        // If List is Not Empty
                        if (CapturedTerritoriesIDs.Count > 0)
                        {
                            int numberT = Random.Range(0, CapturedTerritoriesIDs.Count);

                            count = 0;

                            // Getting Random Territory From Given List
                            foreach (int id in CapturedTerritoriesIDs)
                            {
                                if (count == numberT)
                                {
                                    CapturedTerritoryId = id;
                                    break;
                                }

                                count++;
                            }
                        }


                        if (CapturedTerritoryId != -1)
                        {

                            int troops = int.Parse(AllTerritories[CapturedTerritoryId - 1].totalTroopsText.text);
                            troops += 1;
                            AllTerritories[CapturedTerritoryId - 1].totalTroopsText.text = troops + "";

                            allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                            yield return new WaitForSeconds(AiResponseTime);

                            ChangePlayerTurn();

                            continue;
                        }
                    }

                    //Deploy Troops Code
                    else
                    {
                        countTroops = allPlayers[PlayerTurnHolder - 1].TroopsInPossesion;

                        for (; countTroops > 0; countTroops--)
                        {
                            List<int> BorderTerritoriesIDs = BorderTerritories(PlayerTurnHolder);

                            int CapturedBorderTerritories = -1;

                            if (BorderTerritoriesIDs.Count > 0)
                            {
                                int numberT = Random.Range(0, BorderTerritoriesIDs.Count);

                                count = 0;

                                // Getting Random Territory From Given List
                                foreach (int id in BorderTerritoriesIDs)
                                {
                                    if (count == numberT)
                                    {
                                        CapturedBorderTerritories = id;
                                        break;
                                    }

                                    count++;
                                }

                                if (CapturedBorderTerritories != -1)
                                {

                                    int troops = int.Parse(AllTerritories[CapturedBorderTerritories - 1].totalTroopsText.text);
                                    troops += 1;
                                    AllTerritories[CapturedBorderTerritories - 1].totalTroopsText.text = troops + "";

                                    allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                                    yield return new WaitForSeconds(AiResponseTime);
                                }
                            }

                            else
                            {
                                List<int> CapturedTerritoriesIDs = CapturedTerritories(PlayerTurnHolder);

                                int CapturedTerritoryId = -1;

                                if (CapturedTerritoriesIDs.Count > 0)
                                {
                                    int numberT = Random.Range(0, CapturedTerritoriesIDs.Count);

                                    count = 0;

                                    // Getting Random Territory From Given List
                                    foreach (int id in CapturedTerritoriesIDs)
                                    {
                                        if (count == numberT)
                                        {
                                            CapturedTerritoryId = id;
                                            break;
                                        }

                                        count++;
                                    }
                                }


                                if (CapturedTerritoryId != -1)
                                {

                                    int troops = int.Parse(AllTerritories[CapturedTerritoryId - 1].totalTroopsText.text);
                                    troops += 1;
                                    AllTerritories[CapturedTerritoryId - 1].totalTroopsText.text = troops + "";

                                    allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                                    yield return new WaitForSeconds(AiResponseTime);
                                }
                            }
                        }

                        ChangePhase();

                        Debug.Log("Ai Checking");

                        // Attack Code
                        if (phase.Equals("attack"))
                        {
                            if (allPlayers[PlayerTurnHolder - 1].TroopsInPossesion < 3 && (allPlayers[PlayerTurnHolder - 1].TroopsInPossesion > 0))
                                allPlayers[PlayerTurnHolder - 1].TroopsInPossesion = 3;
                            /*
                             
                             if (allPlayers[PlayerTurnHolder - 1].AiDifficultyValue.Equals("Aggressive"))
                                StartCoroutine(AggressiveAttackStrategy());
                            else if (allPlayers[PlayerTurnHolder - 1].AiDifficultyValue.Equals("choatic"))
                                StartCoroutine(AggressiveAttackStrategy());
                            else if (allPlayers[PlayerTurnHolder - 1].AiDifficultyValue.Equals("passive"))
                                StartCoroutine(AggressiveAttackStrategy());
                            
                             */
                            StartCoroutine(AggressiveAttackStrategy());

                            int tempTurn = PlayerTurnHolder;

                            while (tempTurn == PlayerTurnHolder)
                                yield return new WaitForSeconds(AiResponseTime);
                        }
                    }
                }
            }
        }

        Debug.Log("Ai Slot 3 Is Exiting ");
    }

    IEnumerator AiSlot4()
    {
        Debug.Log("Ai Slot 4 Is Started ");

        int totalplayers = PlayerPrefs.GetInt("allplayers");

        if (totalplayers >= 4)
        {
            while (true)
            {
                yield return new WaitForSeconds(AiResponseTime);

                if (PlayerTurnHolder == 4)
                {
                    List<int> CapturedTerritoriesIDs2 = CapturedTerritories(PlayerTurnHolder);

                    if (!IsClaimPhaseStillOn())
                    {
                        if (CapturedTerritoriesIDs2.Count <= 0)
                        {
                            ChangePlayerTurn();
                            continue;
                        }
                    }

                    Debug.Log("Ai Slot 4 Is Working");

                    // Claim Phase Code
                    if (IsClaimPhaseStillOn())
                    {
                        List<int> AvailAbleTerritories = UnCapturedTerritories();

                        yield return new WaitForSeconds(AiResponseTime);

                        int TerritoryIdToCapture = -1;

                        // If List is Not Empty
                        if (AvailAbleTerritories.Count > 0)
                        {
                            int numberT = Random.Range(0, AvailAbleTerritories.Count);

                            count = 0;

                            // Getting Random Territory From Given List
                            foreach (int id in AvailAbleTerritories)
                            {
                                if (count == numberT)
                                {
                                    TerritoryIdToCapture = id;
                                    break;
                                }

                                count++;
                            }
                        }


                        if (TerritoryIdToCapture != -1)
                        {
                            ClaimTerritoryForAi(TerritoryIdToCapture);

                            allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                            ChangePlayerTurn();

                            yield return new WaitForSeconds(AiResponseTime);

                            continue;
                        }


                        // Incase Claiming Is Complete But Updating Troops On Claim Remains
                        List<int> CapturedTerritoriesIDs = CapturedTerritories(PlayerTurnHolder);

                        int CapturedTerritoryId = -1;

                        // If List is Not Empty
                        if (CapturedTerritoriesIDs.Count > 0)
                        {
                            int numberT = Random.Range(0, CapturedTerritoriesIDs.Count);

                            count = 0;

                            // Getting Random Territory From Given List
                            foreach (int id in CapturedTerritoriesIDs)
                            {
                                if (count == numberT)
                                {
                                    CapturedTerritoryId = id;
                                    break;
                                }

                                count++;
                            }
                        }


                        if (CapturedTerritoryId != -1)
                        {

                            int troops = int.Parse(AllTerritories[CapturedTerritoryId - 1].totalTroopsText.text);
                            troops += 1;
                            AllTerritories[CapturedTerritoryId - 1].totalTroopsText.text = troops + "";

                            allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                            yield return new WaitForSeconds(AiResponseTime);

                            ChangePlayerTurn();

                            continue;
                        }
                    }

                    //Deploy Troops Code
                    else
                    {
                        countTroops = allPlayers[PlayerTurnHolder - 1].TroopsInPossesion;

                        for (; countTroops > 0; countTroops--)
                        {
                            List<int> BorderTerritoriesIDs = BorderTerritories(PlayerTurnHolder);

                            int CapturedBorderTerritories = -1;

                            if (BorderTerritoriesIDs.Count > 0)
                            {
                                int numberT = Random.Range(0, BorderTerritoriesIDs.Count);

                                count = 0;

                                // Getting Random Territory From Given List
                                foreach (int id in BorderTerritoriesIDs)
                                {
                                    if (count == numberT)
                                    {
                                        CapturedBorderTerritories = id;
                                        break;
                                    }

                                    count++;
                                }

                                if (CapturedBorderTerritories != -1)
                                {

                                    int troops = int.Parse(AllTerritories[CapturedBorderTerritories - 1].totalTroopsText.text);
                                    troops += 1;
                                    AllTerritories[CapturedBorderTerritories - 1].totalTroopsText.text = troops + "";

                                    allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                                    yield return new WaitForSeconds(AiResponseTime);
                                }
                            }

                            else
                            {
                                List<int> CapturedTerritoriesIDs = CapturedTerritories(PlayerTurnHolder);

                                int CapturedTerritoryId = -1;

                                if (CapturedTerritoriesIDs.Count > 0)
                                {
                                    int numberT = Random.Range(0, CapturedTerritoriesIDs.Count);

                                    count = 0;

                                    // Getting Random Territory From Given List
                                    foreach (int id in CapturedTerritoriesIDs)
                                    {
                                        if (count == numberT)
                                        {
                                            CapturedTerritoryId = id;
                                            break;
                                        }

                                        count++;
                                    }
                                }


                                if (CapturedTerritoryId != -1)
                                {

                                    int troops = int.Parse(AllTerritories[CapturedTerritoryId - 1].totalTroopsText.text);
                                    troops += 1;
                                    AllTerritories[CapturedTerritoryId - 1].totalTroopsText.text = troops + "";

                                    allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                                    yield return new WaitForSeconds(AiResponseTime);
                                }
                            }
                        }

                        ChangePhase();

                        Debug.Log("Ai Checking");

                        // Attack Code
                        if (phase.Equals("attack"))
                        {
                            if (allPlayers[PlayerTurnHolder - 1].TroopsInPossesion < 3 && (allPlayers[PlayerTurnHolder - 1].TroopsInPossesion > 0))
                                allPlayers[PlayerTurnHolder - 1].TroopsInPossesion = 3;
                            /*
                             
                             if (allPlayers[PlayerTurnHolder - 1].AiDifficultyValue.Equals("Aggressive"))
                                StartCoroutine(AggressiveAttackStrategy());
                            else if (allPlayers[PlayerTurnHolder - 1].AiDifficultyValue.Equals("choatic"))
                                StartCoroutine(AggressiveAttackStrategy());
                            else if (allPlayers[PlayerTurnHolder - 1].AiDifficultyValue.Equals("passive"))
                                StartCoroutine(AggressiveAttackStrategy());
                            
                             */
                            StartCoroutine(AggressiveAttackStrategy());

                            int tempTurn = PlayerTurnHolder;

                            while (tempTurn == PlayerTurnHolder)
                                yield return new WaitForSeconds(AiResponseTime);
                        }
                    }
                }
            }
        }

        Debug.Log("Ai Slot 4 Is Exiting ");
    }

    IEnumerator AiSlot5()
    {
        Debug.Log("Ai Slot 5 Is Started ");

        int totalplayers = PlayerPrefs.GetInt("allplayers");

        if (totalplayers >= 5)
        {
            while (true)
            {
                yield return new WaitForSeconds(AiResponseTime);

                if (PlayerTurnHolder == 5)
                {
                    List<int> CapturedTerritoriesIDs2 = CapturedTerritories(PlayerTurnHolder);

                    if (!IsClaimPhaseStillOn())
                    {
                        if (CapturedTerritoriesIDs2.Count <= 0)
                        {
                            ChangePlayerTurn();
                            continue;
                        }
                    }

                    Debug.Log("Ai Slot 5 Is Working");

                    // Claim Phase Code
                    if (IsClaimPhaseStillOn())
                    {
                        List<int> AvailAbleTerritories = UnCapturedTerritories();

                        yield return new WaitForSeconds(AiResponseTime);

                        int TerritoryIdToCapture = -1;

                        // If List is Not Empty
                        if (AvailAbleTerritories.Count > 0)
                        {
                            int numberT = Random.Range(0, AvailAbleTerritories.Count);

                            count = 0;

                            // Getting Random Territory From Given List
                            foreach (int id in AvailAbleTerritories)
                            {
                                if (count == numberT)
                                {
                                    TerritoryIdToCapture = id;
                                    break;
                                }

                                count++;
                            }
                        }


                        if (TerritoryIdToCapture != -1)
                        {
                            ClaimTerritoryForAi(TerritoryIdToCapture);

                            allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                            ChangePlayerTurn();

                            yield return new WaitForSeconds(AiResponseTime);

                            continue;
                        }


                        // Incase Claiming Is Complete But Updating Troops On Claim Remains
                        List<int> CapturedTerritoriesIDs = CapturedTerritories(PlayerTurnHolder);

                        int CapturedTerritoryId = -1;

                        // If List is Not Empty
                        if (CapturedTerritoriesIDs.Count > 0)
                        {
                            int numberT = Random.Range(0, CapturedTerritoriesIDs.Count);

                            count = 0;

                            // Getting Random Territory From Given List
                            foreach (int id in CapturedTerritoriesIDs)
                            {
                                if (count == numberT)
                                {
                                    CapturedTerritoryId = id;
                                    break;
                                }

                                count++;
                            }
                        }


                        if (CapturedTerritoryId != -1)
                        {

                            int troops = int.Parse(AllTerritories[CapturedTerritoryId - 1].totalTroopsText.text);
                            troops += 1;
                            AllTerritories[CapturedTerritoryId - 1].totalTroopsText.text = troops + "";

                            allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                            yield return new WaitForSeconds(AiResponseTime);

                            ChangePlayerTurn();

                            continue;
                        }
                    }

                    //Deploy Troops Code
                    else
                    {
                        countTroops = allPlayers[PlayerTurnHolder - 1].TroopsInPossesion;

                        for (; countTroops > 0; countTroops--)
                        {
                            List<int> BorderTerritoriesIDs = BorderTerritories(PlayerTurnHolder);

                            int CapturedBorderTerritories = -1;

                            if (BorderTerritoriesIDs.Count > 0)
                            {
                                int numberT = Random.Range(0, BorderTerritoriesIDs.Count);

                                count = 0;

                                // Getting Random Territory From Given List
                                foreach (int id in BorderTerritoriesIDs)
                                {
                                    if (count == numberT)
                                    {
                                        CapturedBorderTerritories = id;
                                        break;
                                    }

                                    count++;
                                }

                                if (CapturedBorderTerritories != -1)
                                {

                                    int troops = int.Parse(AllTerritories[CapturedBorderTerritories - 1].totalTroopsText.text);
                                    troops += 1;
                                    AllTerritories[CapturedBorderTerritories - 1].totalTroopsText.text = troops + "";

                                    allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                                    yield return new WaitForSeconds(AiResponseTime);
                                }
                            }

                            else
                            {
                                List<int> CapturedTerritoriesIDs = CapturedTerritories(PlayerTurnHolder);

                                int CapturedTerritoryId = -1;

                                if (CapturedTerritoriesIDs.Count > 0)
                                {
                                    int numberT = Random.Range(0, CapturedTerritoriesIDs.Count);

                                    count = 0;

                                    // Getting Random Territory From Given List
                                    foreach (int id in CapturedTerritoriesIDs)
                                    {
                                        if (count == numberT)
                                        {
                                            CapturedTerritoryId = id;
                                            break;
                                        }

                                        count++;
                                    }
                                }


                                if (CapturedTerritoryId != -1)
                                {

                                    int troops = int.Parse(AllTerritories[CapturedTerritoryId - 1].totalTroopsText.text);
                                    troops += 1;
                                    AllTerritories[CapturedTerritoryId - 1].totalTroopsText.text = troops + "";

                                    allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                                    yield return new WaitForSeconds(AiResponseTime);
                                }
                            }
                        }

                        ChangePhase();

                        Debug.Log("Ai Checking");

                        // Attack Code
                        if (phase.Equals("attack"))
                        {
                            if (allPlayers[PlayerTurnHolder - 1].TroopsInPossesion < 3 && (allPlayers[PlayerTurnHolder - 1].TroopsInPossesion > 0))
                                allPlayers[PlayerTurnHolder - 1].TroopsInPossesion = 3;
                            /*
                             
                             if (allPlayers[PlayerTurnHolder - 1].AiDifficultyValue.Equals("Aggressive"))
                                StartCoroutine(AggressiveAttackStrategy());
                            else if (allPlayers[PlayerTurnHolder - 1].AiDifficultyValue.Equals("choatic"))
                                StartCoroutine(AggressiveAttackStrategy());
                            else if (allPlayers[PlayerTurnHolder - 1].AiDifficultyValue.Equals("passive"))
                                StartCoroutine(AggressiveAttackStrategy());
                            
                             */
                            StartCoroutine(AggressiveAttackStrategy());

                            int tempTurn = PlayerTurnHolder;

                            while (tempTurn == PlayerTurnHolder)
                                yield return new WaitForSeconds(AiResponseTime);
                        }
                    }
                }
            }
        }

        Debug.Log("Ai Slot 5 Is Exiting ");
    }

    IEnumerator AiSlot6()
    {
        Debug.Log("Ai Slot 6 Is Started ");

        int totalplayers = PlayerPrefs.GetInt("allplayers");

        if (totalplayers >= 6)
        {
            while (true)
            {
                yield return new WaitForSeconds(AiResponseTime);

                if (PlayerTurnHolder == 6)
                {
                    List<int> CapturedTerritoriesIDs2 = CapturedTerritories(PlayerTurnHolder);

                    if (!IsClaimPhaseStillOn())
                    {
                        if (CapturedTerritoriesIDs2.Count <= 0)
                        {
                            ChangePlayerTurn();
                            continue;
                        }
                    }

                    Debug.Log("Ai Slot 6 Is Working");

                    // Claim Phase Code
                    if (IsClaimPhaseStillOn())
                    {
                        List<int> AvailAbleTerritories = UnCapturedTerritories();

                        yield return new WaitForSeconds(AiResponseTime);

                        int TerritoryIdToCapture = -1;

                        // If List is Not Empty
                        if (AvailAbleTerritories.Count > 0)
                        {
                            int numberT = Random.Range(0, AvailAbleTerritories.Count);

                            count = 0;

                            // Getting Random Territory From Given List
                            foreach (int id in AvailAbleTerritories)
                            {
                                if (count == numberT)
                                {
                                    TerritoryIdToCapture = id;
                                    break;
                                }

                                count++;
                            }
                        }


                        if (TerritoryIdToCapture != -1)
                        {
                            ClaimTerritoryForAi(TerritoryIdToCapture);

                            allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                            ChangePlayerTurn();

                            yield return new WaitForSeconds(AiResponseTime);

                            continue;
                        }


                        // Incase Claiming Is Complete But Updating Troops On Claim Remains
                        List<int> CapturedTerritoriesIDs = CapturedTerritories(PlayerTurnHolder);

                        int CapturedTerritoryId = -1;

                        // If List is Not Empty
                        if (CapturedTerritoriesIDs.Count > 0)
                        {
                            int numberT = Random.Range(0, CapturedTerritoriesIDs.Count);

                            count = 0;

                            // Getting Random Territory From Given List
                            foreach (int id in CapturedTerritoriesIDs)
                            {
                                if (count == numberT)
                                {
                                    CapturedTerritoryId = id;
                                    break;
                                }

                                count++;
                            }
                        }


                        if (CapturedTerritoryId != -1)
                        {

                            int troops = int.Parse(AllTerritories[CapturedTerritoryId - 1].totalTroopsText.text);
                            troops += 1;
                            AllTerritories[CapturedTerritoryId - 1].totalTroopsText.text = troops + "";

                            allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                            yield return new WaitForSeconds(AiResponseTime);

                            ChangePlayerTurn();

                            continue;
                        }
                    }

                    //Deploy Troops Code
                    else
                    {
                        countTroops = allPlayers[PlayerTurnHolder - 1].TroopsInPossesion;

                        for (; countTroops > 0; countTroops--)
                        {
                            List<int> BorderTerritoriesIDs = BorderTerritories(PlayerTurnHolder);

                            int CapturedBorderTerritories = -1;

                            if (BorderTerritoriesIDs.Count > 0)
                            {
                                int numberT = Random.Range(0, BorderTerritoriesIDs.Count);

                                count = 0;

                                // Getting Random Territory From Given List
                                foreach (int id in BorderTerritoriesIDs)
                                {
                                    if (count == numberT)
                                    {
                                        CapturedBorderTerritories = id;
                                        break;
                                    }

                                    count++;
                                }

                                if (CapturedBorderTerritories != -1)
                                {

                                    int troops = int.Parse(AllTerritories[CapturedBorderTerritories - 1].totalTroopsText.text);
                                    troops += 1;
                                    AllTerritories[CapturedBorderTerritories - 1].totalTroopsText.text = troops + "";

                                    allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                                    yield return new WaitForSeconds(AiResponseTime);
                                }
                            }

                            else
                            {
                                List<int> CapturedTerritoriesIDs = CapturedTerritories(PlayerTurnHolder);

                                int CapturedTerritoryId = -1;

                                if (CapturedTerritoriesIDs.Count > 0)
                                {
                                    int numberT = Random.Range(0, CapturedTerritoriesIDs.Count);

                                    count = 0;

                                    // Getting Random Territory From Given List
                                    foreach (int id in CapturedTerritoriesIDs)
                                    {
                                        if (count == numberT)
                                        {
                                            CapturedTerritoryId = id;
                                            break;
                                        }

                                        count++;
                                    }
                                }


                                if (CapturedTerritoryId != -1)
                                {

                                    int troops = int.Parse(AllTerritories[CapturedTerritoryId - 1].totalTroopsText.text);
                                    troops += 1;
                                    AllTerritories[CapturedTerritoryId - 1].totalTroopsText.text = troops + "";

                                    allPlayers[PlayerTurnHolder - 1].TroopsInPossesion--;

                                    yield return new WaitForSeconds(AiResponseTime);
                                }
                            }
                        }

                        ChangePhase();

                        Debug.Log("Ai Checking");

                        // Attack Code
                        if (phase.Equals("attack"))
                        {
                            if (allPlayers[PlayerTurnHolder - 1].TroopsInPossesion < 3 && (allPlayers[PlayerTurnHolder - 1].TroopsInPossesion > 0))
                                allPlayers[PlayerTurnHolder - 1].TroopsInPossesion = 3;
                            /*
                             
                             if (allPlayers[PlayerTurnHolder - 1].AiDifficultyValue.Equals("Aggressive"))
                                StartCoroutine(AggressiveAttackStrategy());
                            else if (allPlayers[PlayerTurnHolder - 1].AiDifficultyValue.Equals("choatic"))
                                StartCoroutine(AggressiveAttackStrategy());
                            else if (allPlayers[PlayerTurnHolder - 1].AiDifficultyValue.Equals("passive"))
                                StartCoroutine(AggressiveAttackStrategy());
                            
                             */
                            StartCoroutine(AggressiveAttackStrategy());

                            int tempTurn = PlayerTurnHolder;

                            while (tempTurn == PlayerTurnHolder)
                                yield return new WaitForSeconds(AiResponseTime);
                        }
                    }
                }
            }
        }

        Debug.Log("Ai Slot 6 Is Exiting ");
    }

    public List<int> UnCapturedTerritories()
    {
        List<int> AvailAbleTerritoryIds = new List<int>();

        for (int i = 0; i < AllTerritories.Length; i++)
            if (AllTerritories[i].CapturedBy == 0 && AllTerritories[i].TerritoryActiveState)
            {
                AvailAbleTerritoryIds.Add(AllTerritories[i].Territory_id);
            }

        return AvailAbleTerritoryIds;
    }

    public List<int> CapturedTerritories(int PlayerId)
    {
        List<int> CapturedTerritories = new List<int>();

        for (int i = 0; i < AllTerritories.Length; i++)
            if (AllTerritories[i].CapturedBy == PlayerId)
            {
                CapturedTerritories.Add(AllTerritories[i].Territory_id);
            }

        return CapturedTerritories;
    }

    public List<int> BorderTerritories(int PlayerId)
    {
        List<int> CapturedTerritories = new List<int>();

        for (int i = 0; i < AllTerritories.Length; i++)
            if (AllTerritories[i].CapturedBy == PlayerId)
            {
                CapturedTerritories.Add(AllTerritories[i].Territory_id);
            }

        List<int> BorderTerritories = new List<int>();

        foreach(int id in CapturedTerritories)
        {
            GameObject[] Neighbours = AllTerritories[id - 1].Neigbhors;

            bool flag = true;

            for (int i = 0; i < Neighbours.Length && flag; i++)
            {
                for (int j = 0; j < AllTerritories.Length; j++)
                {
                    if(Neighbours[i].name.Equals(AllTerritories[j].Territory_name))
                    {
                        if (AllTerritories[j].CapturedBy != PlayerId)
                        {
                            BorderTerritories.Add(id);
                            flag = false;
                            break;
                        }
                    }
                }
            }
        }

        return BorderTerritories;

    }

    public void LoadMainMenuAfterDelay()
    {
        StopAllCoroutines();

        SceneManager.LoadScene(MainMenuIndex);
    }
}
