using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Coin_bhv : MonoBehaviour 
{
    // gameObject
    GameObject player, coin, text, flower;
    GameObject greenMarker, blueMarker;
    GameObject notifyBgnd, notifyText;
    GameObject marker;
    GameObject notifier;
    Camera secondcam;
    private Vector3 coinRot;
    public GameObject seed, target;

    // flag
    bool isCoinFound = false;
    float locErrBound;
    private string guiMessage = "";
    private bool showGUI = false;
    private int trialIdx;

    // directory
    
    private string ori_dir, bhv1_dir, bhv2_dir, time_dir, coin1_dir, coin2_dir, coin3_dir, tmpDate;
    
    void Awake()
    {
        string ori_dir = "./output/";
        string tmpDate = System.DateTime.Now.ToString("yyyyMMdd");
        string filenamePrefix = $"{tmpDate}_sbj{SbjManager.sbj_num}";
        SbjManager.FileLogger(ori_dir, filenamePrefix);
    }

    void Start() 
    {
        player = GameObject.Find("FirstPerson-AIO");
        coin = GameObject.Find("coin");
        locErrBound = coin.GetComponent<Collider>().bounds.size.y + 0.3f;
        text = GameObject.Find("Text");      
        greenMarker = GameObject.Find("GreenMarker");
        blueMarker = GameObject.Find("BlueMarker");
        notifyBgnd = GameObject.Find("NotifyBgnd");
        notifyText = GameObject.Find("NotifyText");
        marker = GameObject.Find("Marker");
        notifier = GameObject.Find("Notifier");
        secondcam = GameObject.Find("Camera").GetComponent<Camera>();
        seed = GameObject.Find("Seed");
        target = GameObject.Find("Grass");

        marker.SetActive(false);
        secondcam.gameObject.SetActive(false);
        greenMarker.SetActive(false);
        blueMarker.SetActive(false);
        notifyBgnd.SetActive(false);
        notifyText.SetActive(false);
        // seed.SetActive(false);
        coin.transform.localScale = new Vector3(1f, 0.6f, 1f);
        coin.SetActive(true);
        // target.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);

        // scene setting 
        player.GetComponent<FirstPersonAIO>().playerCanMove = true;
        player.GetComponent<FirstPersonAIO>().useHeadbob = false;
        player.GetComponent<FirstPersonAIO>().canJump = false;
        player.GetComponent<FirstPersonAIO>().useStamina = false;
        player.GetComponent<AudioSource>().enabled = false;
        player.GetComponent<FirstPersonAIO>().enableCameraShake = false;
        player.GetComponent<FirstPersonAIO>().cameraSmoothing = 10f;
        
        SbjManager.start_time = System.DateTime.Now.ToString("HH:mm:ss.fff");
        SbjManager.start_rot = player.transform.rotation.eulerAngles;

        StartCoroutine(placeCoin());
        StartCoroutine(LogPlayerPosition());
    }

    void Update() 
    {
        if (isCoinFound)
        {   
            seed.SetActive(true);
            seed.transform.localScale = new Vector3(1f, 1f, 1f);
            
            Camera mainCamera = Camera.main;
            Transform cameraTransform = mainCamera.transform;
            
            Vector3 forwardPosition = cameraTransform.position + cameraTransform.forward * 1f;
            seed.transform.position = new Vector3(forwardPosition.x, 1.4f, forwardPosition.z);
            
            // 씨앗이 항상 플레이어를 바라보도록 회전 설정
            Vector3 directionToCamera = cameraTransform.position - seed.transform.position;
            directionToCamera.y = 0; // Y축 회전만 고려
            seed.transform.rotation = Quaternion.LookRotation(directionToCamera);
        }
        else
        {
            seed.SetActive(false);
        }
    }                       

    IEnumerator LogPlayerPosition()
    {
        while (true)
        {
            Vector3 pos = player.transform.position;
            string time = System.DateTime.Now.ToString("HH:mm:ss.fff");

            SbjManager.timeWriter.WriteLine($"{SbjManager.curr_task} {pos} {time}");
            SbjManager.timeWriter.Flush(); // 파일에 바로 쓰도록 플러시

            yield return new WaitForSeconds(0.5f); // 0.5초마다 반복
        }
    }
    bool IsPlayerLookingAtCoin(float maxDistance, float viewAngleThreshold)
    {
        Vector3 dirToCoin = coin.transform.position - player.transform.position;
        dirToCoin.y = 0f;

        float distance = dirToCoin.magnitude;
        if (distance > maxDistance) return false;

        Vector3 playerForward = player.transform.forward;
        playerForward.y = 0f;

        float angle = Vector3.Angle(playerForward, dirToCoin);
        return angle < viewAngleThreshold;
    }

    IEnumerator placeCoin() 
    {
        yield return new WaitForSeconds(4f);
        SbjManager.showFixation = false;
        FirstPersonAIO playerController = player.GetComponent<FirstPersonAIO>();
        // player starting position
        player.transform.position = SbjManager.curr_player_start;

        // player not facing to coin
        Vector3 dirToCoin = SbjManager.coinLoc[0] - player.transform.position;
        float distanceToCoin = dirToCoin.magnitude;
        
        Camera mainCamera = Camera.main;
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(SbjManager.coinLoc[0]);
        bool isCoinInView = viewportPoint.x >= 0 && viewportPoint.x <= 1 && 
                           viewportPoint.y >= 0 && viewportPoint.y <= 1 && 
                           viewportPoint.z > 0;
                           
        
        player.transform.rotation = Quaternion.Euler(0, player.transform.rotation.eulerAngles.y + 180f, 0);

        for (trialIdx = 0; trialIdx < SbjManager.numTrial; trialIdx++) 
        {
            coin.SetActive(false);
            yield return new WaitForSeconds(2f);
            coin.SetActive(true);
            
            text.GetComponent<Text>().text = " ";
            coin.transform.position = SbjManager.coinLoc[trialIdx];
        
            yield return new WaitUntil(() => IsPlayerLookingAtCoin(locErrBound, 40f)); // 씨드를 감지하려면 화면안에 있어야함
            
            // log file
            if (SbjManager.curr_coin_sess == 0)
            { 
            SbjManager.coinWriter1.WriteLine($"{SbjManager.start_time}s {SbjManager.curr_player_start} {SbjManager.start_rot} {System.DateTime.Now.ToString("HH:mm:ss.fff")} {SbjManager.curr_task} {SbjManager.curr_scene} {SbjManager.curr_coin_sess}_{SbjManager.curr_coin_run}_{SbjManager.curr_coin_trial} {coin.transform.position} [{string.Join(", ", SbjManager.curr_coin_seq)}]");
            SbjManager.coinWriter1.Flush();
            }
            else if (SbjManager.curr_coin_sess == 1)
            {
            SbjManager.coinWriter2.WriteLine($"{SbjManager.start_time}s {SbjManager.curr_player_start} {SbjManager.start_rot} {System.DateTime.Now.ToString("HH:mm:ss.fff")} {SbjManager.curr_task} {SbjManager.curr_scene} {SbjManager.curr_coin_sess}_{SbjManager.curr_coin_run}_{SbjManager.curr_coin_trial} {coin.transform.position} [{string.Join(", ", SbjManager.curr_coin_seq)}]");
            SbjManager.coinWriter2.Flush();
            }
            else if (SbjManager.curr_coin_sess == 2)
            {
            SbjManager.coinWriter3.WriteLine($"{SbjManager.start_time}s {SbjManager.curr_player_start} {SbjManager.start_rot} {System.DateTime.Now.ToString("HH:mm:ss.fff")} {SbjManager.curr_task} {SbjManager.curr_scene} {SbjManager.curr_coin_sess}_{SbjManager.curr_coin_run}_{SbjManager.curr_coin_trial} {coin.transform.position} [{string.Join(", ", SbjManager.curr_coin_seq)}]");
            SbjManager.coinWriter3.Flush();
            }

            isCoinFound = true;
            playerController.playerCanMove = false;
            yield return new WaitForSeconds(1f);
            isCoinFound = false;
            if (trialIdx != SbjManager.numTrial - 1)
            {
                playerController.playerCanMove = true;
            }

        }
        SbjManager.showFixation = true; 
        // FindObjectOfType<RemapManager>()?.OnSceneCompleted();
        FindObjectOfType<RemapManager_bhv>()?.OnSceneCompleted();
        // FindObjectOfType<RemapManager_eng>()?.OnSceneCompleted();
    }
     
    void OnGUI()
    {
        if (showGUI)
        {
            Texture2D blackTexture = new Texture2D(1, 1);
            blackTexture.SetPixel(0, 0, new Color32(217, 217, 217, 255));
            blackTexture.Apply();
            GUIStyle bgStyle = new GUIStyle();
            bgStyle.normal.background = blackTexture;

            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, bgStyle);

            GUIStyle guiStyle = new GUIStyle();
            guiStyle.fontSize = 90;
            guiStyle.fontStyle = FontStyle.Bold;
            guiStyle.normal.textColor = Color.black;
            guiStyle.alignment = TextAnchor.MiddleCenter; 
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 200, 100), guiMessage, guiStyle);
        }
    }

    void OnDestroy()
    {
        SbjManager.CloseLogger();
    }
}