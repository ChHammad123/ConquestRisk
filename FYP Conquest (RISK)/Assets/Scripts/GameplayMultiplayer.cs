using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayMultiplayer : MonoBehaviour
{
    private readonly string playerDataURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/getDataForWaitingRoom";
    private readonly string synchMapDataURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/getMapData";
    private readonly string playerHavingClaimStatusURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/getPlayerDataHavingClaimStatus";
    private readonly string playerHavingDeployStatusURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/getPlayerDataHavingDeployStatus";
    private readonly string ClaimTerritoryURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/ClaimTerritory";
    private readonly string MovePlayersToDraftURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/movePlayersToDraft";
    private readonly string DeployTroopsURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/DeployTroops";
    private readonly string FortifyTroopsURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/FortifyTerriory";
    private readonly string AttackUpdateTroopsURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/AttackTerritory";
    private readonly string CaptureTerritoryInAttackURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/CaptureTerritoryInAttack";
    private readonly string EndTurnURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/EndTurn";

    public string[] allAvatars;

    public PlayerDataHolder[] allPlayers;

    public MapDataHolder[] allTerritories;

    [Header("UI Elements")]
    [SerializeField]
    private GameObject DraftFortifyTroopsSelectionPanel;

    public Image Bg1, Bg2;

    public Text phaseText;

    public GameObject[] Avatars;

    public Toggle[] allToggles;

    [SerializeField]
    private bool claimPhaseCompleted = false;

    public Text troopsToDeployText;

    public Text DiceText;

    public GameObject[] allDicesParentAttacker;

    public GameObject[] allDicesParentDefender;

    public DiceDataHandler[] AttackerDices;

    public DiceDataHandler[] DefenderDices;

    public GameObject AttackPanel;

    public string playerPhase = "claim";

    public bool neigbhorsActive = false;

    public bool fortifyNeigbhorsActive = false;

    public GameObject NextPhaseButton;

    private int troopsToDeploy = 0;

    private string tempTerritoryName = "";
    
    private string AttackingTerritoryName = "";
    
    private string DefendingTerritoryName = "";

    private int room_id = 0;

    MapDataHolder AttackingTerritory = null;
    MapDataHolder DefendingTerritory = null;

    private int a, a2, a3, d1, d2, d3;
    private int Amax, Amin, Asmax;
    private int Dmax, Dmin, Dsmax;

    void Start()
    {
        NextPhaseButton.SetActive(false);

        DraftFortifyTroopsSelectionPanel.SetActive(false);

        troopsToDeployText.gameObject.SetActive(false);
        // For Claim Phase

        StartCoroutine(trySetPlayersDataOnSide());

        StartCoroutine(trySynchMapData());

        StartCoroutine(trySetPlayerDataOnClaim());

        StartCoroutine(tryMovePlayersToDraftCheck());

    }

    IEnumerator trySetPlayersDataOnSide()
    {
        WWWForm form = new WWWForm();

        string roomKey = PlayerPrefs.GetString("roomKey");

        form.AddField("ROOM_Key", roomKey);

        using (UnityWebRequest request = UnityWebRequest.Post(playerDataURL, form))
        {
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("Error Getting the Room Data ...  ");
            }
            else
            {
                JSONNode data = JSON.Parse(request.downloadHandler.text);

                if (data.Count != 0)
                {
                    int i = 0;

                    for (i = 0; i < data.Count; i++)
                    {

                        // Activating Player Slot
                        allPlayers[i].parent.SetActive(true);

                        string tempColor = data[i]["p_Color"];

                        Color OutputColor;
                        ColorUtility.TryParseHtmlString("#"+tempColor, out OutputColor);

                        allPlayers[i].bg.color = OutputColor;

                        // Assigning name
                        allPlayers[i].name.text = data[i]["p_Name"];

                        // Deactivating Avatar

                        for (int k = 0; k < allPlayers[i].Avatars.Length; k++)
                        {
                            allPlayers[i].Avatars[k].SetActive(false);
                        }

                        // Activating Avatar

                        string avatarName = data[i]["p_Avatar"];
                        int AvatarNo = 1;

                        for (int k = 0; k < allAvatars.Length; k++)
                        {
                            if (allAvatars[k].Equals(avatarName))
                            {
                                AvatarNo = k;
                                break;
                            }
                        }

                        allPlayers[i].Avatars[AvatarNo].SetActive(true);

                    }
                    for (; i < 6; i++)
                    {
                        allPlayers[i].parent.SetActive(false);
                    }
                }

                else
                    SceneManager.LoadScene(3);
            }
        }
    }

    IEnumerator trySynchMapData()
    {
        WWWForm form = new WWWForm();

        string roomKey = PlayerPrefs.GetString("roomKey");

        form.AddField("ROOM_Key", roomKey);

        while (true)
        {
            using (UnityWebRequest request = UnityWebRequest.Post(synchMapDataURL, form))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log("Error Getting the Room Data ...  ");
                }
                else
                {
                    JSONNode data = JSON.Parse(request.downloadHandler.text);

                    if (data.Count != 0)
                    {
                        int i = 0;

                        room_id = data[0]["MAP_ID"];

                        for (i = 0; i < data.Count; i++)
                        {
                            // Assigning Troops Text
                            int troops = data[i]["TOTAL_TROOPS"];

                            if (troops > 0)
                            {
                                allTerritories[i].totalTroopsText.gameObject.SetActive(true);
                                allTerritories[i].totalTroopsText.text = data[i]["TOTAL_TROOPS"];
                            }

                            // Assigning Color
                            string tempColor = data[i]["COLOR"];

                            Color OutputColor;
                            ColorUtility.TryParseHtmlString("#" + tempColor, out OutputColor);

                            allTerritories[i].Territory_image.color = OutputColor;

                            // Assigning Captured By
                            allTerritories[i].CapturedBy = data[i]["CAPTURED_BY"];
                   
                        }
                    }

                    else
                        SceneManager.LoadScene(3);
                }
            }
            yield return new WaitForSeconds(2f);
        }
    }

    IEnumerator trySetPlayerDataOnClaim()
    {
        WWWForm form = new WWWForm();

        string roomKey = PlayerPrefs.GetString("roomKey");

        form.AddField("ROOM_Key", roomKey);

        while (!claimPhaseCompleted)
        {
            using (UnityWebRequest request = UnityWebRequest.Post(playerHavingClaimStatusURL, form))
            {
                yield return request.SendWebRequest();

                if (request.isHttpError || request.isNetworkError)
                {
                    Debug.Log("Error Getting the Data For Claim Player ...  ");
                }
                else
                {
                    JSONNode data = JSON.Parse(request.downloadHandler.text);

                    string tempColor = data[0]["p_Color"];

                    Color OutputColor;
                    ColorUtility.TryParseHtmlString("#" + tempColor, out OutputColor);

                    Bg1.color = OutputColor;
                    Bg2.color = OutputColor;


                    for (int k = 0; k < Avatars.Length; k++)
                    {
                        Avatars[k].SetActive(false);
                    }

                    // Activating Avatar
                    string avatarName = data[0]["p_Avatar"];
                    int AvatarNo = 1;

                    for (int k = 0; k < allAvatars.Length; k++)
                    {
                        if (allAvatars[k].Equals(avatarName))
                        {
                            AvatarNo = k;
                            break;
                        }
                    }

                    Avatars[AvatarNo].SetActive(true);
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator tryMovePlayersToDraftCheck()
    {
        WWWForm form = new WWWForm();

        string roomKey = PlayerPrefs.GetString("roomKey");

        form.AddField("ROOM_Key", roomKey);

        while (!claimPhaseCompleted)
        {
            using (UnityWebRequest request = UnityWebRequest.Post(MovePlayersToDraftURL, form))
            {
                yield return request.SendWebRequest();

                if (request.isHttpError || request.isNetworkError)
                {
                    Debug.Log("Error Getting the MovePlayerToDraft Data ...  ");
                }
                else
                {
                    JSONNode data = JSON.Parse(request.downloadHandler.text);

                    if(data[0].Equals("0"))
                    {
                        Debug.Log("Still in Claim Phase");
                    }
                    else
                    {
                        claimPhaseCompleted = true;

                        troopsToDeploy = 3;
                        troopsToDeployText.text = 3+"";
                        troopsToDeployText.gameObject.SetActive(true);

                        ChangeToggle(0);

                        DraftFortifyTroopsSelectionPanel.SetActive(true);

                        StartCoroutine(trySetPlayerDataOnDeploy());

                        phaseText.text = "Deploy";
                        playerPhase = "deploy";
                    }
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }


    // Claim Territory

    public void ClaimTerritory(string territoryName)
    {
        if (claimPhaseCompleted)
            return;

        tempTerritoryName = territoryName;

        StartCoroutine(tryCheckingForPlayerStatus());
    }

    IEnumerator tryCheckingForPlayerStatus()
    {
        WWWForm form = new WWWForm();

        string roomKey = PlayerPrefs.GetString("roomKey");

        form.AddField("ROOM_Key", roomKey);

        using (UnityWebRequest request = UnityWebRequest.Post(playerHavingClaimStatusURL, form))
        {
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("Error Getting the Room Data ...  ");
            }
            else
            {
                JSONNode data = JSON.Parse(request.downloadHandler.text);

                int tempId = data[0]["p_ID"];
                int tempId2 = PlayerPrefs.GetInt("id");

                if (tempId == tempId2)
                    StartCoroutine(tryClaimingTerritory());
                else
                    Debug.Log("Cannot Claim Not your Turn");
            }
        }

    }

    IEnumerator tryClaimingTerritory()
    {
        WWWForm form = new WWWForm();

        string roomKey = PlayerPrefs.GetString("roomKey");

        form.AddField("COLOR", roomKey);
        form.AddField("TERRITORY_ID", PlayerPrefs.GetInt("id"));
        form.AddField("TERRITORY_NAME", tempTerritoryName);

        Debug.Log("Making Request On Room id " + room_id + "\t With Player ID " + PlayerPrefs.GetInt("id") + "\t With Territory Name " + tempTerritoryName);

        using (UnityWebRequest request = UnityWebRequest.Post(ClaimTerritoryURL, form))
        {
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("Error While Claiming Territory ...  ");
            }
            else
            {
                JSONNode data = JSON.Parse(request.downloadHandler.text);

                Debug.Log("Territory Claimed "+ data);
            }
        }
    }

    // Deploy Troops On Territory

    public void ChangeToggle(int index)
    {
        for (int i = 0; i < allToggles.Length; i++)
            allToggles[i].gameObject.SetActive(true);


        if (playerPhase.Equals("deploy"))
        {
            for (int i = 0; i < allToggles.Length; i++)
            {
                int troopsSelected = int.Parse(allToggles[i].gameObject.name);

                if (troopsSelected > troopsToDeploy)
                    allToggles[i].gameObject.SetActive(false);
            }
        }
    
        for (int i = 0; i < allToggles.Length; i++)
            if(i != index && allToggles[i].gameObject.activeInHierarchy)
                allToggles[i].isOn = false;

    }

    public void DeployTroops(string territoryName)
    {

        if (!playerPhase.Equals("deploy"))
            return;

        if (troopsToDeploy <= 0)
            return;

        tempTerritoryName = territoryName;

        if (!claimPhaseCompleted)
            return;


        for (int i = 0; i < allToggles.Length; i++) 
            if (allToggles[i].isOn)
            {
                int troops = int.Parse(allToggles[i].gameObject.name);

                for (int j = 0; j < allTerritories.Length; j++) 
                    if(allTerritories[j].Territory_name.Equals(tempTerritoryName))
                    {
                        troops += int.Parse(allTerritories[j].totalTroopsText.text);
                        break;
                    }

                StartCoroutine(tryDeployingTroops(troops));

                break;
            }
    }

    IEnumerator tryDeployingTroops(int troops)
    {
        WWWForm form = new WWWForm();

        string roomKey = PlayerPrefs.GetString("roomKey");

        form.AddField("COLOR", roomKey);
        form.AddField("TERRITORY_ID", PlayerPrefs.GetInt("id"));
        form.AddField("TERRITORY_NAME", tempTerritoryName);
        form.AddField("TOTAL_TROOPS", troops);

        using (UnityWebRequest request = UnityWebRequest.Post(DeployTroopsURL, form))
        {
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("Error While Deploying Territory ...  ");
            }
            else
            {
                JSONNode data = JSON.Parse(request.downloadHandler.text);

                if (data[0].Equals("1"))
                {
                    for (int i = 0; i < allToggles.Length; i++)
                        if (allToggles[i].isOn)
                        {
                            int SelectedTroops = int.Parse(allToggles[i].gameObject.name);
                            troopsToDeploy -= SelectedTroops;
                        }
                }
                else
                {
                    Debug.Log("Not Player's Territory ...  ");
                }
                ChangeToggle(0);
            }
        }
    }

    IEnumerator trySetPlayerDataOnDeploy()
    {
        WWWForm form = new WWWForm();

        string roomKey = PlayerPrefs.GetString("roomKey");

        form.AddField("ROOM_Key", roomKey);

        while (claimPhaseCompleted)
        {
            using (UnityWebRequest request = UnityWebRequest.Post(playerHavingDeployStatusURL, form))
            {
                yield return request.SendWebRequest();

                if (request.isHttpError || request.isNetworkError)
                {
                    Debug.Log("Error Getting the Data For Deploy Player ...  ");
                }
                else
                {
                    JSONNode data = JSON.Parse(request.downloadHandler.text);

                    int id = data[0]["p_ID"];

                    if (id != PlayerPrefs.GetInt("id"))
                    {
                        playerPhase = "ingame";
                        NextPhaseButton.SetActive(false);
                        troopsToDeployText.gameObject.SetActive(false);
                    }
                    else
                    {
                        playerPhase = "deploy";
                        DraftFortifyTroopsSelectionPanel.SetActive(true);
                        troopsToDeployText.gameObject.SetActive(false);
                        NextPhaseButton.SetActive(true);
                    }

                    string tempColor = data[0]["p_Color"];

                    Color OutputColor;
                    ColorUtility.TryParseHtmlString("#" + tempColor, out OutputColor);

                    Bg1.color = OutputColor;
                    Bg2.color = OutputColor;


                    for (int k = 0; k < Avatars.Length; k++)
                    {
                        Avatars[k].SetActive(false);
                    }

                    // Activating Avatar
                    string avatarName = data[0]["p_Avatar"];
                    int AvatarNo = 1;

                    for (int k = 0; k < allAvatars.Length; k++)
                    {
                        if (allAvatars[k].Equals(avatarName))
                        {
                            AvatarNo = k;
                            break;
                        }
                    }

                    Avatars[AvatarNo].SetActive(true);
                }
            }

            troopsToDeployText.text = troopsToDeploy + "";

            if (playerPhase.Equals("deploy"))
                break;

            yield return new WaitForSeconds(1f);
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

    public void ActivateNeigbhoringTerritories(string territoryName)
    {
        if (!playerPhase.Equals("attack"))
            return;

        if (neigbhorsActive)
        { 
            bool flag = true;

            for (int i = 0; i < AttackingTerritory.Neigbhors.Length; i++)
            {
                if (AttackingTerritory.Neigbhors[i].name.Equals(territoryName))
                {
                    flag = false;
                    break;
                }
            }

            if (flag == false)
                return;
        }

        neigbhorsActive = false;
        AttackingTerritoryName = territoryName;



        for (int i = 0; i < allTerritories.Length; i++) 
        {
            for (int j = 0; j < allTerritories[i].Neigbhors.Length; j++)
                allTerritories[i].Neigbhors[j].SetActive(false);

        }

        for (int i = 0; i < allTerritories.Length; i++)
        {
            if(allTerritories[i].Territory_name.Equals(territoryName))
            {
                AttackingTerritory = allTerritories[i];

                int t = int.Parse(AttackingTerritory.totalTroopsText.text);

                if (t <= 1)
                    return;

                if (AttackingTerritory.CapturedBy != PlayerPrefs.GetInt("id"))
                    return;

                for (int j = 0; j < allTerritories[i].Neigbhors.Length; j++)
                {
                    string name = allTerritories[i].Neigbhors[j].name;

                    MapDataHolder tempDefendingTerritory = null;

                    for (int k = 0; k < allTerritories.Length; k++)
                    {
                        if (allTerritories[k].Territory_name.Equals(name))
                        {
                            tempDefendingTerritory = allTerritories[k];
                            break;
                        }
                    }

                    if (AttackingTerritory.CapturedBy != tempDefendingTerritory.CapturedBy)
                    {
                        allTerritories[i].Neigbhors[j].SetActive(true);

                        neigbhorsActive = true;
                    }
                    else
                        allTerritories[i].Neigbhors[j].SetActive(false);
                }   

                break;
            }
        }
    }

    public void ActivateAttackPanel(string territoryName)
    {
        if (!playerPhase.Equals("attack"))
            return; 

        DefendingTerritoryName = territoryName;

        for (int i = 0; i < allTerritories.Length; i++)
            if (allTerritories[i].Territory_name.Equals(territoryName))
            {
                DefendingTerritory = allTerritories[i];
                break;
            }
                

        bool flag = false;

        for (int i = 0; i < AttackingTerritory.Neigbhors.Length; i++)
            if (AttackingTerritory.Neigbhors[i].name.Equals(territoryName) && AttackingTerritory.CapturedBy != DefendingTerritory.CapturedBy)
                flag = true;

        if (!flag)
        {
            DefendingTerritory.AttackTerritoryImage.SetActive(false);
            return;
        }

        if (!neigbhorsActive)
            return;

        if (territoryName == AttackingTerritoryName)
            return;



        DefendingTerritoryName = territoryName;

        AttackPanel.SetActive(true);
    }

    public void DeactivateAttackPanel()
    {
        neigbhorsActive = false;

        for (int i = 0; i < allTerritories.Length; i++)
        {
            for (int j = 0; j < allTerritories[i].Neigbhors.Length; j++)
            {
                allTerritories[i].Neigbhors[j].SetActive(false);
                allTerritories[i].totalTroopsText.gameObject.SetActive(true);
            }

        }

        AttackPanel.SetActive(false);
    }

    public void NextPhase(Text buttonText)
    {
        if (playerPhase.Equals("deploy"))
        {
            DraftFortifyTroopsSelectionPanel.SetActive(false);

            phaseText.text = "Attack";

            playerPhase = "attack";

            troopsToDeployText.gameObject.SetActive(false);
        }
        else if (playerPhase.Equals("attack"))
        {
            DeactivateAttackPanel();

            phaseText.text = "Fotify";

            playerPhase = "fortify";

            DraftFortifyTroopsSelectionPanel.SetActive(true);

            buttonText.text = "End Turn";
        }
        else if (playerPhase.Equals("fortify"))
        {
            DraftFortifyTroopsSelectionPanel.SetActive(false);

            phaseText.text = "Deploy";

            playerPhase = "ingame";

            buttonText.text = "Next Phase";

            StartCoroutine(tryEndingTurn());

            Invoke("SetDataDeploy", 1f);
        }
        
    }

    public void Attack()
    {
        if (!playerPhase.Equals("attack"))
            return;

        string dice = DiceText.text;

        int AttackTroop, DefendTroop;

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

                for (int i = 0; i < allTerritories.Length; i++)
                {
                    if (allTerritories[i].Territory_name.Equals(AttackingTerritoryName))
                        AttackingTerritory = allTerritories[i];

                    if (allTerritories[i].Territory_name.Equals(DefendingTerritoryName))
                        DefendingTerritory = allTerritories[i];
                }

                // Updating Troops

                if(a > d1)
                {
                    if(a > d2)
                    {
                        DefendingTerritory.totalTroopsText.text = (int.Parse(DefendingTerritory.totalTroopsText.text) - 1) + "";

                        DefendTroop = int.Parse(DefendingTerritory.totalTroopsText.text);

                        if(DefendTroop < 1)
                        {
                            // Calling Capture Api

                            StartCoroutine(tryCaptureTerritoryInAttack(DefendingTerritory.Territory_name, 1));

                            DeactivateAttackPanel();
                        }
                        else
                        {
                            // Calling Attack Api

                            AttackTroop = int.Parse(AttackingTerritory.totalTroopsText.text);
                            StartCoroutine(tryAttackUpdateTroops(AttackingTerritory.Territory_name, AttackTroop));

                            StartCoroutine(tryAttackUpdateTroops(DefendingTerritory.Territory_name, DefendTroop));
                        }

                    }
                    else
                    {
                        AttackingTerritory.totalTroopsText.text = (int.Parse(AttackingTerritory.totalTroopsText.text) - 1) + "";

                        AttackTroop = int.Parse(AttackingTerritory.totalTroopsText.text);
                        StartCoroutine(tryAttackUpdateTroops(AttackingTerritory.Territory_name, AttackTroop));

                        DefendTroop = int.Parse(DefendingTerritory.totalTroopsText.text);
                        StartCoroutine(tryAttackUpdateTroops(DefendingTerritory.Territory_name, DefendTroop));

                    }
                }
                else
                {

                    AttackingTerritory.totalTroopsText.text = (int.Parse(AttackingTerritory.totalTroopsText.text) - 1) + "";

                    AttackTroop = int.Parse(AttackingTerritory.totalTroopsText.text);
                    StartCoroutine(tryAttackUpdateTroops(AttackingTerritory.Territory_name, AttackTroop));

                    DefendTroop = int.Parse(DefendingTerritory.totalTroopsText.text);
                    StartCoroutine(tryAttackUpdateTroops(DefendingTerritory.Territory_name, DefendTroop));
                }

                for (int i = 0; i < allTerritories.Length; i++)
                {
                    for (int j = 0; j < allTerritories[i].Neigbhors.Length; j++)
                        allTerritories[i].Neigbhors[j].SetActive(false);

                }



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

                for (int i = 0; i < allTerritories.Length; i++)
                {
                    if (allTerritories[i].Territory_name.Equals(AttackingTerritoryName))
                        AttackingTerritory = allTerritories[i];

                    if (allTerritories[i].Territory_name.Equals(DefendingTerritoryName))
                        DefendingTerritory = allTerritories[i];
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
                    AttackTroop = 1;
                    AttackingTerritory.totalTroopsText.text = AttackTroop + "";
                    DeactivateAttackPanel();
                }

                if (DefendTroop < 1)
                {
                    // Calling Capture Api
                    StopCoroutine(trySynchMapData());

                    StartCoroutine(tryCaptureTerritoryInAttack(DefendingTerritory.Territory_name, 1));

                    StartCoroutine(trySynchMapData());

                    for (int i = 0; i < allTerritories.Length; i++)
                    {
                        for (int j = 0; j < allTerritories[i].Neigbhors.Length; j++)
                            allTerritories[i].Neigbhors[j].SetActive(false);
                    }

                    DeactivateAttackPanel();
                    break;
                }
                else
                {
                    // Calling Attack Api

                    StopCoroutine(trySynchMapData());

                    StartCoroutine(tryAttackUpdateTroops(AttackingTerritory.Territory_name, AttackTroop));

                    StartCoroutine(tryAttackUpdateTroops(DefendingTerritory.Territory_name, DefendTroop));

                    StartCoroutine(trySynchMapData());
                }

                // Resetting Neigbhorss
                for (int i = 0; i < allTerritories.Length; i++)
                {
                    for (int j = 0; j < allTerritories[i].Neigbhors.Length; j++)
                        allTerritories[i].Neigbhors[j].SetActive(false);

                }


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
                else if(a2 > a && a2 > a3)
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
                    AttackTroop = 1;
                    AttackingTerritory.totalTroopsText.text = AttackTroop + "";
                    DeactivateAttackPanel();
                }

                if (DefendTroop < 1)
                {
                    // Calling Capture Api
                    StopCoroutine(trySynchMapData());

                    StartCoroutine(tryCaptureTerritoryInAttack(DefendingTerritory.Territory_name, 1));

                    StartCoroutine(trySynchMapData());

                    for (int i = 0; i < allTerritories.Length; i++)
                    {
                        for (int j = 0; j < allTerritories[i].Neigbhors.Length; j++)
                            allTerritories[i].Neigbhors[j].SetActive(false);
                    }

                    DeactivateAttackPanel();
                    break;
                }
                else
                {
                    // Calling Attack Api

                    StopCoroutine(trySynchMapData());

                    StartCoroutine(tryAttackUpdateTroops(AttackingTerritory.Territory_name, AttackTroop));

                    StartCoroutine(tryAttackUpdateTroops(DefendingTerritory.Territory_name, DefendTroop));

                    StartCoroutine(trySynchMapData());
                }

                // Resetting Neigbhorss
                for (int i = 0; i < allTerritories.Length; i++)
                {
                    for (int j = 0; j < allTerritories[i].Neigbhors.Length; j++)
                        allTerritories[i].Neigbhors[j].SetActive(false);

                }

                break;
        }
    }

    IEnumerator tryAttackUpdateTroops(string territory,int troops)
    {
        WWWForm form = new WWWForm();

        string roomKey = PlayerPrefs.GetString("roomKey");

        form.AddField("COLOR", roomKey);
        form.AddField("TERRITORY_NAME", territory);
        form.AddField("TOTAL_TROOPS", troops);

        using (UnityWebRequest request = UnityWebRequest.Post(AttackUpdateTroopsURL, form))
        {
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("Error While Attacking Territory ...  ");
            }
            else
            {
                JSONNode data = JSON.Parse(request.downloadHandler.text);
                Debug.Log("Attack Successful " + data[0]);
            }
        }
    }

    IEnumerator tryCaptureTerritoryInAttack(string territory, int troops)
    {
        yield return new WaitForSeconds(1f);

        WWWForm form = new WWWForm();

        string roomKey = PlayerPrefs.GetString("roomKey");

        form.AddField("COLOR", roomKey);
        form.AddField("TERRITORY_ID", PlayerPrefs.GetInt("id"));
        form.AddField("TERRITORY_NAME", territory);
        form.AddField("TOTAL_TROOPS", troops);

        using (UnityWebRequest request = UnityWebRequest.Post(CaptureTerritoryInAttackURL, form))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error While Attacking Territory ...  ");
            }
            else
            {
                JSONNode data = JSON.Parse(request.downloadHandler.text);
                Debug.Log("Capture Successful " + data[0]);
            }
        }
    }

    public void ActivateNeigbhoringTerritoriesFortify(string territoryName)
    {
        if (!playerPhase.Equals("fortify"))
            return;

        if (fortifyNeigbhorsActive)
        {
            bool flag = true;

            for (int i = 0; i < AttackingTerritory.Neigbhors.Length; i++)
            {
                if (AttackingTerritory.Neigbhors[i].name.Equals(territoryName))
                {
                    flag = false;
                    break;
                }
            }

            if (flag == false)
                return;
        }

        fortifyNeigbhorsActive = false;
        AttackingTerritoryName = territoryName;



        for (int i = 0; i < allTerritories.Length; i++)
        {
            for (int j = 0; j < allTerritories[i].Neigbhors.Length; j++)
                allTerritories[i].Neigbhors[j].SetActive(false);

        }

        for (int i = 0; i < allTerritories.Length; i++)
        {
            if (allTerritories[i].Territory_name.Equals(territoryName))
            {
                AttackingTerritory = allTerritories[i];

                UpdateToggle(AttackingTerritory);

                int t = int.Parse(AttackingTerritory.totalTroopsText.text);

                if (t <= 1)
                    return;

                if (AttackingTerritory.CapturedBy != PlayerPrefs.GetInt("id"))
                    return;

                for (int j = 0; j < allTerritories[i].Neigbhors.Length; j++)
                {
                    string name = allTerritories[i].Neigbhors[j].name;

                    MapDataHolder tempDefendingTerritory = null;

                    for (int k = 0; k < allTerritories.Length; k++)
                    {
                        if (allTerritories[k].Territory_name.Equals(name))
                        {
                            tempDefendingTerritory = allTerritories[k];
                            break;
                        }
                    }

                    if (AttackingTerritory.CapturedBy == tempDefendingTerritory.CapturedBy)
                    {
                        allTerritories[i].Neigbhors[j].SetActive(true);

                        fortifyNeigbhorsActive = true;
                    }
                    else
                        allTerritories[i].Neigbhors[j].SetActive(false);
                }

                break;
            }
        }
    }

    public void SendTroops(string territoryName)
    {
        if (!playerPhase.Equals("fortify"))
            return;

        DefendingTerritoryName = territoryName;

        for (int i = 0; i < allTerritories.Length; i++)
            if (allTerritories[i].Territory_name.Equals(territoryName))
            {
                DefendingTerritory = allTerritories[i];
                break;
            }


        bool flag = false;

        for (int i = 0; i < AttackingTerritory.Neigbhors.Length; i++)
            if (AttackingTerritory.Neigbhors[i].name.Equals(territoryName) && AttackingTerritory.CapturedBy == DefendingTerritory.CapturedBy)
                flag = true;

        if (!flag)
        {
            DefendingTerritory.AttackTerritoryImage.SetActive(false);
            return;
        }

        if (!fortifyNeigbhorsActive)
            return;

        if (territoryName == AttackingTerritoryName)
            return;

        for (int i = 0; i < allToggles.Length; i++)
            if (allToggles[i].isOn)
            {
                int troops = int.Parse(allToggles[i].gameObject.name);

                AttackingTerritory.totalTroopsText.text = (int.Parse(AttackingTerritory.totalTroopsText.text) - troops) + "";
                DefendingTerritory.totalTroopsText.text = (int.Parse(DefendingTerritory.totalTroopsText.text) + troops) + "";

                StartCoroutine(tryFortifyingTroops(AttackingTerritory));
                StartCoroutine(tryFortifyingTroops(DefendingTerritory));

                break;
            }

    }

    IEnumerator tryFortifyingTroops(MapDataHolder territory)
    {
        WWWForm form = new WWWForm();

        string roomKey = PlayerPrefs.GetString("roomKey");

        form.AddField("COLOR", roomKey);
        form.AddField("TERRITORY_NAME", territory.Territory_name);
        form.AddField("TOTAL_TROOPS", territory.totalTroopsText.text);

        using (UnityWebRequest request = UnityWebRequest.Post(FortifyTroopsURL, form))
        {
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                
                Debug.Log("Error While Fortifying Territory ...  ");
            }
            else
            {
                JSONNode data = JSON.Parse(request.downloadHandler.text);
                Debug.Log("Territory Fortified ....  ");
            }
        }

        StartCoroutine(tryEndingTurn());
    }

    public void SetDataDeploy()
    {
        StartCoroutine(trySetPlayerDataOnDeploy());
    }

    public void UpdateToggle(MapDataHolder territory)
    {
        if (territory.CapturedBy != PlayerPrefs.GetInt("id"))
            return;

        int troops = int.Parse(territory.totalTroopsText.text);

        if (troops < 3)
        {
            for (int i = 0; i < allToggles.Length; i++)
                allToggles[i].gameObject.SetActive(false);

            allToggles[0].gameObject.SetActive(true);
        }
        if (troops >= 3)
        {
            for (int i = 0; i < allToggles.Length; i++)
                allToggles[i].gameObject.SetActive(false);

            allToggles[0].gameObject.SetActive(true);
            allToggles[1].gameObject.SetActive(true);
        }
        if (troops >= 5 && troops < 10)
        {
            for (int i = 0; i < allToggles.Length; i++)
                allToggles[i].gameObject.SetActive(false);

            allToggles[0].gameObject.SetActive(true);
            allToggles[1].gameObject.SetActive(true);
            allToggles[2].gameObject.SetActive(true);
        }
        if(troops >= 10)
        {
            for (int i = 0; i < allToggles.Length; i++)
                allToggles[i].gameObject.SetActive(false);

            allToggles[0].gameObject.SetActive(true);
            allToggles[1].gameObject.SetActive(true);
            allToggles[2].gameObject.SetActive(true);
            allToggles[3].gameObject.SetActive(true);
        }

    }

    IEnumerator tryEndingTurn()
    {
        WWWForm form = new WWWForm();

        string roomKey = PlayerPrefs.GetString("roomKey");

        form.AddField("ROOM_Key", roomKey);

        using (UnityWebRequest request = UnityWebRequest.Post(EndTurnURL, form))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error While Ending Turn...  ");
            }
            else
            {
                Debug.Log("Turn Changed");
            }
        }
    }
}
