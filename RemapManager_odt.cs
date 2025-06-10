// using UnityEngine;
// using UnityEngine.UI;
// using System.IO;
// using System;
// using System.Linq;
// using Random=UnityEngine.Random;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine.SceneManagement;
// using System.Text.RegularExpressions;

// public class RemapManager : MonoBehaviour
// {
//     GameObject player, target1, target2, target3, target4;
//     // directory
//     private string ori_dir, bhv1_dir, bhv2_dir, time_dir, coin1_dir, coin2_dir, coin3_dir, odt1_dir, odt2_dir, tmpDate;
//     // task information
//     private int curr_idx = 0;
//     private int curr_env = 0;
//     public int[,] seqOrder;
//     public int[,] subseqOrder;
//     public List<int[][]> chunkOrder; // 클래스 멤버 변수로 선언
//     public List<Vector3[]> playerStart;
//     public List<int[][]> seqList;
//     private Texture2D flowerTexture;    
//     int[,] r_env_rep = new int[5, 5];  // remap 인덱스 랜덤 저장
//     int[] r_env_idx = new int[5];       


//     // input ui
//     public GameObject InputCanvas;
//     public InputField sbjNumInput;
//     public InputField sbjTaskInput; // SbjManager.sbj_task
//     public InputField modeInput; // target mode
//     public InputField flowermodeInput;
//     public Button submitButton;
//     private string flowermode;
//     private int mode = 0;
    
//     // gui
//     private bool showGUI = false;
//     private bool showGUIimage = false;
//     private bool isCompleted = false;
//     private string guiMessage = "";
//     public float maxDistance = 25f;  // 원형 범위의 지름
//     public Vector3 origin = Vector3.zero;  // 기준점 (0,0,0 위치)

//     // flower odt
//     private Sprite[] flowerList = new Sprite[20]; // # of flowers
//     public Sprite target;
//     public string curr_flower;
//     public float duration, isi;
//     public string start_time;

//     private bool showFlower = false;
//     private Texture2D backgroundTex;

//     void Awake()
//     {    
//         DontDestroyOnLoad(this.gameObject);
//         // log file (file name)
//         ori_dir = "./output/";
//         bhv1_dir = Path.Combine(ori_dir, "Log_remap1");
//         bhv2_dir = Path.Combine(ori_dir, "Log_remap2");
//         time_dir = Path.Combine(ori_dir, "Log_time");
//         coin1_dir = Path.Combine(ori_dir, "Log_coin1");
//         coin2_dir = Path.Combine(ori_dir, "Log_coin2");
//         coin3_dir = Path.Combine(ori_dir, "Log_coin3");
//         odt1_dir = Path.Combine(ori_dir, "Log_odt1");
//         odt2_dir = Path.Combine(ori_dir, "Log_odt2");
//         tmpDate = System.DateTime.Now.ToString("yyyyMMdd");
//         SbjManager.FileLogger(bhv1_dir, bhv2_dir, time_dir, coin1_dir, coin2_dir, coin3_dir, odt1_dir, odt2_dir, tmpDate, SbjManager.sbj_num);

//         player = GameObject.Find("FirstPerson-AIO");
//     }


//     void Start()
//     {
//         submitButton.onClick.AddListener(SaveUserInput);
//     }

//     void SaveUserInput()
//     {
//         SbjManager.showFixation = true;

//         SbjManager.sbj_num = sbjNumInput.text;
//         SbjManager.sbj_task = int.Parse(sbjTaskInput.text);
//         SbjManager.sbj_mode = modeInput.text;
//         int.TryParse(flowermodeInput.text, out mode);
//         flowermode = flowermodeInput.text;
        
//         // === fmri
//         SbjManager.fmriWriter = File.AppendText("./output/" + SbjManager.sbj_num + "_fmri.txt");
//         SbjManager.fmriWriter.WriteLine("RemapManager" + "\t" + SbjManager.fmriTimer.ToString());

//         Logfile();
//         // modeInput 설명 업데이트
//         int modeValue;
//         bool isValidMode = int.TryParse(modeInput.text, out modeValue);
//         if (!isValidMode || modeValue < 1 || modeValue > 4)
//         {
//             Debug.LogWarning("⚠️ modeInput에는 '1', '2', '3', '4' 중 하나를 입력하세요. 잘못된 값은 기본값 1로 처리합니다.");
//             SbjManager.sbj_mode = "1";
//             modeInput.text = "1"; // modeInput의 텍스트도 업데이트
//         }

//         Debug.Log($"[SUBJECT INFO] Sbject ID: {SbjManager.sbj_num}, Task starts with: {SbjManager.sbj_task}, target mode: {SbjManager.sbj_mode}, flower mode: {flowermode}");
//         InputCanvas.SetActive(false);
        
//         // 사용자 입력 저장 후에 sceneOrderSetting 호출
//         sceneOrderSetting();
        
//         if (flowermode == "1") // 텍스트 파일에서 flowerSeq 불러오기
//         {
//             LoadFlowerSequencesFromFile();
//             LoadRemapFlowersFromFile();
//         }
//         else // 새로 생성하고 텍스트 파일로 저장
//         {
//             LoadSprites();  
//             GenerateSequences();
//             SaveFlowerSequencesToFile();
//             SaveRemapFlowersToFile();
//         }
//         StartCoroutine(Taskplayer());
//     }
    
//     void sceneOrderSetting()
//     {
//         // SbjManager.sbj_fam_order = GenerateFamVector(); // 이전 랜덤 방식 제거

//         // 새로운 modeInput 처리
//         int modeValue;
//         bool isValid = int.TryParse(modeInput.text, out modeValue);

//         if (!isValid || modeValue < 1 || modeValue > 4)
//         {
//             modeValue = 1; // 잘못된 값이 들어오면 기본값 1로 처리
//         }

//         // modeInput 값에 따라 sbj_fam_order 설정
//         SbjManager.sbj_fam_order = new int[2];
        
//         if (modeValue == 1 || modeValue == 2)
//         {
//             // [0,1] 설정
//             SbjManager.sbj_fam_order[0] = 0;
//             SbjManager.sbj_fam_order[1] = 1;
//         }
//         else // modeValue == 3 || modeValue == 4
//         {
//             // [1,0] 설정
//             SbjManager.sbj_fam_order[0] = 1;
//             SbjManager.sbj_fam_order[1] = 0;
//         }

//         // 타겟 순서 설정을 위한 swap 값 결정
//         bool swap = (modeValue == 2 || modeValue == 4);
        
//         Vector3[] contextA = new Vector3[]
//         {
//             new Vector3(-8.2f, 0f, 7.0f),   // A1
//             new Vector3(-6.8f, 0f, -7.5f)   // A2
//         };

//         Vector3[] contextB = new Vector3[]
//         {
//             new Vector3(8f, 0f, -7.9f),     // B1
//             new Vector3(6.4f, 0f, 6.6f)     // B2
//         };

//         // modeInput 값에 따른 swap 설정 (앞에서 결정됨)

//         Debug.Log($"Mode value: {modeValue}, swap: {swap}, sbj_fam_order: [{string.Join(", ", SbjManager.sbj_fam_order)}]");

//         if (swap)
//         {
//             // mode 2 또는 4: 타겟 순서 바꿈
//             SbjManager.context_target[SbjManager.sbj_fam_order[0], 0] = contextA[1];
//             SbjManager.context_target[SbjManager.sbj_fam_order[0], 1] = contextA[0];
//             SbjManager.context_target[1 - SbjManager.sbj_fam_order[0], 0] = contextB[1];
//             SbjManager.context_target[1 - SbjManager.sbj_fam_order[0], 1] = contextB[0];
//         }
//         else
//         {
//             // mode 1 또는 3: 기본 타겟 순서
//             SbjManager.context_target[SbjManager.sbj_fam_order[0], 0] = contextA[0];
//             SbjManager.context_target[SbjManager.sbj_fam_order[0], 1] = contextA[1];
//             SbjManager.context_target[1 - SbjManager.sbj_fam_order[0], 0] = contextB[0];
//             SbjManager.context_target[1 - SbjManager.sbj_fam_order[0], 1] = contextB[1];
//         }
//         Debug.Log($"[context {SbjManager.sbj_fam_order[0]}] target1: {SbjManager.context_target[SbjManager.sbj_fam_order[0], 0]}");
//         Debug.Log($"[context {SbjManager.sbj_fam_order[0]}] target2: {SbjManager.context_target[SbjManager.sbj_fam_order[0], 1]}");
//         Debug.Log($"[context {1 - SbjManager.sbj_fam_order[0]}] target3: {SbjManager.context_target[1 - SbjManager.sbj_fam_order[0], 0]}");
//         Debug.Log($"[context {1 - SbjManager.sbj_fam_order[0]}] target4: {SbjManager.context_target[1 - SbjManager.sbj_fam_order[0], 1]}");
//     }

//     public enum TaskPhase
//     {
//         PreODT = 1,
//         PreForaging = 2,
//         Learning1 = 3,
//         IntermForaging = 4,
//         Learning2 = 5,
//         PostForaging = 6,
//         PostODT = 7
//     }

//     TaskPhase GetTaskPhase(int sbj_task)
//     {
//         if (sbj_task == 0 || sbj_task == 1)
//             return TaskPhase.PreODT;
//         else if (sbj_task == 2)
//             return TaskPhase.PreForaging;
//         else if (sbj_task == 3 || sbj_task == 4 || sbj_task == 5)
//             return TaskPhase.Learning1;
//         else if (sbj_task == 6)
//             return TaskPhase.IntermForaging;
//         else if (sbj_task == 7 || sbj_task == 8 || sbj_task == 9)
//             return TaskPhase.Learning2;
//         else if (sbj_task == 10)
//             return TaskPhase.PostForaging;
//         else if (sbj_task == 11)
//             return TaskPhase.PostODT;
            
//         throw new ArgumentOutOfRangeException(nameof(sbj_task), $"Invalid task value: {sbj_task}");
//     }

//     IEnumerator Taskplayer()
//     {
//         TaskPhase phase = GetTaskPhase(SbjManager.sbj_task);

//         ChunkSetting();

//         if (phase == TaskPhase.PreODT)
//         {
//             yield return StartCoroutine(CoinTaskplayer()); // pre
//             SbjManager.curr_coin_sess++;
 
//             yield return StartCoroutine(odtTaskplayer());

//             yield return StartCoroutine(RemapTaskplayer()); // 1st run
//             SbjManager.curr_run++;
            
//             yield return StartCoroutine(CoinTaskplayer()); // intermediate
//             SbjManager.curr_coin_sess++;
            
//             yield return StartCoroutine(ReverseTaskplayer()); // 2nd run
            
//             yield return StartCoroutine(CoinTaskplayer()); // 2nd run

//             yield return StartCoroutine(odtTaskplayer()); 
//         }
//         else if (phase == TaskPhase.PreForaging)
//         {
//             SbjManager.curr_coin_sess++;
//             yield return StartCoroutine(odtTaskplayer());  

//             yield return StartCoroutine(RemapTaskplayer()); // 1st run
//             SbjManager.curr_run++;
            
//             yield return StartCoroutine(CoinTaskplayer()); // intermediate
//             SbjManager.curr_coin_sess++;
            
//             yield return StartCoroutine(ReverseTaskplayer()); // 2nd run
            
//             yield return StartCoroutine(CoinTaskplayer()); // 2nd run

//             yield return StartCoroutine(odtTaskplayer()); 
//         }
//         else if (phase == TaskPhase.Learning1)
//         {
//             SbjManager.curr_coin_sess++;
//             yield return StartCoroutine(RemapTaskplayer()); // 1st run
//             SbjManager.curr_run++;
            
//             yield return StartCoroutine(CoinTaskplayer()); // intermediate
//             SbjManager.curr_coin_sess++;
            
//             yield return StartCoroutine(ReverseTaskplayer()); // 2nd run
            
//             yield return StartCoroutine(CoinTaskplayer()); // 2nd run

//             yield return StartCoroutine(odtTaskplayer()); 
//         }
//         else if (phase == TaskPhase.IntermForaging)
//         {
            
//             SbjManager.curr_coin_sess++;
//             SbjManager.curr_run++;
//             yield return StartCoroutine(CoinTaskplayer()); // intermediate
//             SbjManager.curr_coin_sess++;
            
//             yield return StartCoroutine(ReverseTaskplayer()); // 2nd run
            
//             yield return StartCoroutine(CoinTaskplayer()); // 2nd run

//             yield return StartCoroutine(odtTaskplayer()); 
//         }
//         else if (phase == TaskPhase.Learning2)
//         {
//             SbjManager.curr_coin_sess++;
//             SbjManager.curr_run++;
//             yield return StartCoroutine(ReverseTaskplayer()); // 2nd run
            
//             yield return StartCoroutine(CoinTaskplayer()); // 2nd run
//             SbjManager.curr_coin_sess++;
            
//             yield return StartCoroutine(odtTaskplayer()); 
//         }
//         else if (phase == TaskPhase.PostForaging)
//         {
//             SbjManager.curr_coin_sess++;
//             SbjManager.curr_run++;
//             yield return StartCoroutine(CoinTaskplayer()); // 2nd run

//             SbjManager.curr_coin_sess++;
//             yield return StartCoroutine(odtTaskplayer()); 
//         }
//         else if (phase == TaskPhase.PostODT)
//         {
//             SbjManager.curr_coin_sess++;
//             SbjManager.curr_run++;
//             SbjManager.curr_coin_sess++;

//             yield return StartCoroutine(odtTaskplayer()); 
//         }
//     }

//     IEnumerator SceneTransit()
//     {
//         showGUI = true;
//         guiMessage = " ";
//         yield return new WaitForSeconds(0.2f);
//         showGUI = false;
//     }

//     IEnumerator odtTaskplayer()
//     {
//         showGUIimage = true; 
//         SbjManager.showFixation = false;
//         guiMessage = "화면에 나타나는 꽃을 보고,\n타겟이 나오면 선택 버튼을 눌러주세요.";
//         Sprite flowerSprite = Resources.Load<Sprite>("flowers/Magic flowers-82");
//         flowerTexture = SpriteToTexture(flowerSprite);
//         yield return new WaitForSeconds(6f);
//         showGUIimage = false;
//         flowerTexture = null;

//         AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("ODTScene");                
//         yield return new WaitUntil(() => isCompleted);
//         asyncLoad.allowSceneActivation = false;
//         isCompleted = false;

//         SbjManager.curr_odt_run++;

//         if (SbjManager.curr_odt_run == 2)
//         {
//             showGUI = true;
//             guiMessage = "실험이 모두 종료되었습니다.";
//             yield return new WaitForSeconds(60f);
//             showGUI = false;
//         }
        
//         SbjManager.showFixation = true; // task 전환 시 번쩍 제거용
//     }
//     IEnumerator CoinTaskplayer()
//     {
        
//         if (SbjManager.curr_coin_sess == 0)
//         {
//             SbjManager.sbj_coin_order = GenerateCoinOrder(3);
//             SbjManager.coinWriter1.WriteLine($"[{string.Join(", ", SbjManager.sbj_coin_order[0])}]");
//             SbjManager.coinWriter1.WriteLine($"[{string.Join(", ", SbjManager.sbj_coin_order[1])}]");
//             SbjManager.coinWriter1.WriteLine($"[{string.Join(", ", SbjManager.sbj_coin_order[2])}]");
//             SbjManager.coinWriter1.Flush();
//         }
//         else if (SbjManager.curr_coin_sess == 1)
//         {
//             SbjManager.sbj_coin_order = GenerateCoinOrder(3);
//             SbjManager.coinWriter2.WriteLine($"[{string.Join(", ", SbjManager.sbj_coin_order[0])}]");
//             SbjManager.coinWriter2.WriteLine($"[{string.Join(", ", SbjManager.sbj_coin_order[1])}]");
//             SbjManager.coinWriter2.WriteLine($"[{string.Join(", ", SbjManager.sbj_coin_order[2])}]");
//             SbjManager.coinWriter2.Flush();
//         }
//         else if (SbjManager.curr_coin_sess == 2)
//         {
//             SbjManager.sbj_coin_order = GenerateCoinOrder(3);
//             SbjManager.coinWriter3.WriteLine($"[{string.Join(", ", SbjManager.sbj_coin_order[0])}]");
//             SbjManager.coinWriter3.WriteLine($"[{string.Join(", ", SbjManager.sbj_coin_order[1])}]");
//             SbjManager.coinWriter3.WriteLine($"[{string.Join(", ", SbjManager.sbj_coin_order[2])}]");
//             SbjManager.coinWriter3.Flush();
//         }
//         yield return new WaitForSeconds(4f);
//         SbjManager.showFixation = false;
//         SbjManager.curr_coin_run = 0;
//         Debug.Log("=== Coin foraging task started");
        
//         showGUI = true;
//         guiMessage = "당신은 농부입니다.\n\n여러 텃밭을 돌아다니며\n씨앗 주머니를 주워주세요.";
//         yield return new WaitForSeconds(6f);
//         showGUI = false;

//         // run1
//         for (int curr_idx = 0; curr_idx < SbjManager.sbj_coin_order[SbjManager.curr_coin_run].Length; curr_idx++) // 1 run ( 5 environment * 3 times)
//         {
//             int order = SbjManager.sbj_coin_order[SbjManager.curr_coin_run][curr_idx];
//             string sceneName = $"CoinScene{order}";
//             curr_env = order;
//             Debug.Log($"=== {sceneName} ready for trial #{curr_idx + 1}");
//             CoinSetting();

//             string taskname = "foraging_run1";
//             SbjManager.curr_task = taskname;
//             SbjManager.curr_scene = sceneName;
//             SbjManager.curr_coin_trial++;

//             SbjManager.showFixation = true;
//             yield return new WaitForSeconds(6f);
//             AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);                
//             yield return new WaitUntil(() => isCompleted);
//             asyncLoad.allowSceneActivation = false;
//             isCompleted = false;
//         }
        
//         showGUI = true;
//         guiMessage = "휴식 시간입니다.\n휴식이 끝나면 스페이스 바를 누르세요.";
//         yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
//         showGUI = false;

//         SbjManager.showFixation = true;
//         yield return new WaitForSeconds(6f);
//         SbjManager.showFixation = false;
        
//         SbjManager.curr_coin_run++;
//         Debug.Log($"==curr_coin_run: {SbjManager.curr_coin_run}");
//         // run 2
//         for (int curr_idx = 0; curr_idx < SbjManager.sbj_coin_order[SbjManager.curr_coin_run].Length; curr_idx++) // 1 run ( 5 environment * 3 times)
//         {
//             int order = SbjManager.sbj_coin_order[SbjManager.curr_coin_run][curr_idx];
//             string sceneName = $"CoinScene{order}";
//             curr_env = order;
//             Debug.Log($"=== {sceneName} ready for trial #{curr_idx + 1}");
//             CoinSetting();

//             string taskname = "foraging_run2";
//             SbjManager.curr_task = taskname;
//             SbjManager.curr_scene = sceneName;
//             SbjManager.curr_coin_trial = curr_idx + 1;

//             SbjManager.showFixation = true;
//             yield return new WaitForSeconds(6f);
//             AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);                
//             yield return new WaitUntil(() => isCompleted);
//             asyncLoad.allowSceneActivation = false;
//             isCompleted = false;
//         }
//         showGUI = true;
//         guiMessage = "휴식 시간입니다.\n휴식이 끝나면 스페이스 바를 누르세요.";
//         yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
//         showGUI = false;
        
//         SbjManager.showFixation = true;
//         yield return new WaitForSeconds(6f);
//         SbjManager.showFixation = false;

//         SbjManager.curr_coin_run++;
//         Debug.Log($"==curr_coin_run: {SbjManager.curr_coin_run}");
//         // run 3
//         for (int curr_idx = 0; curr_idx < SbjManager.sbj_coin_order[SbjManager.curr_coin_run].Length; curr_idx++) // 1 run ( 5 environment * 3 times)
//         {
//             int order = SbjManager.sbj_coin_order[SbjManager.curr_coin_run][curr_idx];
//             string sceneName = $"CoinScene{order}";
//             curr_env = order;
//             Debug.Log($"=== {sceneName} ready for trial #{curr_idx + 1}");
//             CoinSetting();

//             string taskname = "foraging_run3";
//             SbjManager.curr_task = taskname;
//             SbjManager.curr_scene = sceneName;
//             SbjManager.curr_coin_trial = curr_idx + 1;

//             SbjManager.showFixation = true;
//             yield return new WaitForSeconds(6f);
//             AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);                
//             yield return new WaitUntil(() => isCompleted);
//             asyncLoad.allowSceneActivation = false;
//             isCompleted = false;
            
//             if (curr_idx == 4)
//             {
//                 showGUI = true;
//                 guiMessage = "이제 씨앗을 충분히 주웠습니다.";
//                 yield return new WaitForSeconds(6f);
//                 showGUI = false;
//             }
//         }
//         showGUI = true;
//         guiMessage = "휴식 시간입니다.\n휴식이 끝나면 스페이스 바를 누르세요.";
//         yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
//         showGUI = false;

//         SbjManager.showFixation = true; // task 전환 시 번쩍 제거용
//     }

//     IEnumerator RemapTaskplayer()
//     {   
//             SbjManager.sbj_fam_order = GenerateFamVector();
//             SbjManager.sbj_learn_order = GenerateLearnVector();
//             SbjManager.sbj_test_order = GenerateTestVector();
//             SbjManager.writer1.WriteLine($"familiarization: [{string.Join(", ", SbjManager.sbj_fam_order)}]");
//             SbjManager.writer1.WriteLine($"learning: [{string.Join(", ", SbjManager.sbj_learn_order)}]");
//             SbjManager.writer1.WriteLine($"testing: [{string.Join(", ", SbjManager.sbj_test_order)}]");
//             SbjManager.writer1.Flush();

//             Debug.Log("=== Remapping Taskplayer started");

//             yield return new WaitForSeconds(6f);
//             SbjManager.showFixation = false; // task 전환 시 번쩍 제거용
            
//             showGUI = true;
//             guiMessage = "텃밭에 있는 버섯을 찾는 미션입니다.\n\n텃밭마다 맛있는 버섯과\n독버섯이 있습니다.";
//             yield return new WaitForSeconds(6f);
//             showGUI = false;


//         if (SbjManager.sbj_task <= 3)
//         {
//             showGUI = true;
//             guiMessage = "등장하는 텃밭에서\n두 가지 버섯의 위치를 기억하세요.\n(45초)";
//             yield return new WaitForSeconds(6f);
//             showGUI = false;

//             for (curr_idx = 0; curr_idx < SbjManager.sbj_fam_order.Length; curr_idx++)
//             {
//                 int order = SbjManager.sbj_fam_order[curr_idx];
//                 string sceneName = order == 0 ? "FamScene0" : "FamScene4";
//                 Debug.Log($"=== {sceneName} ready");
//                 curr_env = order;

//                 SbjManager.curr_task = "fam";
//                 SbjManager.curr_scene = sceneName;
//                 SbjManager.curr_trial = curr_idx + 1;
//                 SbjManager.curr_order = SbjManager.sbj_fam_order[curr_idx];

//                 SbjManager.showFixation = true;
//                 AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);                
//                 yield return new WaitUntil(() => isCompleted);
//                 asyncLoad.allowSceneActivation = false;
//                 yield return new WaitForSeconds(6f);
//                 showGUI = true;
//                 guiMessage = "다음 텃밭으로 이동합니다.";
//                 yield return new WaitForSeconds(4f);
//                 showGUI = false;
//                 isCompleted = false;
//                 if (curr_idx == SbjManager.sbj_fam_order.Length - 1)
//                 {        
//                     SbjManager.showFixation = false;    
//                 }
//             }
//         }

//         if (SbjManager.sbj_task <= 4)
//         {
//             showGUI = true;
//             guiMessage = "이제 피드백을 통해\n버섯의 위치를 학습합니다.";
//             yield return new WaitForSeconds(4f);
//             showGUI = false;
//             showGUI = true;
//             guiMessage = "등장하는 텃밭에서 버섯들을 찾아주세요.";
//             yield return new WaitForSeconds(4f);
//             showGUI = false;

//             for (curr_idx = 0; curr_idx < SbjManager.sbj_learn_order.Length; curr_idx++)
//             {
//                 Debug.Log($"[{string.Join(", ", SbjManager.sbj_learn_order)}]");

//                 int order = SbjManager.sbj_learn_order[curr_idx];
//                 string sceneName = order == 0 ? "LearnScene0" : "LearnScene4";
//                 Debug.Log($"=== {sceneName} ready");
//                 curr_env = order;

//                 SbjManager.curr_task = "learning";
//                 SbjManager.curr_scene = sceneName;
//                 SbjManager.curr_trial = curr_idx + 1;
//                 SbjManager.curr_order = SbjManager.sbj_learn_order[curr_idx];

//                 AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);                
//                 yield return new WaitUntil(() => isCompleted);
//                 asyncLoad.allowSceneActivation = false;
//                 yield return new WaitForSeconds(6f);
//                 isCompleted = false;
//                 if (curr_idx == SbjManager.sbj_learn_order.Length - 1)
//                 {        
//                     SbjManager.showFixation = false;    
//                 }
//             }
//         }

//         if (SbjManager.sbj_task <= 5)
//         {
//             SbjManager.env_rep = new int[5];// 초기화

//             showGUI = true;
//             guiMessage = "이제 피드백이 제시되지 않습니다.\n\n텃밭에 사는 동물들이\n먹을 버섯을 찾아주세요.";
//             yield return new WaitForSeconds(4f);
//             showGUI = false;

//             for (curr_idx = 0; curr_idx < SbjManager.sbj_test_order.Length; curr_idx++)
//             {
//                 Debug.Log($"[{string.Join(", ", SbjManager.sbj_test_order)}]");
                
//                 int order = SbjManager.sbj_test_order[curr_idx];
//                 string sceneName = $"TestScene{order}";
//                 curr_env = order;

//                 SbjManager.curr_task = "testing";
//                 SbjManager.curr_scene = sceneName;
//                 SbjManager.curr_trial = curr_idx + 1;
//                 SbjManager.curr_order = SbjManager.sbj_test_order[curr_idx];

//                 Debug.Log($"== flowers in curr env: env#{SbjManager.curr_order}, {SbjManager.remapflowers[SbjManager.curr_order, SbjManager.env_rep[SbjManager.curr_order]]}");
                
//                 AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);                
//                 yield return new WaitUntil(() => isCompleted);
//                 asyncLoad.allowSceneActivation = false;
//                 isCompleted = false;
//                 yield return new WaitForSeconds(6f);
//                 SbjManager.env_rep[curr_env]++;

//                 if (curr_idx == SbjManager.sbj_test_order.Length - 1)
//                 {        
//                     SbjManager.showFixation = false;
//                 }
//             }
//         }
//         // showGUI = true;
//         // guiMessage = "휴식 시간입니다.\n다음 세션을 위해 준비해주세요.\n(1분)";
//         // yield return new WaitForSeconds(50f);
//         // showGUI = false;
//         showGUI = true;
//         guiMessage = "휴식 시간입니다.\n휴식이 끝나면 스페이스 바를 누르세요.";
//         yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
//         showGUI = false;

//         SbjManager.showFixation = true; // task 전환 시 번쩍 제거용
//     }

//     IEnumerator ReverseTaskplayer()
//     {   
//         SbjManager.sbj_fam_order = GenerateFamVector();
//         SbjManager.sbj_learn_order = GenerateLearnVector();
//         SbjManager.sbj_test_order = GenerateTestVector();
//         SbjManager.writer2.WriteLine($"familiarization: [{string.Join(", ", SbjManager.sbj_fam_order)}]");
//         SbjManager.writer2.WriteLine($"learning: [{string.Join(", ", SbjManager.sbj_learn_order)}]");
//         SbjManager.writer2.WriteLine($"testing: [{string.Join(", ", SbjManager.sbj_test_order)}]");
//         SbjManager.writer2.Flush();

//         Debug.Log("=== Reverse Remapping Taskplayer started");
//         SbjManager.showFixation = false; // task 전환 시 번쩍 제거용
//         showGUI = true;
//         guiMessage = "텃밭에 지각변동이 일어났습니다!";
//         yield return new WaitForSeconds(2f);
//         showGUI = false;

//         showGUI = true;
//         guiMessage = "맛있는 버섯과\n독버섯의 위치가 바뀌었습니다.";
//         yield return new WaitForSeconds(4f);
//         showGUI = false;

//         if (SbjManager.sbj_task <= 7)
//         {
//             showGUI = true;
//             guiMessage = "등장하는 텃밭에서\n두 가지 버섯의 위치를 기억하세요.\n(45초)";
//             yield return new WaitForSeconds(6f);
//             showGUI = false;

//             for (curr_idx = 0; curr_idx < SbjManager.sbj_fam_order.Length; curr_idx++)
//             {
                
//                 Debug.Log($"[{string.Join(", ", SbjManager.sbj_fam_order)}]");
//                 int order = SbjManager.sbj_fam_order[curr_idx];
//                 string sceneName = order == 0 ? "FamScene0" : "FamScene4";
//                 Debug.Log($"=== {sceneName} ready");
//                 curr_env = order;

//                 SbjManager.curr_task = "rfam";
//                 SbjManager.curr_scene = sceneName;
//                 SbjManager.curr_trial = curr_idx + 1;
//                 SbjManager.curr_order = SbjManager.sbj_fam_order[curr_idx];

//                 AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);                
//                 yield return new WaitUntil(() => isCompleted);
//                 asyncLoad.allowSceneActivation = false;
//                 isCompleted = false;
//                 yield return new WaitForSeconds(6f);
//                 if (curr_idx == SbjManager.sbj_fam_order.Length - 1)
//                 {        
//                     SbjManager.showFixation = false;    
//                 }
//             }
//         }

//         if (SbjManager.sbj_task <= 8)
//         {
//             showGUI = true;
//             guiMessage = "이제 피드백을 통해\n버섯의 위치를 학습합니다.";
//             yield return new WaitForSeconds(6f);
//             showGUI = false;

//             for (curr_idx = 0; curr_idx < SbjManager.sbj_learn_order.Length; curr_idx++)
//             {
//                 Debug.Log($"[{string.Join(", ", SbjManager.sbj_learn_order)}]");

//                 int order = SbjManager.sbj_learn_order[curr_idx];
//                 string sceneName = order == 0 ? "LearnScene0" : "LearnScene4";
//                 Debug.Log($"=== {sceneName} ready");
//                 curr_env = order;

//                 SbjManager.curr_task = "rlearning";
//                 SbjManager.curr_scene = sceneName;
//                 SbjManager.curr_trial = curr_idx + 1;
//                 SbjManager.curr_order = SbjManager.sbj_learn_order[curr_idx];
                
//                 AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);                
//                 yield return new WaitUntil(() => isCompleted);
//                 asyncLoad.allowSceneActivation = false;
//                 isCompleted = false;
//                 yield return new WaitForSeconds(6f);
//                 if (curr_idx == SbjManager.sbj_learn_order.Length - 1)
//                 {        
//                     SbjManager.showFixation = false;    
//                 }
//             }
//         }

//         if (SbjManager.sbj_task <= 9)
//         {            
//             Reversalenvrep(); // env_rep_idx 만들기
//             showGUI = true;
//             guiMessage = "이제 피드백이 제시되지 않습니다.\n\n텃밭에 사는 동물들이\n먹을 버섯을 찾아주세요.";
//             yield return new WaitForSeconds(6f);
//             showGUI = false;

//             for (curr_idx = 0; curr_idx < SbjManager.sbj_test_order.Length; curr_idx++)
//             {
//                 Debug.Log($"[{string.Join(", ", SbjManager.sbj_test_order)}]");
                
//                 int order = SbjManager.sbj_test_order[curr_idx];
//                 string sceneName = $"TestScene{order}";
//                 Debug.Log($"Testing {sceneName} ready");
//                 curr_env = order;

//                 SbjManager.curr_task = "rtesting";
//                 SbjManager.curr_scene = sceneName;
//                 SbjManager.curr_trial = curr_idx + 1;
//                 SbjManager.curr_order = SbjManager.sbj_test_order[curr_idx];

//                 int rep_idx = r_env_idx[order];
//                 Debug.Log($"== flowers in curr env: env#{SbjManager.curr_order}, {SbjManager.remapflowers[SbjManager.curr_order, r_env_rep[order, rep_idx]]}");
                
//                 r_env_idx[order]++; // 다음에 쓸 인덱스를 위해 증가
//                 AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);                
//                 yield return new WaitUntil(() => isCompleted);
//                 asyncLoad.allowSceneActivation = false;
//                 isCompleted = false;
//                 yield return new WaitForSeconds(6f);

//                 if (curr_idx == SbjManager.sbj_test_order.Length - 1)
//                 {        
//                     SbjManager.showFixation = false;    
//                 }
//             }
//         }
//         // showGUI = true;
//         // guiMessage = "휴식 시간입니다.\n다음 세션을 위해 준비해주세요.\n(1분)";
//         // yield return new WaitForSeconds(50f);
//         // showGUI = false;
//         showGUI = true;
//         guiMessage = "휴식 시간입니다.\n휴식이 끝나면 스페이스 바를 누르세요.";
//         yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
//         showGUI = false;

//         SbjManager.showFixation = true; // task 전환 시 번쩍 제거용
//     }

//     // =================================================================================
//     void Reversalenvrep()
//     {
//         for (int env = 0; env < 5; env++)
//         {
//             List<int> indices = Enumerable.Range(0, 5).OrderBy(x => Random.value).ToList();
//             for (int i = 0; i < 5; i++)
//             {
//                 r_env_rep[env, i] = indices[i];
//             }
//             r_env_idx[env] = 0; // 인덱스 카운터 초기화
//         }
//     }
//     void LoadSprites()
//     {
//         Sprite[] loadedSprites = Resources.LoadAll<Sprite>("flowers");
//         List<Sprite> flowerCandidates = new List<Sprite>();

//         foreach (Sprite sprite in loadedSprites)
//         {
//             if (sprite.name == "Magic flowers-82" && target == null)
//             {
//                 target = sprite;
//             }
//             else
//             {
//                 flowerCandidates.Add(sprite);
//             }
//         }
//         flowerCandidates = flowerCandidates.OrderBy(x => Random.value).ToList();
//         for (int i = 0; i < flowerList.Length; i++)
//         {
//             if (i < flowerCandidates.Count)
//             {
//                 flowerList[i] = flowerCandidates[i];
//             }
//             else
//             {
//                 flowerList[i] = null; // 명시적 null (디버깅에 도움됨)
//             }
//         }


//         // remap flowers allocation
//         for (int i = 0; i < 5; i++) // i: row
//         {
//             for (int j = 0; j < 4; j++) // j: column
//             {
//                 int index = i * 4 + j;
//                 SbjManager.remapflowers[i, j] = flowerList[index].name;
//             }
//         }

//         for (int i = 0; i < 5; i++)
//         {
//             List<string> rowValues = new List<string>();
//             for (int j = 0; j < 4; j++)
//             {
//                 rowValues.Add(SbjManager.remapflowers[i, j]);
//             }

//             string rowLog = string.Join(", ", rowValues);
//             Debug.Log($"REMAP FLOWERS - row {i}: [{rowLog}]");
//             SbjManager.odtWriter1.WriteLine($"remapping flowers - row {i}: [{rowLog}]");
//         }
//         SbjManager.odtWriter1.Flush();
//     }

//     void GenerateSequences()
//     {
//         for (int seq = 0; seq < 3; seq++)
//         {
//             List<Sprite> normalList = new List<Sprite>(flowerList);
//             Sprite[] sequence = new Sprite[24];

//             // 1. 타겟 위치 미리 정하기 (예: 평균 7칸 간격, 랜덤하게 흩어짐)
//             List<int> targetPositions = new List<int>();
//             while (targetPositions.Count < 4)
//             {
//                 int pos = Random.Range(0, 20); // 최대한 앞쪽에서 분산
//                 // 간격 너무 좁지 않게 (예: 3칸 이상 떨어지도록)
//                 if (targetPositions.All(p => Mathf.Abs(p - pos) >= 3))
//                 {
//                     targetPositions.Add(pos);
//                 }
//             }
//             targetPositions.Sort();

//             // 2. 타겟 배치
//             foreach (int idx in targetPositions)
//             {
//                 sequence[idx] = target;
//             }

//             // 3. 나머지 위치에 일반 꽃 무작위로 배치
//             for (int i = 0; i < 24; i++)
//             {
//                 if (sequence[i] == null && normalList.Count > 0)
//                 {
//                     int randIdx = Random.Range(0, normalList.Count);
//                     sequence[i] = normalList[randIdx];
//                     normalList.RemoveAt(randIdx);
//                 }
//             }

//             // 저장
//             for (int i = 0; i < 24; i++)
//             {
//                 SbjManager.flowerSeq[seq, i] = sequence[i];
//             }

//             // 디버깅 출력
//             string line = string.Join(" ", sequence.Select(s => s.name));
//             Debug.Log($"[FlowerSequence {seq}] {line}");
//             SbjManager.odtWriter1.WriteLine(line);
//             SbjManager.odtWriter1.Flush();
//         }
//     }

//     void ChunkSetting()
//     {
//         // coin sequence
//         chunkOrder = new List<int[][]>();
//         chunkOrder.Add(new int[][] { // seq set 1
//             new int[] { 2, 1, 8, 7, 4, 5, 3, 6 },
//             new int[] { 4, 3, 5, 2, 7, 8, 1, 6 },
//             new int[] { 8, 5, 4, 2, 3, 6, 7, 1 }
//         });
 
//         chunkOrder.Add(new int[][] {
//             new int[] { 7, 4, 8, 3, 5, 6, 2, 1 },
//             new int[] { 1, 2, 7, 6, 8, 5, 4, 3 },
//             new int[] { 8, 1, 4, 6, 5, 3, 2, 7 }
//         });

//         chunkOrder.Add(new int[][] {
//             new int[] { 7, 8, 1, 4, 5, 3, 2, 6 },
//             new int[] { 3, 5, 4, 1, 2, 7, 6, 8 },
//             new int[] { 1, 8, 7, 4, 6, 2, 3, 5 }
//         });

//         chunkOrder.Add(new int[][] {
//             new int[] { 4, 7, 8, 3, 1, 2, 5, 6 },
//             new int[] { 2, 3, 5, 7, 4, 1, 8, 6 },
//             new int[] { 8, 1, 4, 6, 3, 2, 8, 5 }
//         });

//         chunkOrder.Add(new int[][] {
//             new int[] { 7, 6, 4, 5, 8, 1, 2, 3 },
//             new int[] { 3, 1, 6, 2, 7, 8, 5, 4 },
//             new int[] { 4, 5, 3, 2, 6, 1, 8, 7 }
//         });

//         // 2. 세션 별로 청크 쑤도랜덤하게 선택
//         seqList = new List<int[][]>();
//         seqList.Add(new int[][] { // seq set 1
//             new int[] { 1, 0, 3, 2, 4 },
//             new int[] { 3, 4, 0, 1, 2 },
//             new int[] { 0, 2, 1, 4, 3 }
//         });
//         seqList.Add(new int[][] { // seq set 1
//             new int[] { 2, 1, 0, 4, 3 },
//             new int[] { 0, 3, 1, 2, 4},
//             new int[] { 3, 4, 2, 1, 0 }
//         });
//         seqList.Add(new int[][] { // seq set 1
//             new int[] { 1, 4, 2, 3, 0 },
//             new int[] { 0, 2, 3, 4, 1},
//             new int[] { 2, 3, 1, 0, 4 }
//         });
//         seqList.Add(new int[][] { // seq set 1
//             new int[] { 0, 1, 2, 3, 4 },
//             new int[] { 2, 4, 0, 1, 3 },
//             new int[] { 4, 2, 3, 0, 1 }
//         });

//         seqOrder = new int[3, 5];
//         int i = UnityEngine.Random.Range(0, seqList.Count); // 랜덤 인덱스 선택
//         int[][] selectedSeq = seqList[i]; // 선택된 시퀀스 세트
//         for (int row = 0; row < 3; row++)
//         {
//             for (int col = 0; col < 5; col++)
//             {
//                 seqOrder[row, col] = selectedSeq[row][col];
//             }
//         }
//         subseqOrder = new int[3, 3]
//         {
//             { 0, 1, 2 },
//             { 1, 0, 2 },
//             { 2, 0, 1 }
//         };

//         for (int row = 0; row < 3; row++)
//         {
//             List<int> values = new List<int> { 0, 1, 2 };
//             // 랜덤 셔플
//             values = values.OrderBy(_ => UnityEngine.Random.value).ToList();

//             for (int col = 0; col < 3; col++)
//             {
//                 subseqOrder[row, col] = values[col];
//             }
//         }

//         playerStart = new List<Vector3[]>();

//         playerStart.Add(new Vector3[]
//         {
//             new Vector3(-4.8f, 0.91f, -7.1f),
//             new Vector3(-10.2f, 0.91f, -4.6f),
//             new Vector3(8.2f, 0.91f, 1.9f)
//         });

//         playerStart.Add(new Vector3[]
//         {
//             new Vector3(7.0f, 0.91f, 6.4f),
//             new Vector3(-1.9f, 0.91f, 0.8f),
//             new Vector3(7.3f, 0.91f, -1.8f)
//         });

//         playerStart.Add(new Vector3[]
//         {
//             new Vector3(3.9f, 0.91f, -2.5f),
//             new Vector3(2.9f, 0.91f, 3.6f),
//             new Vector3(-10.4f, 0.91f, 6.1f)
//         });

//         playerStart.Add(new Vector3[]
//         {
//             new Vector3(5.1f, 0.91f, -5.2f),
//             new Vector3(-7.9f, 0.91f, 7.4f),
//             new Vector3(3.6f, 0.91f, 8.3f)
//         });

//         playerStart.Add(new Vector3[]
//         {
//             new Vector3(7.0f, 0.91f, -6.8f),
//             new Vector3(-10.1f, 0.91f, 3.0f),
//             new Vector3(1.3f, 0.91f, -8.8f)
//         });
//     }

//     void CoinSetting()
//     {        
//         // 1. 청크 별로 스폰할 위치값 정하기
//         List<Vector3> chunkCenter = new List<Vector3>();
//         float chunkDia = 0f;
//         List<Vector3> chunkLoc = new List<Vector3>();

//         for (int i = 1; i <= 8; i++)
//         {
//             GameObject chunkObj = GameObject.Find($"chunk{i}"); // 각 청크의 중심좌표
//             chunkCenter.Add(chunkObj.transform.position);
//         }
//         chunkDia = chunkCenter.Count > 0 ? GameObject.Find("chunk1").transform.localScale.x : 0f; // 지름크기
      
//         float radius = chunkDia / 6f;
//         float y = 0f; // 씨드 Y 높이
//         foreach (var center in chunkCenter)
//         {
//             float angle = Random.Range(0f, 360f);
//             float dist = Random.Range(0f, radius);
//             float x = center.x + Mathf.Cos(angle * Mathf.Deg2Rad) * dist;
//             float z = center.z + Mathf.Sin(angle * Mathf.Deg2Rad) * dist;
//             Vector3 newPos = new Vector3(x, y, z);
            
//             chunkLoc.Add(newPos);
//         }

//         // 2. 현재 sess, run, env 고려해서 청크 순서 불러오기
//         int setidx = seqOrder[SbjManager.curr_coin_sess, curr_env];
//         int subsetidx = subseqOrder[SbjManager.curr_coin_sess, SbjManager.curr_coin_run];
//         SbjManager.curr_coin_seq = chunkOrder[setidx][subsetidx];
//         Debug.Log($"[{string.Join(", ", SbjManager.curr_coin_seq)}]");

//         // 3. 해당 seq set에 맞게 코인 위치값 할당하기
//         for (int trialidx = 0; trialidx < SbjManager.numTrial; trialidx++)
//         {
//             SbjManager.coinLoc[trialidx] = chunkLoc[SbjManager.curr_coin_seq[trialidx] - 1];
//         }

//         // 4. player starting location 정하기
//         SbjManager.coin_start_loc = playerStart[setidx][subsetidx];
//         Debug.Log($"SbjManager.start_loc = {SbjManager.coin_start_loc}");

//         // for (int locidx = 1; locidx < 9; locidx++)
//         // {
//         //     int firstchunk = SbjManager.curr_coin_seq[0];
//         //     SbjManager.start_loc = playerLoc[firstchunk][Random.Range(0, playerLoc[firstchunk].Count)];
//         // }
//     }

//     GameObject[] FindObjectsByNamePrefix(string prefix)
//     {
//         GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
//         List<GameObject> matchedObjects = new List<GameObject>();

//         foreach (GameObject obj in allObjects)
//         {
//             if (obj.name.StartsWith(prefix))
//             {
//                 matchedObjects.Add(obj);
//             }
//         }

//         return matchedObjects.ToArray();
//     }

//     void Logfile()
//     {
//         SbjManager.writer1.WriteLine("===Remapping task 1");
//         SbjManager.writer1.WriteLine($"Subject ID: {SbjManager.sbj_num}");
//         SbjManager.writer1.WriteLine("Date: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
//         SbjManager.writer1.Flush();
//         SbjManager.writer2.WriteLine("===Remapping task 2");
//         SbjManager.writer2.WriteLine($"Subject ID: {SbjManager.sbj_num}");
//         SbjManager.writer2.WriteLine("Date: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
//         SbjManager.writer2.Flush();

//         SbjManager.coinWriter1.WriteLine("===Foraging task 1");
//         SbjManager.coinWriter1.WriteLine($"Subject ID: {SbjManager.sbj_num}");
//         SbjManager.coinWriter1.WriteLine("Date: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
//         SbjManager.coinWriter1.Flush();   
//         SbjManager.coinWriter2.WriteLine("===Foraging task 2");
//         SbjManager.coinWriter2.WriteLine($"Subject ID: {SbjManager.sbj_num}");
//         SbjManager.coinWriter2.WriteLine("Date: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
//         SbjManager.coinWriter2.Flush();   
//         SbjManager.coinWriter3.WriteLine("===Foraging task 3");
//         SbjManager.coinWriter3.WriteLine($"Subject ID: {SbjManager.sbj_num}");
//         SbjManager.coinWriter3.WriteLine("Date: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
//         SbjManager.coinWriter3.Flush();   

//         SbjManager.timeWriter.WriteLine("===fMRI Time Log");
//         SbjManager.timeWriter.WriteLine($"Subject ID: {SbjManager.sbj_num}");
//         SbjManager.timeWriter.WriteLine("Date: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
//         SbjManager.timeWriter.Flush();

//         SbjManager.odtWriter1.WriteLine("===Object Detection Task 1");
//         SbjManager.odtWriter1.WriteLine($"Subject ID: {SbjManager.sbj_num}");
//         SbjManager.odtWriter1.WriteLine("Date: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
//         SbjManager.odtWriter1.Flush();
//         SbjManager.odtWriter2.WriteLine("===Object Detection Task 2");
//         SbjManager.odtWriter2.WriteLine($"Subject ID: {SbjManager.sbj_num}");
//         SbjManager.odtWriter2.WriteLine("Date: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
//         SbjManager.odtWriter2.Flush();
//     }


//     // condition order(shape, distal, time)
//     int[] GenerateCondVector()
//     {
//         int[][] predefinedOrders =
//         {
//             new int[] { 1, 2, 3 },
//             new int[] { 2, 3, 1 },
//             new int[] { 3, 1, 2 }
//         };

//         // 0, 1, 2 중 랜덤하게 하나 선택
//         int index = UnityEngine.Random.Range(0, predefinedOrders.Length);
//         return predefinedOrders[index];
//     }

//     // scene order generator(randomize within block)
//     // Familiarization Phase (2 trials)
//     int[] GenerateFamVector()
//     {
//         int[] numbers = { 0,1 };
//         return GenerateRandomSeq(numbers, SbjManager.sbj_fam_order.Length, numbers.Length);
//     }

//     // Learning Phase (6 trials)
//     int[] GenerateLearnVector()
//     {
//         int[] numbers = { 0,1 };
//         return GenerateRandomSeq(numbers, SbjManager.sbj_learn_order.Length, numbers.Length);
//     }

//     // Testing Phase (60 trials)
//     int[] GenerateTestVector()
//     {
//         int[] numbers = { 0, 1, 2, 3, 4};
//         return GenerateRandomSeq(numbers, SbjManager.sbj_test_order.Length, numbers.Length);
//     }
//     List<int[]> GenerateCoinOrder(int setCount)
//     {
//         List<int[]> coinOrder = new List<int[]>();

//         for (int i = 0; i < setCount; i++)
//         {
//             int[] numbers = { 0, 1, 2, 3, 4 };
//             int[] shuffled = numbers.OrderBy(_ => UnityEngine.Random.value).ToArray();
//             coinOrder.Add(shuffled);
//         }

//         return coinOrder;
//     }
//     int[] GenerateRandomSeq(int[] numbers, int length, int blockSize)
//     {
//         List<int> finalList = new List<int>();

//         int fullBlocks = length / blockSize; 
//         int remainder = length % blockSize; 

//         for (int i = 0; i < fullBlocks; i++)
//         {
//             int[] shuffledBlock = numbers.OrderBy(_ => UnityEngine.Random.value).ToArray();
//             finalList.AddRange(shuffledBlock);
//         }

//         if (remainder > 0)
//         {
//             int[] shuffledBlock = numbers.OrderBy(_ => UnityEngine.Random.value).ToArray();
//             finalList.AddRange(shuffledBlock.Take(remainder));
//         }

//         return finalList.ToArray();
//     }
//     Texture2D SpriteToTexture(Sprite sprite)
//     {
//         int x = Mathf.RoundToInt(sprite.textureRect.x);
//         int y = Mathf.RoundToInt(sprite.textureRect.y);
//         int width = Mathf.RoundToInt(sprite.textureRect.width);
//         int height = Mathf.RoundToInt(sprite.textureRect.height);

//         Color[] pixels = sprite.texture.GetPixels(x, y, width, height);

//         Texture2D newTex = new Texture2D(width, height);
//         newTex.SetPixels(pixels);
//         newTex.Apply();

//         return newTex;
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
//             guiStyle.fontSize = 100;
//             guiStyle.fontStyle = FontStyle.Bold;
//             guiStyle.normal.textColor = Color.black;
//             guiStyle.alignment = TextAnchor.MiddleCenter; 
//             GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 200, 100), guiMessage, guiStyle);
//         }

//         if (SbjManager.showFixation)
//         {
//             Texture2D blackTexture = new Texture2D(1, 1);
//             blackTexture.SetPixel(0, 0, new Color32(217, 217, 217, 255));
//             blackTexture.Apply();

//             GUIStyle bgStyle = new GUIStyle();
//             bgStyle.normal.background = blackTexture;

//             GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, bgStyle);

//             GUIStyle textStyle = new GUIStyle();
//             textStyle.fontSize = 100;
//             textStyle.alignment = TextAnchor.MiddleCenter;
//             textStyle.normal.textColor = Color.black;

//             Rect rect = new Rect(Screen.width / 2 - 25, Screen.height / 2 - 25, 50, 50);
//             GUI.Label(rect, "+", textStyle);
//         }

//         if (showGUIimage)
//         {
//             Texture2D blackTexture = new Texture2D(1, 1);
//             blackTexture.SetPixel(0, 0, new Color32(217, 217, 217, 255));
//             blackTexture.Apply();
//             float imgSize = 256f;
            
//             GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);

//             Rect imgRect = new Rect(
//             Screen.width / 2 - imgSize / 2,  // X: 화면 중앙 정렬
//             Screen.height - imgSize - 80,   // Y: 하단에서 위로 띄움
//             imgSize,
//             imgSize);
//             GUI.DrawTexture(imgRect, flowerTexture, ScaleMode.ScaleToFit, true);

//             GUIStyle style = new GUIStyle(GUI.skin.label);
//             style.fontSize = 80;
//             style.normal.textColor = Color.black;
//             style.fontStyle = FontStyle.Bold;
//             style.alignment = TextAnchor.MiddleCenter;
//             style.wordWrap = true;

//             Rect messageRect = new Rect(Screen.width / 2 - 500, Screen.height / 2 - 200, 1000, 400);
//             GUI.Label(messageRect, guiMessage, style);
//         }

//     }

//     int ExtractNumber(string fileName)
//     {  
//         var match = System.Text.RegularExpressions.Regex.Match(fileName, @"\d+");
//         return match.Success ? int.Parse(match.Value) : int.MaxValue; 
//     }

//     string GetNextTaskType(string currentTask)
//     {
//         if (currentTask == "Fam") return "Learning";
//         if (currentTask == "Learning") return "Testing";
//         return "";
//     }

//     int[] GetNextOrder(string taskType)
//     {
//         if (taskType == "Fam") return SbjManager.sbj_fam_order;
//         if (taskType == "Learning") return SbjManager.sbj_learn_order;
//         if (taskType == "Testing") return SbjManager.sbj_test_order;
//         return new int[0];
//     }
//     public void OnSceneCompleted()
//     {
//         isCompleted = true;
//     }
//     void OnDestroy()
//     {
//         SbjManager.CloseLogger();
//     }

//     // flowerSeq를 텍스트 파일로 저장하는 메서드
//     void SaveFlowerSequencesToFile()
//     {
//         string filename = $"flowerSeq_{SbjManager.sbj_num}_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt";
//         string filepath = Path.Combine(ori_dir, filename);
        
//         using (StreamWriter writer = new StreamWriter(filepath))
//         {
//             for (int seq = 0; seq < 3; seq++)
//             {
//                 List<string> flowerNames = new List<string>();
//                 for (int i = 0; i < 24; i++)
//                 {
//                     if (SbjManager.flowerSeq[seq, i] != null)
//                     {
//                         flowerNames.Add(SbjManager.flowerSeq[seq, i].name);
//                     }
//                     else
//                     {
//                         flowerNames.Add("null");
//                     }
//                 }
//                 string line = string.Join("|", flowerNames); // 구분자를 공백에서 | 로 변경
//                 writer.WriteLine(line);
//             }
//         }
//     }
    
//     // remapflowers를 텍스트 파일로 저장하는 메서드
//     void SaveRemapFlowersToFile()
//     {
//         string filename = $"remapflowers_{SbjManager.sbj_num}_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt";
//         string filepath = Path.Combine(ori_dir, filename);
        
//         using (StreamWriter writer = new StreamWriter(filepath))
//         {
//             for (int i = 0; i < 5; i++) // 5개 환경
//             {
//                 List<string> rowValues = new List<string>();
//                 for (int j = 0; j < 4; j++) // 각 환경별 4개 꽃
//                 {
//                     rowValues.Add(SbjManager.remapflowers[i, j]);
//                 }
//                 string line = string.Join("|", rowValues); // 구분자를 공백에서 | 로 변경
//                 writer.WriteLine(line);
//             }
//         }
//     }
    
//     // 텍스트 파일에서 flowerSeq를 불러오는 메서드
//     void LoadFlowerSequencesFromFile()
//     {
//         string[] files = Directory.GetFiles(ori_dir, "flowerSeq_*.txt");
//         if (files.Length == 0)
//         {
//             Debug.LogError("⚠️ flowerSeq 파일을 찾을 수 없습니다. 새로 생성합니다.");
//             LoadSprites();
//             GenerateSequences();
//             SaveFlowerSequencesToFile();
//             return;
//         }
        
//         // 파일 생성 시간 기준으로 정렬하여 가장 최근 파일 사용
//         string latestFile = files.OrderByDescending(f => File.GetCreationTime(f)).First();
//         Debug.Log($"Loading flowerSeq from file: {latestFile}");
        
//         try
//         {
//             string[] lines = File.ReadAllLines(latestFile);
//             Debug.Log($"Found {lines.Length} sequences in file");
            
//             for (int seq = 0; seq < lines.Length && seq < 3; seq++)
//             {
//                 // 파이프(|) 구분자로 분리 시도
//                 string[] flowerNames = lines[seq].Split('|');
                
//                 // 파이프로 분리한 결과가 충분하지 않으면 공백으로 구분된 파일로 처리
//                 if (flowerNames.Length < 24 && lines[seq].Contains("Magic flowers"))
//                 {
//                     Debug.Log($"파이프 구분자로 분리 실패, 공백 구분자로 다시 시도 (항목 수: {flowerNames.Length})");
//                     // "Magic flowers-XX" 패턴을 찾아 재구성
//                     List<string> reconstructed = new List<string>();
//                     string line = lines[seq];
                    
//                     // 정규식을 사용하여 "Magic flowers-숫자" 패턴 추출
//                     MatchCollection matches = Regex.Matches(line, @"Magic flowers-\d+");
//                     foreach (Match match in matches)
//                     {
//                         reconstructed.Add(match.Value);
//                     }
                    
//                     Debug.Log($"정규식으로 추출한 꽃 이름 수: {reconstructed.Count}");
                    
//                     if (reconstructed.Count > 0)
//                     {
//                         flowerNames = reconstructed.ToArray();
//                     }
//                 }
                
//                 Debug.Log($"Sequence {seq} has {flowerNames.Length} flowers after processing");
                
//                 for (int i = 0; i < flowerNames.Length && i < 24; i++)
//                 {
//                     if (flowerNames[i] != "null")
//                     {
//                         Sprite flowerSprite = Resources.Load<Sprite>("flowers/" + flowerNames[i]);
//                         if (flowerSprite != null)
//                         {
//                             SbjManager.flowerSeq[seq, i] = flowerSprite;
//                         }
//                         else
//                         {
//                             Debug.LogWarning($"Could not load sprite for flower: {flowerNames[i]}, using target instead");
//                             if (target == null)
//                             {
//                                 target = Resources.Load<Sprite>("flowers/Magic flowers-82");
//                             }
//                             SbjManager.flowerSeq[seq, i] = target; // 찾을 수 없는 경우 타겟으로 대체
//                         }
//                     }
//                     else
//                     {
//                         SbjManager.flowerSeq[seq, i] = null;
//                     }
//                 }
//             }
            
//             // 로그에 기록
//             for (int seq = 0; seq < 3; seq++)
//             {
//                 List<string> flowerNames = new List<string>();
//                 for (int i = 0; i < 24; i++)
//                 {
//                     if (SbjManager.flowerSeq[seq, i] != null)
//                     {
//                         flowerNames.Add(SbjManager.flowerSeq[seq, i].name);
//                     }
//                     else
//                     {
//                         flowerNames.Add("null");
//                     }
//                 }
//                 string seqLog = string.Join(", ", flowerNames);
//                 Debug.Log($"LOADED FLOWER SEQUENCE {seq}: [{seqLog}]");
//                 SbjManager.odtWriter1.WriteLine($"loaded flower sequence {seq}: [{seqLog}]");
//             }
//             SbjManager.odtWriter1.Flush();
//         }
//         catch (Exception ex)
//         {
//             Debug.LogError($"⚠️ flowerSeq 파일을 읽는 중 오류가 발생했습니다: {ex.Message}");
//             Debug.LogException(ex);
//             LoadSprites();
//             GenerateSequences();
//             SaveFlowerSequencesToFile();
//         }
//     }

//     // 텍스트 파일에서 remapflowers를 불러오는 메서드
//     void LoadRemapFlowersFromFile()
//     {
//         string[] files = Directory.GetFiles(ori_dir, "remapflowers_*.txt");
//         if (files.Length == 0)
//         {
//             Debug.LogError("⚠️ remapflowers 파일을 찾을 수 없습니다. 새로 생성합니다.");
//             if (target == null)
//             {
//                 LoadSprites();
//             }
//             SaveRemapFlowersToFile();
//             return;
//         }
        
//         // 파일 생성 시간 기준으로 정렬하여 가장 최근 파일 사용
//         string latestFile = files.OrderByDescending(f => File.GetCreationTime(f)).First();
        
//         try
//         {
//             string[] lines = File.ReadAllLines(latestFile);
//             for (int i = 0; i < lines.Length && i < 5; i++) // 5개 환경
//             {
//                 // 구분자를 공백에서 | 로 변경
//                 string[] flowerNames = lines[i].Split('|');
                
//                 // 기존 파일과의 호환성을 위한 코드 (공백 구분)
//                 if (flowerNames.Length < 4 && lines[i].Contains("Magic flowers"))
//                 {
//                     // 공백으로 나뉜 경우 "Magic flowers-XX" 패턴을 사용해 재구성
//                     string[] tokens = lines[i].Split(' ');
//                     List<string> reconstructed = new List<string>();
                    
//                     for (int t = 0; t < tokens.Length; t++)
//                     {
//                         if (tokens[t] == "Magic" && t + 1 < tokens.Length && tokens[t+1].StartsWith("flowers-"))
//                         {
//                             reconstructed.Add("Magic " + tokens[t+1]);
//                             t++; // flowers 부분 건너뛰기
//                         }
//                     }
                    
//                     if (reconstructed.Count >= 4)
//                     {
//                         flowerNames = reconstructed.ToArray();
//                     }
//                 }
                
//                 for (int j = 0; j < flowerNames.Length && j < 4; j++) // 각 환경별 4개 꽃
//                 {
//                     SbjManager.remapflowers[i, j] = flowerNames[j];
//                 }
//             }
            
//             // 로그에 기록
//             for (int i = 0; i < 5; i++)
//             {
//                 List<string> rowValues = new List<string>();
//                 for (int j = 0; j < 4; j++)
//                 {
//                     rowValues.Add(SbjManager.remapflowers[i, j]);
//                 }
//                 string rowLog = string.Join(", ", rowValues);
//                 Debug.Log($"LOADED REMAP FLOWERS - row {i}: [{rowLog}]");
//                 SbjManager.odtWriter1.WriteLine($"loaded remapping flowers - row {i}: [{rowLog}]");
//             }
//             SbjManager.odtWriter1.Flush();
//         }
//         catch (Exception ex)
//         {
//             Debug.LogError($"⚠️ remapflowers 파일을 읽는 중 오류가 발생했습니다: {ex.Message}");
//             if (target == null)
//             {
//                 LoadSprites();
//             }
//             SaveRemapFlowersToFile();
//         }
//     }
// }