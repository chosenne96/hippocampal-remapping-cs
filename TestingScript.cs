// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.SceneManagement;

// public class TestingScript : MonoBehaviour
// {
//     // gameobject
//     FirstPersonAIO player;
//     private GameObject playerObject;
//     GameObject text;
//     GameObject greenMarker, blueMarker;
//     GameObject notifyBgnd, notifyText;
//     GameObject marker;
//     GameObject target1, target2;

//     public Camera spectatorCamera;
//     private GameObject presentObject;
//     private float guiStartTime = -1f;
//     private int response_idx = 0;
//     private GUIStyle guiStyle;
//     private bool isDetecting = false;
//     private bool hasKeyPressed = false;
//     private string guiMessage = "";
//     private bool showGUI = false;
//     private bool sceneReady = true;
//     private bool writingtime = false;
//     private Texture2D blackTexture;  // GUI 텍스처 캐싱
    
//     // directory
//     private string ori_dir, bhv1_dir, bhv2_dir, time_dir, coin1_dir, coin2_dir, coin3_dir, tmpDate;
//     private List<GameObject> flowerObjects = new List<GameObject>();  // 꽃 오브젝트 캐싱

//     private bool isKeyProcessing = false;  // 키 처리 상태

//     void Awake()
//     {
//         string ori_dir = "./output/";
//         string tmpDate = System.DateTime.Now.ToString("yyyyMMdd");
//         string filenamePrefix = $"{tmpDate}_sbj{SbjManager.sbj_num}";
//         SbjManager.FileLogger(ori_dir, filenamePrefix);
//     }

//     void Start()
//     {
//         playerObject = GameObject.FindWithTag("Player");
//         player = playerObject.GetComponent<FirstPersonAIO>();
//         // scene setting
//         text = GameObject.Find("Text");      
//         greenMarker = GameObject.Find("GreenMarker");
//         blueMarker = GameObject.Find("BlueMarker");
//         // notifyBgnd = GameObject.Find("NotifyBgnd");
//         // notifyText = GameObject.Find("NotifyText");
//         marker = GameObject.Find("Marker");
//         target1 = GameObject.Find("target1"); // 큰토끼(first run)
//         target2 = GameObject.Find("target2");

//         marker.SetActive(false);
//         greenMarker.SetActive(false);
//         blueMarker.SetActive(false);
//         // notifyBgnd.SetActive(false);
//         // notifyText.SetActive(false);
//         target1.SetActive(false);
//         target2.SetActive(false);
        
//         player.GetComponent<FirstPersonAIO>().playerCanMove = true;
//         player.GetComponent<FirstPersonAIO>().useHeadbob = false;
//         player.GetComponent<FirstPersonAIO>().canJump = false;
//         player.GetComponent<FirstPersonAIO>().useStamina = false;
//         player.GetComponent<AudioSource>().enabled = false;
//         player.GetComponent<FirstPersonAIO>().enableCameraShake = false;
//         player.GetComponent<FirstPersonAIO>().cameraSmoothing = 10f;
        
//         // GUI 텍스처 초기화
//         blackTexture = new Texture2D(1, 1);
//         blackTexture.SetPixel(0, 0, new Color32(217, 217, 217, 255));
//         blackTexture.Apply();
//         writingtime = true;

//         // SpawnFlower();
//         // UpdateFlowerList();  // 꽃과 스템 객체 리스트 업데이트
//         StartCoroutine(SceneSetting());
//     }
    
//     IEnumerator SceneSetting()
//     {
//         SetPlayerPos(playerObject);
//         spectatorCamera.enabled = false;

//         yield return new WaitForSeconds(6f);  // Wait for fixation period

//         RemapManager.fmriWriter.WriteLine($"scene_started" + "\t" + RemapManager.fmriTimer.ToString());
//         RemapManager.fmriWriter.Flush();
//         SbjManager.showFixation = false; 

//         SbjManager.start_time = System.DateTime.Now.ToString("HH:mm:ss.fff");
//         SbjManager.start_rot = playerObject.transform.rotation.eulerAngles;

//         writingtime = true;
//         StartCoroutine(LogPlayerPosition());  // Start logging only after the scene is fully set
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

//     void UpdateFlowerList()
//     {
//         flowerObjects.Clear();
//         GameObject[] allObjects = FindObjectsOfType<GameObject>();
//         foreach (GameObject obj in allObjects)
//         {
//             if (obj.name.ToLower().Contains("flower") || obj.name.ToLower().Contains("stem"))
//             {
//                 flowerObjects.Add(obj);
//             }
//         }
//     }

//     void Update()
//     {        
//         if (!sceneReady || SbjManager.showFixation || isKeyProcessing || hasKeyPressed)
//         return;

//         // 최초 1회만 키 입력 감지하고, showFixation == false일 때만
//         if (Input.GetKeyDown(KeyCode.Alpha4))
//         {
//             hasKeyPressed = true;  // 다시 감지하지 않도록
//             RemapManager.fmriWriter.WriteLine($"resp_test" + "\t" + RemapManager.fmriTimer.ToString());
//             RemapManager.fmriWriter.Flush();
//             StartCoroutine(Detectkey());
//         }
//         // 꽃 회전
//         // foreach (GameObject flower in flowerObjects)
//         // {
//         //     RotateToFacePlayer(flower);
//         // }
//     }

//     IEnumerator Detectkey()
//     {
//         if (isKeyProcessing) yield break;
//         isKeyProcessing = true;
//         writingtime = false;

//         // 카메라 전환
//         Camera playerCamera = player.GetComponentInChildren<Camera>();
//         Vector3 lastpos = player.transform.position;
//         Quaternion lastrot = player.transform.rotation;

//         // 타겟 보여주기
//         target1.transform.position = player.transform.position + player.transform.forward * 3;
//         target1.transform.position = new Vector3(target1.transform.position.x, 0f, target1.transform.position.z);
//         Quaternion playerRotation = player.transform.rotation;
//         Quaternion offsetRotation = Quaternion.Euler(0, 150f, 0);
//         Quaternion targetRotation = playerRotation * offsetRotation;
//         target1.transform.rotation = targetRotation;
//         spectatorCamera.transform.SetPositionAndRotation(playerCamera.transform.position, playerCamera.transform.rotation);
//         playerCamera.enabled = false;
//         spectatorCamera.enabled = true;
//         player.enabled = false;
//         target1.SetActive(true);
//         yield return new WaitForSeconds(2f);

//         // 원래 상태로 복구
//         target1.SetActive(false);
//         spectatorCamera.enabled = false;
//         playerCamera.enabled = true;
//         player.transform.SetPositionAndRotation(lastpos, lastrot);
//         player.enabled = true;

//         if (SbjManager.curr_task == "testing")
//         {
//         SbjManager.writer1.WriteLine($"{SbjManager.start_time}s {SbjManager.start_loc} {SbjManager.start_rot} {System.DateTime.Now.ToString("HH:mm:ss.fff")}s {SbjManager.curr_task} {SbjManager.curr_scene} {SbjManager.curr_run}_{SbjManager.curr_trial} {lastpos} {null} {null}");
//         SbjManager.writer1.Flush();
//         }
//         else
//         {
//         SbjManager.writer2.WriteLine($"{SbjManager.start_time}s {SbjManager.start_loc} {SbjManager.start_rot} {System.DateTime.Now.ToString("HH:mm:ss.fff")}s {SbjManager.curr_task} {SbjManager.curr_scene} {SbjManager.curr_run}_{SbjManager.curr_trial} {lastpos} {null} {null}");
//         SbjManager.writer2.Flush();
//         }
        
//         SbjManager.showFixation = true; RemapManager.fmriWriter.WriteLine($"fix_start" + "\t" + RemapManager.fmriTimer.ToString());
//         RemapManager.fmriWriter.WriteLine($"scene_ended" + "\t" + RemapManager.fmriTimer.ToString());
//         RemapManager.fmriWriter.Flush();
//         // FindObjectOfType<RemapManager>()?.OnSceneCompleted();
//         FindObjectOfType<RemapManager_bhv>()?.OnSceneCompleted();
//         // FindObjectOfType<RemapManager_eng>()?.OnSceneCompleted();

//         isKeyProcessing = false;
//     }

//     void SpawnFlower()
//     {
//         string flowerName = SbjManager.remapflowers[SbjManager.curr_order, SbjManager.env_rep[SbjManager.curr_order]];
//         Sprite flowerSprite = Resources.Load<Sprite>("flowers/" + flowerName);

//         GameObject flower = new GameObject("Flower");
//         SpriteRenderer renderer = flower.AddComponent<SpriteRenderer>();
//         renderer.sprite = flowerSprite;
        
//         if (flowerName == "Magic flowers-59" || flowerName == "Magic flowers-76")
//         {
//             flower.transform.position = new Vector3(0f, 1.1f, 0f);
//             flower.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
//         }
//         else 
//         {
//             flower.transform.position = new Vector3(0f, 1.3f, 0f);
//             flower.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
//         }
//     }

//     void RotateToFacePlayer(GameObject flower)
//     {
//         Vector3 direction = player.transform.position - flower.transform.position;
//         direction.y = 0f; // Y축 회전만 적용하기 위해 높이 차 제거

//         if (direction != Vector3.zero)
//         {
//             // 방향 벡터 기준 Y축 회전만 적용
//             Quaternion targetRotation = Quaternion.LookRotation(direction);
//             flower.transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
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

//     void OnGUI()
//     {
//         if (showGUI && blackTexture != null)
//         {
//             GUIStyle bgStyle = new GUIStyle();
//             bgStyle.normal.background = blackTexture;

//             GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, bgStyle);

//             GUIStyle guiStyle = new GUIStyle();
//             guiStyle.fontSize = 100;
//             guiStyle.fontStyle = FontStyle.Bold;
//             guiStyle.normal.textColor = Color.black;
//             guiStyle.alignment = TextAnchor.MiddleCenter; 
//             GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 200, 100), guiMessage, guiStyle);
//         }
//     }

//     void OnDestroy()
//     {
//         if (blackTexture != null)
//         {
//             Destroy(blackTexture);
//         }
//         SbjManager.CloseLogger();
//     }
// }