// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.SceneManagement;

// public class LearningScript : MonoBehaviour
// {    
//     // gameobject
//     FirstPersonAIO player;
//     GameObject text;
//     GameObject greenMarker, blueMarker;
//     GameObject notifyBgnd, notifyText;
//     GameObject marker;
//     GameObject target1, target2;
//     private GameObject playerObject;
//     GameObject goodmush, badmush;
//     private string guiMessage = "";
//     private string response_time = "";

//     // feedback information
//     private float error = 0f; // error distance
//     private static Vector3 response_loc;
//     public int response_idx = 0;
//     private GUIStyle guiStyle;
//     private bool showFeedback = false;
//     private bool showGUI = false;
//     private bool sceneReady = true;
//     private bool writingtime = false;
//     private bool isBirdEyeViewActive = false;  // Add new flag for birdeye view

//     private static Vector3 correct_loc = Vector3.zero; // present 위치값
    
//     public Camera spectatorCamera;
//     private GameObject feedback_res, feedback_ans, response_mt;
//     private GameObject mountainObj;
//     private Texture2D goodmushTex, badmushTex;
//     private enum MushType { none, good, bad }
//     private MushType currMush = MushType.none;

//     // directory
//     private string ori_dir, bhv1_dir, bhv2_dir, time_dir, coin1_dir, coin2_dir, coin3_dir, tmpDate;
    
//     void Awake()
//     {
//         string ori_dir = "./output/";
//         string tmpDate = System.DateTime.Now.ToString("yyyyMMdd");
//         string filenamePrefix = $"{tmpDate}_sbj{SbjManager.sbj_num}";
//         SbjManager.FileLogger(ori_dir, filenamePrefix);

//         spectatorCamera.enabled = false;
//         playerObject = GameObject.FindWithTag("Player");
//         player = playerObject.GetComponent<FirstPersonAIO>();
//     }

//     void Start() // +수정: 모든 task phase에 범용되게끔 수정하기
//     {
//         // scene setting
//         text = GameObject.Find("Text");
//         greenMarker = GameObject.Find("GreenMarker");
//         blueMarker = GameObject.Find("BlueMarker");
//         notifyBgnd = GameObject.Find("NotifyBgnd");
//         notifyText = GameObject.Find("NotifyText");
//         marker = GameObject.Find("Marker");
//         target1 = GameObject.Find("target1"); // 큰토끼(first run)
//         target2 = GameObject.Find("target2");
//         feedback_res = GameObject.Find("feedback_res");
//         feedback_ans = GameObject.Find("feedback_ans");
//         response_mt = GameObject.Find("feedback_mt");
//         goodmush = GameObject.Find("goodmush");
//         badmush = GameObject.Find("badmush");

//         marker.SetActive(false);
//         greenMarker.SetActive(false);
//         blueMarker.SetActive(false);
//         notifyBgnd.SetActive(false);
//         notifyText.SetActive(false);
//         target1.SetActive(false);
//         target2.SetActive(false);
//         goodmush.SetActive(false);
//         badmush.SetActive(false);
//         response_mt.SetActive(false);
//         feedback_res.SetActive(false);
//         feedback_ans.SetActive(false);

//         player.GetComponent<FirstPersonAIO>().playerCanMove = true;
//         player.GetComponent<FirstPersonAIO>().useHeadbob = false;
//         player.GetComponent<FirstPersonAIO>().canJump = false;
//         player.GetComponent<FirstPersonAIO>().useStamina = false;
//         player.GetComponent<AudioSource>().enabled = false;
//         player.GetComponent<FirstPersonAIO>().enableCameraShake = false;
//         player.GetComponent<FirstPersonAIO>().cameraSmoothing = 10f;

//         goodmushTex = Resources.Load<Texture2D>("mushroom/goodmush");
//         badmushTex = Resources.Load<Texture2D>("mushroom/badmush");
//         writingtime = true;
//         StartCoroutine(SceneSetting()); // 씬 로드 시 Present 태그의 오브젝트 위치를 업데이트
//         StartCoroutine(LogPlayerPosition());
        
//         // target location

//         // if (SbjManager.curr_run == 0) // 1st learning run // 잘못된 코드인지 확인
//         // {
//         //     if (SbjManager.sbj_fam_order[0] == SbjManager.sbj_learn_order[0])
//         //     {
//         //         target1.transform.position = SbjManager.context_target[SbjManager.curr_order, 0];
//         //         target2.transform.position = SbjManager.context_target[SbjManager.curr_order, 1];
//         //     }
//         //     else if (SbjManager.sbj_fam_order[0] == SbjManager.sbj_learn_order[1])
//         //     {
//         //         target1.transform.position = SbjManager.context_target[1 - SbjManager.curr_order, 0];
//         //         target2.transform.position = SbjManager.context_target[1 - SbjManager.curr_order, 1];
//         //     }
//         // }
//         // else if (SbjManager.curr_run == 1) // reverse run
//         // {
//         //     if (SbjManager.sbj_fam_order[0] == SbjManager.sbj_learn_order[0])
//         //     {
//         //         target1.transform.position = SbjManager.context_target[SbjManager.curr_order, 1];
//         //         target2.transform.position = SbjManager.context_target[SbjManager.curr_order, 0];
//         //     }
//         //     else if (SbjManager.sbj_fam_order[0] == SbjManager.sbj_learn_order[1])
//         //     {
//         //         target1.transform.position = SbjManager.context_target[1 - SbjManager.curr_order, 1];
//         //         target2.transform.position = SbjManager.context_target[1 - SbjManager.curr_order, 0];
//         //     }
//         // }
//     }

//     IEnumerator SceneSetting()
//     {
//         yield return new WaitForSeconds(6f);
//         SbjManager.showFixation = false; RemapManager.fmriWriter.WriteLine($"fix_end" + "\t" + RemapManager.fmriTimer.ToString());
//         RemapManager.fmriWriter.WriteLine($"scene_started" + "\t" + RemapManager.fmriTimer.ToString());
//         RemapManager.fmriWriter.Flush();
        
//         SbjManager.target_idx = SbjManager.response_order[SbjManager.curr_trial - 1];
//         showGUI = true; RemapManager.fmriWriter.WriteLine($"ins_start" + "\t" + RemapManager.fmriTimer.ToString());
//         string[] nutrientLabels = { "영양분이 훨씬 많은 위치", "영양분이 있는 위치" };
//         string prefix = (response_idx == 0) ? "먼저, 다음 환경에서\n" : "이번에는 \n";

//         // target_idx가 0이면 nutrientLabels 그대로, 1이면 순서 반전
//         int labelIdx = (SbjManager.target_idx == 0) ? response_idx : 1 - response_idx;

//         guiMessage = prefix + nutrientLabels[labelIdx] + "를 찾아주세요.";

//         // guiMessage = "First, find the location of the edible";
        
//         SetPlayerPos(playerObject);
//         yield return new WaitForSeconds(6f);
//         showGUI = false; RemapManager.fmriWriter.WriteLine($"ins_end" + "\t" + RemapManager.fmriTimer.ToString());

//         // player.transform.position = new Vector3(0f,0f,0f);
//         SbjManager.start_time = System.DateTime.Now.ToString("HH:mm:ss.fff");
//         SbjManager.start_rot = playerObject.transform.rotation.eulerAngles;
//         // sceneReady = true;

//     }

//     void Update()
//     {
//         if (SbjManager.showFixation || showGUI || isBirdEyeViewActive)  // Add isBirdEyeViewActive check
//             return;
//         if (Input.GetKeyDown(KeyCode.Alpha4) && response_idx < 2)
//         {
//             RemapManager.fmriWriter.WriteLine($"resp_learn" + "\t" + RemapManager.fmriTimer.ToString());
//             StartCoroutine(DetectKey());
//         }
//     }
//     IEnumerator LogPlayerPosition()
//     {
//         while (writingtime)   
//         {
//             Vector3 pos = player.transform.position;
//             string time = System.DateTime.Now.ToString("HH:mm:ss.fff");

//             SbjManager.timeWriter.WriteLine($"{SbjManager.curr_task} {pos} {time}");
//             SbjManager.timeWriter.Flush(); // 파일에 바로 쓰도록 플러시

//             yield return new WaitForSeconds(0.5f); // 0.5초마다 반복
//         }
//     }
//     IEnumerator DetectKey()
//     {
//         Camera playerCamera = player.GetComponentInChildren<Camera>();
//         spectatorCamera.transform.position = playerCamera.transform.position;
//         spectatorCamera.transform.rotation = playerCamera.transform.rotation;

//         // 공통 정보 기록
//         writingtime = false;
//         GameObject chosenMush;
        
//         // 어떤 버섯을 보여줄 것인지 선택
//         if ((SbjManager.target_idx == 0 && response_idx == 0) || (SbjManager.target_idx == 1 && response_idx == 1))
//         {
//             chosenMush = goodmush;
//         }
//         else
//         {
//             chosenMush = badmush;
//         }

//         // 버섯 표시 및 위치 설정
//         chosenMush.SetActive(true);
//         chosenMush.transform.position = new Vector3(player.transform.position.x, 0f, player.transform.position.z) + player.transform.forward * 3;
//         Quaternion playerRotation = player.transform.rotation;
//         Quaternion offsetRotation = Quaternion.Euler(0, 150f, 0);
//         Quaternion targetRotation = playerRotation * offsetRotation;
//         chosenMush.transform.rotation = targetRotation;
//         response_loc = chosenMush.transform.position;
//         response_time = System.DateTime.Now.ToString("HH:mm:ss.fff");

//         // 피드백 카메라 전환
//         spectatorCamera.enabled = true;
//         playerCamera.enabled = false;
//         player.enabled = false;
//         yield return new WaitForSeconds(2f);

//         // 버섯 숨김
//         chosenMush.SetActive(false);

//         // 피드백 표시
//         showFeedback = true;
//         RemapManager.fmriWriter.WriteLine($"fb_start" + "\t" + RemapManager.fmriTimer.ToString());
//         RemapManager.fmriWriter.Flush();
//         feedback_res.SetActive(true);
//         response_mt.SetActive(true);
//         StartCoroutine(birdeye());

//         error = Vector3.Distance(response_loc, correct_loc);
//         SbjManager.dist_error.Add(error);
//         yield return new WaitForSeconds(4f);

//         // 피드백 종료
//         showFeedback = false;
//         RemapManager.fmriWriter.WriteLine($"fb_end" + "\t" + RemapManager.fmriTimer.ToString());
//         RemapManager.fmriWriter.Flush();
//         feedback_res.SetActive(false);
//         feedback_ans.SetActive(false);
//         response_mt.SetActive(false);
//         spectatorCamera.enabled = false;
//         playerCamera.enabled = true;
//         player.enabled = true;
//         writingtime = true;

//         // 로그 기록
//         string log = $"{SbjManager.start_time}s {SbjManager.start_loc} {SbjManager.start_rot} {response_time} {SbjManager.curr_task} {SbjManager.curr_scene} {SbjManager.curr_run}_{SbjManager.curr_trial} {response_loc} {correct_loc}";
//         if (SbjManager.curr_task == "learning")
//             SbjManager.writer1.WriteLine(log);
//         else
//             SbjManager.writer2.WriteLine(log);
//         SbjManager.writer1?.Flush();
//         SbjManager.writer2?.Flush();

//             showGUI = true;
//             RemapManager.fmriWriter.WriteLine($"ins_start" + "\t" + RemapManager.fmriTimer.ToString());
//             RemapManager.fmriWriter.Flush();
//             string[] nutrientLabels = { "영양분이 훨씬 많은 위치", "영양분이 있는 위치" };
//             string prefix = "이번에는 \n";  // 항상 고정
//             int labelIdx = (SbjManager.target_idx == 0) ? response_idx : 1 - response_idx;
//             guiMessage = prefix + nutrientLabels[labelIdx] + "를 찾아주세요.";

//             SetPlayerPos(playerObject);
//             yield return new WaitForSeconds(6f);
//             showGUI = false;

//             showGUI = true;
//             guiMessage = " ";
//             yield return new WaitForSeconds(0.2f);
//             showGUI = false;
//             RemapManager.fmriWriter.WriteLine($"ins_end" + "\t" + RemapManager.fmriTimer.ToString());
//             RemapManager.fmriWriter.Flush();

//         // 두 번째 응답까지 완료되면 종료
//         if (response_idx >= 2)
//         {
//             SbjManager.showFixation = true;
//             RemapManager.fmriWriter.WriteLine($"fix_start" + "\t" + RemapManager.fmriTimer.ToString());
//             RemapManager.fmriWriter.Flush();
//             RemapManager.fmriWriter.WriteLine($"scene_ended" + "\t" + RemapManager.fmriTimer.ToString());
//             RemapManager.fmriWriter.Flush();
//             FindObjectOfType<RemapManager>()?.OnSceneCompleted();
//         }
//     }


//     void OnGUI()
//     {
//         if (showGUI)
//         {
//             Texture2D blackTexture = new Texture2D(1, 1);
//             blackTexture.SetPixel(0, 0, new Color32(217, 217, 217, 255));
//             blackTexture.Apply();
//             GUIStyle bgStyle = new GUIStyle();
//             bgStyle.normal.background = blackTexture;

//             GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, bgStyle);

//             GUIStyle guiStyle = new GUIStyle();
//             guiStyle.fontSize = 90;
//             guiStyle.fontStyle = FontStyle.Bold;
//             guiStyle.normal.textColor = Color.black;
//             guiStyle.alignment = TextAnchor.MiddleCenter; 
//             GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 200, 100), guiMessage, guiStyle);
//         }
//     }
    
//     void SetPlayerPos(GameObject playerObject, float minDistance = 3f)
//     {
//         GameObject playerStart = GameObject.Find("player_start");
//         Vector3 center = playerStart.transform.position;
//         float radius = playerStart.transform.localScale.x * 0.5f;

//         Vector3 randomPosition = Vector3.zero;
//         int maxAttempts = 100;
//         int attempts = 0;
//         bool found = false;

//         while (attempts < maxAttempts)
//         {
//             // 원 안의 무작위 점 뽑기 (평면 기준)
//             Vector2 randomCircle = Random.insideUnitCircle * radius;
//             randomPosition = new Vector3(center.x + randomCircle.x, 0.91f, center.z + randomCircle.y);

//             bool tooClose = false;

//             for (int i = 0; i < 2; i++)
//             {
//                 for (int j = 0; j < 2; j++)
//                 {
//                     Vector3 target = SbjManager.context_target[i, j];
//                     float distance = Vector3.Distance(
//                         new Vector3(randomPosition.x, 0f, randomPosition.z),
//                         new Vector3(target.x, 0f, target.z)
//                     );

//                     if (distance < minDistance)
//                     {
//                         tooClose = true;
//                         break;
//                     }
//                 }
//                 if (tooClose) break;
//             }

//             if (!tooClose)
//             {
//                 found = true;
//                 break;
//             }

//             attempts++;
//         }

//         if (!found)
//         {
//             randomPosition = center + new Vector3(0f, 0.91f, -radius * 0.8f);
//         }

//         // 플레이어 위치 설정
//         playerObject.transform.position = randomPosition;
//         SbjManager.start_loc = playerObject.transform.position;
//     }

//     IEnumerator birdeye()
//     {
//         isBirdEyeViewActive = true;

//         while (showFeedback)
//         {
//             feedback_res.transform.position = response_loc;

//             // === 핵심 인덱스 추출 ===
//             int order_idx = SbjManager.sbj_learn_order[SbjManager.curr_order];
//             int fam_idx = SbjManager.sbj_fam_order[0];
//             int curr_run = SbjManager.curr_run;
//             // === 피드백 위치 설정 ===
//             feedback_ans.SetActive(true);
            
//             // fam_idx와 order_idx가 같은 경우 (learn_order: [0, 1])
//             if (fam_idx == order_idx)
//             {
//                 if (curr_run == 0)
//                 {
//                     if (response_idx == 0 && SbjManager.target_idx == 0)
//                         feedback_ans.transform.position = SbjManager.context_target[0, 0]; // good
//                     else if (response_idx == 1 && SbjManager.target_idx == 0)
//                         feedback_ans.transform.position = SbjManager.context_target[0, 1]; // bad
//                     else if (response_idx == 0 && SbjManager.target_idx == 1)
//                         feedback_ans.transform.position = SbjManager.context_target[0, 1]; // bad
//                     else if (response_idx == 1 && SbjManager.target_idx == 1)
//                         feedback_ans.transform.position = SbjManager.context_target[0, 0]; // good
//                 }
//                 else // curr_run == 1
//                 {
//                     if (response_idx == 0 && SbjManager.target_idx == 0)
//                         feedback_ans.transform.position = SbjManager.context_target[0, 1]; // good
//                     else if (response_idx == 1 && SbjManager.target_idx == 0)
//                         feedback_ans.transform.position = SbjManager.context_target[0, 0]; // bad
//                     else if (response_idx == 0 && SbjManager.target_idx == 1)
//                         feedback_ans.transform.position = SbjManager.context_target[0, 1]; // bad
//                     else if (response_idx == 1 && SbjManager.target_idx == 1)
//                         feedback_ans.transform.position = SbjManager.context_target[0, 0]; // good
//                 }
//             }
//             // fam_idx와 order_idx가 다른 경우 (learn_order: [1, 0])
//             else
//             {
//                 if (curr_run == 0)
//                 {
//                     if (response_idx == 0 && SbjManager.target_idx == 0)
//                         feedback_ans.transform.position = SbjManager.context_target[1, 0]; // good
//                     else if (response_idx == 1 && SbjManager.target_idx == 0)
//                         feedback_ans.transform.position = SbjManager.context_target[1, 1]; // bad
//                     else if (response_idx == 0 && SbjManager.target_idx == 1)
//                         feedback_ans.transform.position = SbjManager.context_target[1, 1]; // bad
//                     else if (response_idx == 1 && SbjManager.target_idx == 1)
//                         feedback_ans.transform.position = SbjManager.context_target[1, 0]; // good
//                 }
//                 else // curr_run == 1
//                 {
//                     if (response_idx == 0 && SbjManager.target_idx == 0)
//                         feedback_ans.transform.position = SbjManager.context_target[1, 1]; // good
//                     else if (response_idx == 1 && SbjManager.target_idx == 0)
//                         feedback_ans.transform.position = SbjManager.context_target[1, 0]; // bad
//                     else if (response_idx == 0 && SbjManager.target_idx == 1)
//                         feedback_ans.transform.position = SbjManager.context_target[1, 0]; // bad
//                     else if (response_idx == 1 && SbjManager.target_idx == 1)
//                         feedback_ans.transform.position = SbjManager.context_target[1, 1]; // good
//                 }
//             }

//             correct_loc = feedback_ans.transform.position;

//             // === 카메라 설정 ===
//             float camHeight = (curr_run == 0) ? 40f : 50f;
//             spectatorCamera.transform.position = new Vector3(0f, camHeight, 0f);
//             spectatorCamera.transform.rotation = Quaternion.Euler(90f, -180f, 0f);

//             yield return null;
//         }

//         isBirdEyeViewActive = false;
//     }

//     void OnDestroy()
//     {
//         SbjManager.CloseLogger();
//     }
// }