using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Learning_bhv : MonoBehaviour
{    
    // gameobject
    FirstPersonAIO player;
    GameObject text;
    GameObject greenMarker, blueMarker;
    GameObject notifyBgnd, notifyText;
    GameObject marker;
    GameObject target1, target2;
    private GameObject playerObject;
    GameObject goodmush, badmush;
    private string guiMessage = "";
    private string response_time = "";

    // feedback information
    private float error = 0f; // error distance
    private static Vector3 response_loc;
    public int response_idx = 0;
    private GUIStyle guiStyle;
    public static bool showFeedback = false;
    private bool showGUI = false;
    private bool sceneReady = true;
    private bool writingtime = false;
    private bool isBirdEyeViewActive = false;  // Add new flag for birdeye view
    private int labelIdx = 0;

    private static Vector3 correct_loc = Vector3.zero; // present 위치값
    
    public Camera spectatorCamera;
    private GameObject feedback_res, feedback_ans, response_mt;
    private GameObject mountainObj;
    private Texture2D goodmushTex, badmushTex;
    private Texture2D image1, image2; // 추가: 이미지 텍스처 변수
    private enum MushType { none, good, bad }
    private MushType currMush = MushType.none;

    // directory
    private string ori_dir, bhv1_dir, bhv2_dir, time_dir, coin1_dir, coin2_dir, coin3_dir, tmpDate;
    
    void Awake()
    {
        string ori_dir = "./output/";
        string tmpDate = System.DateTime.Now.ToString("yyyyMMdd");
        string filenamePrefix = $"{tmpDate}_sbj{SbjManager.sbj_num}";
        SbjManager.FileLogger(ori_dir, filenamePrefix);

        spectatorCamera.enabled = false;
        playerObject = GameObject.FindWithTag("Player");
        player = playerObject.GetComponent<FirstPersonAIO>();
    }

    void Start() // +수정: 모든 task phase에 범용되게끔 수정하기
    {
        // scene setting
        text = GameObject.Find("Text");
        greenMarker = GameObject.Find("GreenMarker");
        blueMarker = GameObject.Find("BlueMarker");
        notifyBgnd = GameObject.Find("NotifyBgnd");
        notifyText = GameObject.Find("NotifyText");
        marker = GameObject.Find("Marker");
        target1 = GameObject.Find("target1"); // 큰토끼(first run)
        target2 = GameObject.Find("target2");
        feedback_res = GameObject.Find("feedback_res");
        feedback_ans = GameObject.Find("feedback_ans");
        response_mt = GameObject.Find("feedback_mt");
        goodmush = GameObject.Find("goodmush");
        badmush = GameObject.Find("badmush");

        marker.SetActive(false);
        greenMarker.SetActive(false);
        blueMarker.SetActive(false);
        notifyBgnd.SetActive(false);
        notifyText.SetActive(false);
        target1.SetActive(false);
        target2.SetActive(false);
        goodmush.SetActive(false);
        badmush.SetActive(false);
        response_mt.SetActive(false);
        feedback_res.SetActive(false);
        feedback_ans.SetActive(false);

        player.GetComponent<FirstPersonAIO>().playerCanMove = true;
        player.GetComponent<FirstPersonAIO>().useHeadbob = false;
        player.GetComponent<FirstPersonAIO>().canJump = false;
        player.GetComponent<FirstPersonAIO>().useStamina = false;
        player.GetComponent<AudioSource>().enabled = false;
        player.GetComponent<FirstPersonAIO>().enableCameraShake = false;
        player.GetComponent<FirstPersonAIO>().cameraSmoothing = 10f;
        image1 = Resources.Load<Texture2D>("tomato1"); // 추가: 이미지 로드
        image2 = Resources.Load<Texture2D>("tomato2"); // 추가: 이미지 로드
        writingtime = true;
        StartCoroutine(SceneSetting()); // 씬 로드 시 Present 태그의 오브젝트 위치를 업데이트
        StartCoroutine(LogPlayerPosition());
    }

    IEnumerator SceneSetting()
    {
        yield return new WaitForSeconds(2f);
        SbjManager.showFixation = false; 
        
        SbjManager.target_idx = SbjManager.response_order[SbjManager.curr_trial - 1];
        showGUI = true;
        string[] nutrientLabels = { "영양분이 훨씬 많은 위치", "영양분이 있는 위치" };
        string prefix = "먼저, 다음 환경에서\n";

        // target_idx가 0이면 nutrientLabels 그대로, 1이면 순서 반전
        labelIdx = (SbjManager.target_idx == 0) ? response_idx : 1 - response_idx;

        guiMessage = prefix + nutrientLabels[labelIdx] + "를 찾아주세요.";

        // guiMessage = "First, find the location of the edible";
        
        yield return new WaitForSeconds(6f);
        SbjManager.showFixation = true;
        yield return new WaitForSeconds(4f);
        showGUI = false;
        SbjManager.showFixation = false;

        SetPlayerPos(playerObject);
        // player.transform.position = new Vector3(0f,0f,0f);
        SbjManager.start_time = System.DateTime.Now.ToString("HH:mm:ss.fff");
        SbjManager.start_rot = playerObject.transform.rotation.eulerAngles;
        // sceneReady = true;

    }

    void Update()
    {
        if (SbjManager.showFixation || showGUI || isBirdEyeViewActive)  // Add isBirdEyeViewActive check
            return;
        if (Input.GetKeyDown(KeyCode.Alpha4) && response_idx < 2)
        {
            StartCoroutine(DetectKey());
        }
    }
    
    IEnumerator LogPlayerPosition()
    {
        while (writingtime)   
        {
            Vector3 pos = player.transform.position;
            string time = System.DateTime.Now.ToString("HH:mm:ss.fff");

            SbjManager.timeWriter.WriteLine($"{SbjManager.curr_task} {pos} {time}");
            SbjManager.timeWriter.Flush(); 
            yield return new WaitForSeconds(0.5f);
        }
    }
    IEnumerator DetectKey()
    {
        Camera playerCamera = player.GetComponentInChildren<Camera>();
        spectatorCamera.transform.position = playerCamera.transform.position;
        spectatorCamera.transform.rotation = playerCamera.transform.rotation;

        // 카메라 설정 동기화
        spectatorCamera.clearFlags = playerCamera.clearFlags;
        spectatorCamera.backgroundColor = playerCamera.backgroundColor;
        spectatorCamera.cullingMask = playerCamera.cullingMask;
        spectatorCamera.renderingPath = playerCamera.renderingPath;
        spectatorCamera.allowHDR = playerCamera.allowHDR;
        spectatorCamera.allowMSAA = playerCamera.allowMSAA;

        // 공통 정보 기록
        GameObject chosenMush;

        // 어떤 버섯을 보여줄 것인지 선택
        if ((SbjManager.target_idx == 0 && response_idx == 0) || (SbjManager.target_idx == 1 && response_idx == 1))
        {
            chosenMush = goodmush;
        }
        else
        {
            chosenMush = badmush;
        }

        // 버섯 표시 및 위치 설정
        chosenMush.SetActive(true);
        chosenMush.transform.position = new Vector3(player.transform.position.x, 0f, player.transform.position.z) + player.transform.forward * 3;
        Quaternion playerRotation = player.transform.rotation;
        Quaternion offsetRotation = Quaternion.Euler(0, 150f, 0);
        Quaternion targetRotation = playerRotation * offsetRotation;
        chosenMush.transform.rotation = targetRotation;
        response_loc = chosenMush.transform.position;
        response_time = System.DateTime.Now.ToString("HH:mm:ss.fff");

        // 피드백 카메라 전환
        spectatorCamera.enabled = true;
        playerCamera.enabled = false;
        player.enabled = false;
        yield return new WaitForSeconds(2f);

        // 버섯 숨김
        chosenMush.SetActive(false);

        // 피드백 표시
        showFeedback = true;
        feedback_res.SetActive(true);
        response_mt.SetActive(true);
        StartCoroutine(birdeye());

        error = Vector3.Distance(response_loc, correct_loc);
        SbjManager.dist_error.Add(error);
        yield return new WaitForSeconds(4f);

        // 피드백 종료
        showFeedback = false;
        feedback_res.SetActive(false);
        feedback_ans.SetActive(false);
        response_mt.SetActive(false);
        spectatorCamera.enabled = false;
        playerCamera.enabled = true;
        player.enabled = true;
        writingtime = true;

        if (response_idx >= 1)
        {
            SbjManager.showFixation = true;
            writingtime = false;
            FindObjectOfType<RemapManager_bhv>()?.OnSceneCompleted();
        }
        
        // 로그 기록
        string log = $"{SbjManager.start_time}s {SbjManager.start_loc} {SbjManager.start_rot} {response_time} {SbjManager.curr_task} {SbjManager.curr_scene} {SbjManager.curr_run}_{SbjManager.curr_trial} {response_loc} {correct_loc}";
        if (SbjManager.curr_task == "learning")
            SbjManager.writer1.WriteLine(log);
        else
            SbjManager.writer2.WriteLine(log);
        SbjManager.writer1?.Flush();
        SbjManager.writer2?.Flush();
        
        showGUI = true;
        string[] nutrientLabels = { "영양분이 훨씬 많은 위치", "영양분이 있는 위치" };
        string prefix = "이번에는 \n";  // 항상 고정
        labelIdx = (SbjManager.target_idx == 0) ? 1 - response_idx : response_idx;
        guiMessage = prefix + nutrientLabels[labelIdx] + "를 찾아주세요.";

        SetPlayerPos(playerObject);
        yield return new WaitForSeconds(6f);
        
        SbjManager.showFixation = true;
        yield return new WaitForSeconds(4f);
        showGUI = false;
        SbjManager.showFixation = false;

        response_idx++;
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

            // 이미지 표시
            if (image1 != null && image2 != null)
            {
                Texture2D selectedImage = (labelIdx == 0) ? image1 : image2;
                float imageWidth = 150f; // 이미지 너비
                float imageHeight = 150f; // 이미지 높이
                float imageX = (Screen.width - imageWidth) / 2; // 화면 중앙
                float imageY = Screen.height - imageHeight - 50f; // 화면 하단에서 50픽셀 위

                GUI.DrawTexture(new Rect(imageX, imageY, imageWidth, imageHeight), selectedImage);
            }
        }
    }
    
    void SetPlayerPos(GameObject playerObject, float minDistance = 3f)
    {
        Vector3 randomPosition = Vector3.zero;
        int maxAttempts = 100;
        int attempts = 0;
        bool found = false;

        while (attempts < maxAttempts)
        {
            // x, z값을 ±10 범위 내에서 랜덤하게 선택
            float randomX = Random.Range(-9f, 9f);
            float randomZ = Random.Range(-9f, 9f);
            randomPosition = new Vector3(randomX, 0.91f, randomZ);

            bool tooClose = false;

            // 타겟과의 거리 체크
            float distance1 = Vector3.Distance(
                new Vector3(randomPosition.x, 0f, randomPosition.z),
                new Vector3(SbjManager.context_target[0, 0].x, 0f, SbjManager.context_target[0, 0].z)
            );
            float distance2 = Vector3.Distance(
                new Vector3(randomPosition.x, 0f, randomPosition.z),
                new Vector3(SbjManager.context_target[1, 0].x, 0f, SbjManager.context_target[1, 0].z)
            );

            if (distance1 < 7f || distance2 < 7f)
            {
                tooClose = true;
            }

            if (!tooClose)
            {
                found = true;
                break;
            }

            attempts++;
        }

        if (!found)
        {
            // 적절한 위치를 찾지 못한 경우 기본 위치 설정
            randomPosition = new Vector3(0f, 0.91f, -8f);
        }

        // 플레이어 위치 설정
        playerObject.transform.position = randomPosition;
        SbjManager.start_loc = playerObject.transform.position;
    }


    IEnumerator birdeye()
    {
        isBirdEyeViewActive = true;

        while (showFeedback)
        {
            feedback_res.transform.position = response_loc;

            // === 핵심 인덱스 추출 ===
            int order_idx = SbjManager.curr_order;
            Debug.Log($"order_idx: {order_idx}");
            int fam_idx = SbjManager.sbj_fam_order[0];
            Debug.Log($"fam_idx: {fam_idx}");
            int curr_run = SbjManager.curr_run;
            
            // === 피드백 위치 설정 ===
            feedback_ans.SetActive(true);
            
            // fam_idx와 order_idx가 같은 경우
            if (fam_idx == order_idx)
            {
                if (curr_run == 0) // [0, 1] -> [0, 1]
                {
                    if (response_idx == 0 && SbjManager.target_idx == 0)
                        feedback_ans.transform.position = SbjManager.context_target[0, 0]; // good
                    else if (response_idx == 1 && SbjManager.target_idx == 0)
                        feedback_ans.transform.position = SbjManager.context_target[0, 1]; // bad
                    else if (response_idx == 0 && SbjManager.target_idx == 1)
                        feedback_ans.transform.position = SbjManager.context_target[0, 1]; // bad
                    else if (response_idx == 1 && SbjManager.target_idx == 1)
                        feedback_ans.transform.position = SbjManager.context_target[0, 0]; // good
                }
                else // curr_run == 1  // [0, 1] -> [0, 1]
                {
                    if (response_idx == 0 && SbjManager.target_idx == 0)
                        feedback_ans.transform.position = SbjManager.context_target[0, 1]; // good
                    else if (response_idx == 1 && SbjManager.target_idx == 0)
                        feedback_ans.transform.position = SbjManager.context_target[0, 0]; // bad
                    else if (response_idx == 0 && SbjManager.target_idx == 1)
                        feedback_ans.transform.position = SbjManager.context_target[0, 0]; // bad
                    else if (response_idx == 1 && SbjManager.target_idx == 1)
                        feedback_ans.transform.position = SbjManager.context_target[0, 1]; // good
                }
            }
            // fam_idx와 order_idx가 다른 경우
            else
            {
                if (curr_run == 0) // [0, 1] -> [1, 0]
                {
                    if (response_idx == 0 && SbjManager.target_idx == 0)
                        feedback_ans.transform.position = SbjManager.context_target[1, 0]; // good
                    else if (response_idx == 1 && SbjManager.target_idx == 0)
                        feedback_ans.transform.position = SbjManager.context_target[1, 1]; // bad
                    else if (response_idx == 0 && SbjManager.target_idx == 1)
                        feedback_ans.transform.position = SbjManager.context_target[1, 1]; // bad
                    else if (response_idx == 1 && SbjManager.target_idx == 1)
                        feedback_ans.transform.position = SbjManager.context_target[1, 0]; // good
                }
                else // curr_run == 1
                {
                    if (response_idx == 0 && SbjManager.target_idx == 0)
                        feedback_ans.transform.position = SbjManager.context_target[1, 1]; // good
                    else if (response_idx == 1 && SbjManager.target_idx == 0)
                        feedback_ans.transform.position = SbjManager.context_target[1, 0]; // bad
                    else if (response_idx == 0 && SbjManager.target_idx == 1)
                        feedback_ans.transform.position = SbjManager.context_target[1, 0]; // bad
                    else if (response_idx == 1 && SbjManager.target_idx == 1)
                        feedback_ans.transform.position = SbjManager.context_target[1, 1]; // good
                }
            }

            correct_loc = feedback_ans.transform.position;

            // === 카메라 설정 ===
            float camHeight = (curr_run == 0) ? 40f : 50f;
            spectatorCamera.transform.position = new Vector3(0f, camHeight, 0f);
            spectatorCamera.transform.rotation = Quaternion.Euler(90f, -180f, 0f);

            yield return null;
        }

        isBirdEyeViewActive = false;
    }

    void OnDestroy()
    {
        SbjManager.CloseLogger();
    }
}