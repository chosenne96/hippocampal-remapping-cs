using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Linq;
using Random=UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;


public class RemapManager_bhv : MonoBehaviour
{
    GameObject player, target1, target2, target3, target4;
    // directory
    private string ori_dir, bhv1_dir, bhv2_dir, time_dir, coin1_dir, coin2_dir, coin3_dir, tmpDate;
    // task information
    private int curr_idx = 0;
    private int curr_env = 0;
    public int[,] seqOrder;
    public int[,] subseqOrder;
    public List<int[][]> chunkOrder; // 클래스 멤버 변수로 선언
    public List<Vector3[]> playerStart;
    public List<int[][]> seqList;
    // int[,] r_env_rep = new int[5, 5];  // remap 인덱스 랜덤 저장
    // int[] r_env_idx = new int[5];       


    // input ui
    public GameObject InputCanvas;
    public InputField sbjNumInput;
    public InputField sbjTaskInput; // SbjManager.sbj_task
    public InputField modeInput; // target mode
    public Button submitButton;
    private int mode = 0;
    
    // gui
    public static bool showGUI = false;
    private bool showGUIimage = false;
    public static bool isCompleted = false;  // static으로 변경
    private string guiMessage = "";
    public float maxDistance = 25f;  // 원형 범위의 지름
    public Vector3 origin = Vector3.zero;  // 기준점 (0,0,0 위치)
    public Sprite target;
    public float duration, isi;
    public string start_time;
    private Texture2D backgroundTex;

    // Fixation debug
    private bool prevFixationState = false;
    private float fixationStartTime = 0f;
    private bool isMonitoringFixation = false;

    // New variables for GUI
    private Texture2D blackTexture;
    private GUIStyle bgStyle;
    private GUIStyle guiStyle;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        player = GameObject.Find("FirstPerson-AIO");
    }

    void Start()
    {
        submitButton.onClick.AddListener(SaveUserInput);
        
        // Texture2D 초기화
        blackTexture = new Texture2D(1, 1);
        blackTexture.SetPixel(0, 0, new Color32(217, 217, 217, 255));
        blackTexture.Apply();
        
        // GUIStyle 초기화
        bgStyle = new GUIStyle();
        bgStyle.normal.background = blackTexture;
        
        guiStyle = new GUIStyle();
        guiStyle.fontSize = 90;
        guiStyle.fontStyle = FontStyle.Bold;
        guiStyle.normal.textColor = Color.black;
        guiStyle.alignment = TextAnchor.MiddleCenter;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void SaveUserInput()
    {
        SbjManager.showFixation = true;

        SbjManager.sbj_num = sbjNumInput.text;
        SbjManager.sbj_task = int.Parse(sbjTaskInput.text);
        SbjManager.sbj_mode = modeInput.text;
        
        // modeInput 설명 업데이트
        int modeValue;
        bool isValidMode = int.TryParse(modeInput.text, out modeValue);
        if (!isValidMode || modeValue < 1 || modeValue > 4)
        {
            SbjManager.sbj_mode = "1";
            modeInput.text = "1"; // modeInput의 텍스트도 업데이트
        }

        Debug.Log($"[SUBJECT INFO] Sbject ID: {SbjManager.sbj_num}, Task starts with: {SbjManager.sbj_task}, target mode: {SbjManager.sbj_mode}");
        InputCanvas.SetActive(false);

        Logfile();
        sceneOrderSetting();
        StartCoroutine(Taskplayer());
    }
    
    void sceneOrderSetting()
    {
        // SbjManager.sbj_fam_order = GenerateFamVector(); // 이전 랜덤 방식 제거

        // 새로운 modeInput 처리
        int modeValue;
        bool isValid = int.TryParse(modeInput.text, out modeValue);

        if (!isValid || modeValue < 1 || modeValue > 4)
        {
            modeValue = 1; // 잘못된 값이 들어오면 기본값 1로 처리
        }

        // modeInput 값에 따라 sbj_fam_order 설정
        SbjManager.sbj_fam_order = new int[2];
        
        if (modeValue == 1 || modeValue == 2)
        {
            // [0,1] 설정
            SbjManager.sbj_fam_order[0] = 0;
            SbjManager.sbj_fam_order[1] = 1;
        }
        else // modeValue == 3 || modeValue == 4
        {
            // [1,0] 설정
            SbjManager.sbj_fam_order[0] = 1;
            SbjManager.sbj_fam_order[1] = 0;
        }

        // 타겟 순서 설정을 위한 swap 값 결정
        bool swap = (modeValue == 2 || modeValue == 4);
        
        Vector3[] contextA = new Vector3[]
        {
            new Vector3(-6.18f, 0f, 5.91f),   // A1
            new Vector3(-5.73f, 0f, -5.99f)   // A2
        };

        Vector3[] contextB = new Vector3[]
        {
            new Vector3(5.98f, 0f, -6.08f),     // B1
            new Vector3(5.39f, 0f, 5.94f)     // B2
        };

        // modeInput 값에 따른 swap 설정 (앞에서 결정됨)

        Debug.Log($"Mode value: {modeValue}, swap: {swap}, sbj_fam_order: [{string.Join(", ", SbjManager.sbj_fam_order)}]");

        if (swap)
        {
            // mode 2 또는 4: 타겟 순서 바꿈
            SbjManager.context_target[SbjManager.sbj_fam_order[0], 0] = contextA[1];
            SbjManager.context_target[SbjManager.sbj_fam_order[0], 1] = contextA[0];
            SbjManager.context_target[1 - SbjManager.sbj_fam_order[0], 0] = contextB[1];
            SbjManager.context_target[1 - SbjManager.sbj_fam_order[0], 1] = contextB[0];
        }
        else
        {
            // mode 1 또는 3: 기본 타겟 순서
            SbjManager.context_target[SbjManager.sbj_fam_order[0], 0] = contextA[0];
            SbjManager.context_target[SbjManager.sbj_fam_order[0], 1] = contextA[1];
            SbjManager.context_target[1 - SbjManager.sbj_fam_order[0], 0] = contextB[0];
            SbjManager.context_target[1 - SbjManager.sbj_fam_order[0], 1] = contextB[1];
        }
    }
    

    public enum TaskPhase
    {
        PreForaging = 1,
        Learning1 = 2,
        IntermForaging = 3,
        Learning2 = 4,
        PostForaging = 5,
    }

    TaskPhase GetTaskPhase(int sbj_task)
    {
        if (sbj_task == 1)
            return TaskPhase.PreForaging;
        else if (sbj_task == 2 || sbj_task == 3 || sbj_task == 4)
            return TaskPhase.Learning1;
        else if (sbj_task == 5 || sbj_task == 6 || sbj_task == 7)
            return TaskPhase.Learning2;
            
        throw new ArgumentOutOfRangeException(nameof(sbj_task), $"Invalid task value: {sbj_task}");
    }

    IEnumerator Taskplayer()
    {
        TaskPhase phase = GetTaskPhase(SbjManager.sbj_task);

        ChunkSetting();
        SbjManager.sbj_coin_order = GenerateCoinOrder(3);
        
        if (phase == TaskPhase.PreForaging)
        {
            yield return StartCoroutine(CoinTaskplayer()); // pre
            SbjManager.curr_coin_sess++;
            yield return StartCoroutine(RemapTaskplayer()); // 1st run
            SbjManager.curr_run++;
            yield return StartCoroutine(ReverseTaskplayer()); // 2nd run
        }
        else if (phase == TaskPhase.Learning1)
        {
            SbjManager.curr_coin_sess++;
            yield return StartCoroutine(RemapTaskplayer()); // 1st run
            SbjManager.curr_run++;
            yield return StartCoroutine(ReverseTaskplayer()); // 2nd run
        }
        else if (phase == TaskPhase.Learning2)
        {
            SbjManager.curr_coin_sess++;
            SbjManager.curr_run++;
            yield return StartCoroutine(ReverseTaskplayer()); // 2nd run
        }
    }

    IEnumerator CoinTaskplayer()
    {
        yield return new WaitForSeconds(4f);
        SbjManager.showFixation = false;
        SbjManager.curr_coin_run = 0;
        
        showGUI = true;
        // guiMessage = "You are a farmer.\n\nWalk around the gardens\nand collect seed bags.";
        guiMessage = "당신은 농부입니다.\n\n여러 텃밭을 돌아다니며\n씨앗 주머니를 주워주세요.";
        yield return new WaitForSeconds(6f);
        showGUI = false;

        if (SbjManager.curr_coin_sess == 0) {
        SbjManager.coinWriter1.WriteLine($"scene order for block1: {string.Join(", ", SbjManager.sbj_coin_order[0])}");
        SbjManager.coinWriter1.WriteLine($"scene order for block2: {string.Join(", ", SbjManager.sbj_coin_order[1])}");
        SbjManager.coinWriter1.WriteLine($"scene order for block3: {string.Join(", ", SbjManager.sbj_coin_order[2])}");
        SbjManager.coinWriter1.Flush();
        }
        else if (SbjManager.curr_coin_sess == 1) {
        SbjManager.coinWriter1.WriteLine($"scene order for block1: {string.Join(", ", SbjManager.sbj_coin_order[0])}");
        SbjManager.coinWriter1.WriteLine($"scene order for block2: {string.Join(", ", SbjManager.sbj_coin_order[1])}");
        SbjManager.coinWriter1.WriteLine($"scene order for block3: {string.Join(", ", SbjManager.sbj_coin_order[2])}");
        SbjManager.coinWriter2.Flush();
        }
        else if (SbjManager.curr_coin_sess == 2) {
        SbjManager.coinWriter1.WriteLine($"scene order for block1: {string.Join(", ", SbjManager.sbj_coin_order[0])}");
        SbjManager.coinWriter1.WriteLine($"scene order for block2: {string.Join(", ", SbjManager.sbj_coin_order[1])}");
        SbjManager.coinWriter1.WriteLine($"scene order for block3: {string.Join(", ", SbjManager.sbj_coin_order[2])}");
        SbjManager.coinWriter3.Flush();
        }

        SbjManager.curr_coin_trial = 0;
        // run1
        for (int curr_idx = 0; curr_idx < SbjManager.sbj_coin_order[SbjManager.curr_coin_run].Length; curr_idx++) // 1 run ( 5 environment * 3 times)
        {
            int order = SbjManager.sbj_coin_order[SbjManager.curr_coin_run][curr_idx];
            string sceneName = $"CoinScene{order}";
            curr_env = order;
            SbjManager.curr_coin_env = order;
            Debug.Log($"=== {sceneName} ready for trial #{curr_idx + 1}");
            CoinSetting();

            string taskname = "foraging_run1";
            SbjManager.curr_task = taskname;
            SbjManager.curr_scene = sceneName;
            SbjManager.curr_coin_trial++;

            SbjManager.showFixation = true;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);                
            yield return new WaitUntil(() => isCompleted);
            asyncLoad.allowSceneActivation = false;
            isCompleted = false;
        }

        
        SbjManager.curr_coin_trial = 0;
        SbjManager.curr_coin_run++;
        Debug.Log($"==curr_coin_run: {SbjManager.curr_coin_run}");
        // run 2
        for (int curr_idx = 0; curr_idx < SbjManager.sbj_coin_order[SbjManager.curr_coin_run].Length; curr_idx++)
        {
            int order = SbjManager.sbj_coin_order[SbjManager.curr_coin_run][curr_idx];
            string sceneName = $"CoinScene{order}";
            curr_env = order;
            SbjManager.curr_coin_env = order;
            Debug.Log($"=== {sceneName} ready for trial #{curr_idx + 1}");
            CoinSetting();

            string taskname = "foraging_run2";
            SbjManager.curr_task = taskname;
            SbjManager.curr_scene = sceneName;
            SbjManager.curr_coin_trial = curr_idx + 1;

            SbjManager.showFixation = true;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);                
            yield return new WaitUntil(() => isCompleted);
            asyncLoad.allowSceneActivation = false;
            isCompleted = false;
        }


        SbjManager.curr_coin_trial = 0;
        SbjManager.curr_coin_run++;
        Debug.Log($"==curr_coin_run: {SbjManager.curr_coin_run}");
        // run 3
        for (int curr_idx = 0; curr_idx < SbjManager.sbj_coin_order[SbjManager.curr_coin_run].Length; curr_idx++) // 1 run ( 5 environment * 3 times)
        {
            int order = SbjManager.sbj_coin_order[SbjManager.curr_coin_run][curr_idx];
            string sceneName = $"CoinScene{order}";
            curr_env = order;
            SbjManager.curr_coin_env = order;
            Debug.Log($"=== {sceneName} ready for trial #{curr_idx + 1}");
            CoinSetting();

            string taskname = "foraging_run3";
            SbjManager.curr_task = taskname;
            SbjManager.curr_scene = sceneName;
            SbjManager.curr_coin_trial = curr_idx + 1;

            SbjManager.showFixation = true;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);                
            yield return new WaitUntil(() => isCompleted);
            asyncLoad.allowSceneActivation = false;
            isCompleted = false;
            
            if (curr_idx == 4)
            {
                SbjManager.showFixation = false;
                showGUI = true;
                // guiMessage = "You've collected enough seeds.";
                guiMessage = "이제 씨앗을 충분히 주웠습니다.";
                yield return new WaitForSeconds(6f);
                showGUI = false;
            }
        }
        
        if (SbjManager.curr_coin_sess != 2)
        {
            showGUI = true;
            // guiMessage = "It's break time.\nPress the space bar\nwhen you're ready to continue.";
            guiMessage = "휴식을 취한 후 스페이스 바를 누르세요.";
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            showGUI = false;

            SbjManager.showFixation = true; // task 전환 시 번쩍 제거용
        }
    }

    IEnumerator RemapTaskplayer()
    {       
        // SbjManager.sbj_fam_order = GenerateFamVector();
        SbjManager.sbj_learn_order = GenerateLearnVector();
        SbjManager.sbj_test_order = GenerateTestVector();
        SbjManager.response_order = GenerateResponseVector();
        SbjManager.writer1.WriteLine($"familiarization: [{string.Join(", ", SbjManager.sbj_fam_order)}]");
        SbjManager.writer1.WriteLine($"learning: [{string.Join(", ", SbjManager.sbj_learn_order)}]");
        SbjManager.writer1.WriteLine($"testing: [{string.Join(", ", SbjManager.sbj_test_order)}]");
        SbjManager.writer1.WriteLine($"learning_response_order: [{string.Join(", ", SbjManager.response_order)}]");
        Debug.Log($"learning_response_order: [{string.Join(", ", SbjManager.response_order)}]");
        SbjManager.writer1.Flush();

        yield return new WaitForSeconds(4f);
        SbjManager.showFixation = false; // task 전환 시 번쩍 제거용
        
        showGUI = true;
        // guiMessage = "Your task is to find mushrooms in the garden.\n\nEach garden has\nboth edible and poisonous mushrooms.";
        guiMessage = "텃밭에서 농작물을 수확해주세요.\n\n당신은 영양가가 가장 많고\n탐스러운 농작물을 수확하는 것이 목적입니다.";
        yield return new WaitForSeconds(8f);
        guiMessage = "텃밭마다 영양분이 많은 위치와\n훨씬 더 많은 위치가 있습니다.";
        yield return new WaitForSeconds(6f);
        showGUI = false;


    if (SbjManager.sbj_task <= 2)
    {
        showGUI = true;
        // guiMessage = "Remember the locations\nof the two mushrooms in each garden.\n(30 seconds)";
        guiMessage = "등장하는 텃밭에서\n영양분이 많은 위치와\n훨씬 많은 위치를 기억하세요.\n(30초)";
        yield return new WaitForSeconds(6f);
        showGUI = false;

        for (curr_idx = 0; curr_idx < SbjManager.sbj_fam_order.Length; curr_idx++)
        {
            int order = SbjManager.sbj_fam_order[curr_idx];
            string sceneName = order == 0 ? "FamScene0" : "FamScene4";
            Debug.Log($"=== {sceneName} ready");
            curr_env = order;

            SbjManager.curr_task = "fam";
            SbjManager.curr_scene = sceneName;
            SbjManager.curr_trial = curr_idx + 1;
            SbjManager.curr_order = SbjManager.sbj_fam_order[curr_idx];

            SbjManager.showFixation = true;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);                
            yield return new WaitUntil(() => isCompleted);
            asyncLoad.allowSceneActivation = false;
            isCompleted = false;

            if (curr_idx < SbjManager.sbj_fam_order.Length - 1)
            {
                SbjManager.showFixation = false;
                showGUI = true;
                // guiMessage = "Moving to the next garden.";
                guiMessage = "다음 텃밭으로 이동합니다.";
                yield return new WaitForSeconds(2f);
                showGUI = false;
            }
            else
            {
                SbjManager.showFixation = false;
            }
        }
    }

    
    if (SbjManager.sbj_task <= 3)
    {
        showGUI = true; 
        guiMessage = "이제 피드백을 통해\n영양분이 있는 위치를 학습합니다.";
        yield return new WaitForSeconds(6f);
        showGUI = false; 
        
        // Reset learning state
        SbjManager.learning_end = false;
        SbjManager.dist_error.Clear();
        int trialCount = 0;
        string sceneName = "";  // Move sceneName declaration outside the loop
        
        while (!SbjManager.learning_end && trialCount < SbjManager.max_learning_trials)
        {
            int order = SbjManager.sbj_learn_order[trialCount % SbjManager.sbj_learn_order.Length];
            sceneName = order == 0 ? "LearnScene0" : "LearnScene4";  // Assign to the outer variable
            Debug.Log($"=== {sceneName} ready for trial {trialCount + 1}");
            Debug.Log($"Current dist_error values: [{string.Join(", ", SbjManager.dist_error)}]");
            curr_env = order;

            SbjManager.curr_task = "learning";
            SbjManager.curr_scene = sceneName;
            SbjManager.curr_trial = trialCount + 1;
            SbjManager.curr_order = order;

            SbjManager.showFixation = true; 
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = true;
            yield return new WaitUntil(() => isCompleted);
            isCompleted = false;

            // Check if we have enough trials to evaluate
            if (trialCount >= SbjManager.min_learning_trials && SbjManager.dist_error.Count >= 6)
            {
                // Calculate average of last 6 trials
                float recentAvg = SbjManager.dist_error.Skip(SbjManager.dist_error.Count - 6).Average();
                Debug.Log($"Recent average error: {recentAvg}");
                Debug.Log($"Last 6 error values: [{string.Join(", ", SbjManager.dist_error.Skip(SbjManager.dist_error.Count - 6))}]");

                if (recentAvg <= SbjManager.learning_threshold)
                {
                    SbjManager.learning_end = true;
                }
            }

            trialCount++;

            if (trialCount == SbjManager.max_learning_trials)
            {
                SbjManager.learning_end = true;
            }
        }
        
        yield return new WaitForSeconds(2f);
        SbjManager.showFixation = false;
        SceneManager.UnloadSceneAsync(sceneName);  // Now sceneName is accessible here
    }

    if (SbjManager.sbj_task <= 4)
    {
        showGUI = true;
        // guiMessage = "Feedback is no longer provided.\nFind the mushrooms for the animals\nin the garden.";
        guiMessage = "이제 피드백이 제시되지 않습니다.\n\n농작물을 수확할 위치를 찾아주세요.";
        yield return new WaitForSeconds(6f);
        SbjManager.showFixation = true;
        showGUI = false;

        for (curr_idx = 0; curr_idx < SbjManager.sbj_test_order.Length; curr_idx++)
        {
            Debug.Log($"[{string.Join(", ", SbjManager.sbj_test_order)}]");
            
            int order = SbjManager.sbj_test_order[curr_idx];
            string sceneName = $"TestScene{order}";
            curr_env = order;

            SbjManager.curr_task = "testing";
            SbjManager.curr_scene = sceneName;
            SbjManager.curr_trial = curr_idx + 1;
            SbjManager.curr_order = SbjManager.sbj_test_order[curr_idx];

            SbjManager.showFixation = true;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);                
            yield return new WaitUntil(() => isCompleted);
            asyncLoad.allowSceneActivation = false;
            isCompleted = false;
            
            if (curr_idx < SbjManager.sbj_test_order.Length - 1)
            {
                SbjManager.showFixation = true;
                yield return new WaitForSeconds(4f);
                SbjManager.showFixation = false;
            }
            else
            {        
                SbjManager.showFixation = false;    
            }

            if (curr_idx == 20)
            {
                showGUI = true;
                // guiMessage = "It's break time.\nPress the space bar\nwhen you're ready to continue.";
                guiMessage = "휴식을 취한 후 스페이스 바를 누르세요.";
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                showGUI = false;
            }
        }
    }

    showGUI = true;
    // guiMessage = "It's break time.\nPress the space bar\nwhen you're ready to continue.";
    guiMessage = "휴식을 취한 후 스페이스 바를 누르세요.";
    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
    showGUI = false;

    SbjManager.showFixation = true; // task 전환 시 번쩍 제거용
}

IEnumerator ReverseTaskplayer()
{   
    // SbjManager.sbj_fam_order = GenerateFamVector();
    SbjManager.sbj_learn_order = GenerateLearnVector();
    SbjManager.sbj_test_order = GenerateTestVector();
    SbjManager.response_order = GenerateResponseVector();
    SbjManager.writer2.WriteLine($"familiarization: [{string.Join(", ", SbjManager.sbj_fam_order)}]");
    SbjManager.writer2.WriteLine($"learning: [{string.Join(", ", SbjManager.sbj_learn_order)}]");
    SbjManager.writer2.WriteLine($"testing: [{string.Join(", ", SbjManager.sbj_test_order)}]");
    SbjManager.writer2.WriteLine($"learning_response_order: [{string.Join(", ", SbjManager.response_order)}]");
        Debug.Log($"learning_response_order: [{string.Join(", ", SbjManager.response_order)}]");
    SbjManager.writer2.Flush();

    SbjManager.showFixation = false; // task 전환 시 번쩍 제거용
    
    showGUI = true;
    // guiMessage = "There's been a shift in the garden.";
    guiMessage = "텃밭에 지각변동이 일어났습니다!";
    yield return new WaitForSeconds(4f);
    showGUI = false;

    showGUI = true;
    // guiMessage = "The locations of the edible\nand poisonous mushrooms have changed.";
    guiMessage = "텃밭의 영양분의 위치가 뒤바뀌었습니다.";
    yield return new WaitForSeconds(6f);
    showGUI = false;

    if (SbjManager.sbj_task <= 5)
    {
        showGUI = true;
        // guiMessage = "Remember the locations\nof the two mushrooms in each garden.\n(30 seconds)";
        guiMessage = "등장하는 텃밭에서\n영양분이 많은 위치와\n훨씬 많은 위치를 기억하세요.\n(30초)";
        yield return new WaitForSeconds(6f);
        showGUI = false;

        for (curr_idx = 0; curr_idx < SbjManager.sbj_fam_order.Length; curr_idx++)
        {
            Debug.Log($"[{string.Join(", ", SbjManager.sbj_fam_order)}]");
            int order = SbjManager.sbj_fam_order[curr_idx];
            string sceneName = order == 0 ? "FamScene0" : "FamScene4";
            Debug.Log($"=== {sceneName} ready");
            curr_env = order;

            SbjManager.curr_task = "rfam";
            SbjManager.curr_scene = sceneName;
            SbjManager.curr_trial = curr_idx + 1;
            SbjManager.curr_order = SbjManager.sbj_fam_order[curr_idx];

            SbjManager.showFixation = true;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);                
            yield return new WaitUntil(() => isCompleted);
            asyncLoad.allowSceneActivation = false;
            isCompleted = false;
            
            if (curr_idx < SbjManager.sbj_fam_order.Length - 1)
            {
                SbjManager.showFixation = false;
                showGUI = true;
                // guiMessage = "Moving to the next garden.";
                guiMessage = "다음 텃밭으로 이동합니다.";
                yield return new WaitForSeconds(4f);
                showGUI = false;
            }
            else
            {        
                SbjManager.showFixation = false;    
            }
        }
    }

    if (SbjManager.sbj_task <= 6)
    {
        showGUI = true;
        guiMessage = "이제 피드백을 통해\n영양분이 있는 위치를 학습합니다.";
        yield return new WaitForSeconds(6f);
        showGUI = false; 
        
        // Reset learning state
        SbjManager.learning_end = false;
        SbjManager.dist_error.Clear();
        int trialCount = 0;
        string sceneName = "";  // Move sceneName declaration outside the loop
        
        while (!SbjManager.learning_end && trialCount < SbjManager.max_learning_trials)
        {
            int order = SbjManager.sbj_learn_order[trialCount % SbjManager.sbj_learn_order.Length];
            sceneName = order == 0 ? "LearnScene0" : "LearnScene4";  // Assign to the outer variable
            Debug.Log($"=== {sceneName} ready for trial {trialCount + 1}");
            Debug.Log($"Current dist_error values: [{string.Join(", ", SbjManager.dist_error)}]");
            curr_env = order;

            SbjManager.curr_task = "rlearning";
            SbjManager.curr_scene = sceneName;
            SbjManager.curr_trial = trialCount + 1;
            SbjManager.curr_order = order;

            SbjManager.showFixation = true;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = true;
            yield return new WaitUntil(() => isCompleted);
            isCompleted = false;

            // Check if we have enough trials to evaluate
            if (trialCount >= SbjManager.min_learning_trials && SbjManager.dist_error.Count >= 6)
            {
                // Calculate average of last 6 trials
                float recentAvg = SbjManager.dist_error.Skip(SbjManager.dist_error.Count - 6).Average();
                Debug.Log($"Recent average error: {recentAvg}");
                Debug.Log($"Last 6 error values: [{string.Join(", ", SbjManager.dist_error.Skip(SbjManager.dist_error.Count - 6))}]");

                if (recentAvg <= SbjManager.learning_threshold)
                {
                    SbjManager.learning_end = true;
                }
            }

            trialCount++;

            if (trialCount == SbjManager.max_learning_trials)
            {
                SbjManager.learning_end = true;
            }
        }
        
        yield return new WaitForSeconds(2f);
        SbjManager.showFixation = false;
        SceneManager.UnloadSceneAsync(sceneName);  // Now sceneName is accessible here
    }

    if (SbjManager.sbj_task <= 7)
    {            
        showGUI = true;
        // guiMessage = "Feedback is no longer provided.\nFind the mushrooms\nfor the animals in the garden.";
        guiMessage = "이제 피드백이 제시되지 않습니다.\n\n농작물을 수확할 위치를 찾아주세요.";
        yield return new WaitForSeconds(6f);
        SbjManager.showFixation = true;
        showGUI = false;

        for (curr_idx = 0; curr_idx < SbjManager.sbj_test_order.Length; curr_idx++)
        {
            Debug.Log($"[{string.Join(", ", SbjManager.sbj_test_order)}]");
            
            int order = SbjManager.sbj_test_order[curr_idx];
            string sceneName = $"TestScene{order}";
            Debug.Log($"Testing {sceneName} ready");
            curr_env = order;

            SbjManager.curr_task = "rtesting";
            SbjManager.curr_scene = sceneName;
            SbjManager.curr_trial = curr_idx + 1;
            SbjManager.curr_order = SbjManager.sbj_test_order[curr_idx];

            SbjManager.showFixation = true;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);                
            yield return new WaitUntil(() => isCompleted);
            asyncLoad.allowSceneActivation = false;
            isCompleted = false;

            if (curr_idx < SbjManager.sbj_test_order.Length - 1)
            {
                SbjManager.showFixation = true;
                yield return new WaitForSeconds(2f);
                SbjManager.showFixation = false;
            }
            else
            {        
                SbjManager.showFixation = false;    
            }
            if (curr_idx == 20)
            {
                showGUI = true;
                // guiMessage = "It's break time.\nPress the space bar\nwhen you're ready to continue.";
                guiMessage = "휴식을 취한 후 스페이스 바를 누르세요.";
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                showGUI = false;
            }
        }
    }
    showGUI = true;
    // guiMessage = "The experiment is now complete.\nPlease exit and follow the experimenter's instructions.";
    guiMessage = "실험이 모두 종료되었습니다.\n수고하셨습니다.\n\n나가서 실험자의 지시에 따라주세요.";
    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Escape));
    showGUI = false;
}

// =================================================================================
  
void ChunkSetting()
{
    // 1. chunk order sequence 생성
    chunkOrder = new List<int[][]>();
    chunkOrder.Add(new int[][] {
        new int[] {5, 7, 3, 9},
        new int[] {1, 6, 4, 8},
        new int[] {8, 2, 9, 4}
    });

    chunkOrder.Add(new int[][] {
        new int[] {4, 8, 2, 6},
        new int[] {8, 3, 4, 9},
        new int[] {1, 5, 7, 6} 
    });

    chunkOrder.Add(new int[][] {
        new int[] {9, 5, 4, 3},
        new int[] {4, 8, 3, 1},
        new int[] {2, 6, 7, 5}
    });

    chunkOrder.Add(new int[][] {
        new int[] {4, 9, 3, 1},
        new int[] {8, 1, 5, 3},
        new int[] {2, 7, 6, 4} 
    });

    chunkOrder.Add(new int[][] {
        new int[] {2, 9, 7, 3},
        new int[] {9, 3, 1, 5},
        new int[] {6, 4, 2, 8}
    });

    chunkOrder.Add(new int[][] {
        new int[] {3, 4, 6, 7},
        new int[] {7, 2, 1, 9},
        new int[] {5, 3, 8, 4}
    });

    chunkOrder.Add(new int[][] {
        new int[] {3, 9, 2, 7},
        new int[] {4, 3, 6, 8},
        new int[] {5, 7, 8, 1} 
    });
    chunkOrder.Add(new int[][] {
        new int[] {3, 8, 1, 7},
        new int[] {6, 4, 9, 2},
        new int[] {7, 3, 5, 2} 
    });
    chunkOrder.Add(new int[][] {
        new int[] {9, 3, 7, 5},
        new int[] {6, 1, 3, 7},
        new int[] {1, 6, 4, 2} 
    });
    chunkOrder.Add(new int[][] {
        new int[] {7, 1, 5, 3},
        new int[] {3, 5, 9, 7},
        new int[] {9, 2, 8, 4}
    });
    
    // 2. set combination을 pre/interm/post에  대해 선택 
    int[,] setComb = new int[5, 5]
    {
        { 0, 2, 4, 5, 6 },
        { 0, 1, 3, 6, 9 },
        { 0, 3, 4, 5, 8 },
        { 1, 3, 4, 7, 9 },
        { 2, 3, 5, 7, 8 }
    };

    List<int[][]> seqtoenv = new List<int[][]>(); // environment allocation to sequence
    seqtoenv.Add(new int[][] { 
        new int[] { 1, 0, 3, 2, 4 }, // pre foraging
        new int[] { 3, 4, 0, 1, 2 }, // interm foraging
        new int[] { 0, 2, 1, 4, 3 } // post foraging
    });
    seqtoenv.Add(new int[][] { 
        new int[] { 2, 1, 0, 4, 3 },
        new int[] { 0, 3, 1, 2, 4 },
        new int[] { 3, 4, 2, 1, 0 }
    });
    seqtoenv.Add(new int[][] { 
        new int[] { 1, 4, 2, 3, 0 },
        new int[] { 0, 2, 3, 4, 1 },
        new int[] { 2, 3, 1, 0, 4 }
    });
    seqtoenv.Add(new int[][] { 
        new int[] { 0, 1, 2, 3, 4 },
        new int[] { 2, 4, 0, 1, 3 },
        new int[] { 4, 2, 3, 0, 1 }
    });

    List<int> allSetCombIndices = Enumerable.Range(0, 5).ToList();
    List<int> selectedSetComb = allSetCombIndices.OrderBy(_ => Random.value).Take(3).ToList();
    Debug.Log($"선택된 setComb 인덱스: [{string.Join(", ", selectedSetComb)}]");
    SbjManager.coinWriter1.WriteLine($"Sequence combination[{string.Join(", ", selectedSetComb)}]");
    SbjManager.coinWriter2.WriteLine($"Sequence combination[{string.Join(", ", selectedSetComb)}]");
    SbjManager.coinWriter3.WriteLine($"Sequence combination[{string.Join(", ", selectedSetComb)}]");
    SbjManager.coinWriter1.Flush();
    SbjManager.coinWriter2.Flush();
    SbjManager.coinWriter3.Flush();

    int[][,] pre_chunkorder = new int[5][,];
    int[][,] interm_chunkorder = new int[5][,];
    int[][,] post_chunkorder = new int[5][,];

    void FillChunkOrder(int setCombIdx, ref int[][,] targetChunkOrder, string name)
    {
        for (int i = 0; i < 5; i++)
        {
            int chunkIdx = setComb[setCombIdx, i];
            if (chunkIdx >= 0 && chunkIdx < chunkOrder.Count)
            {
                int[][] source = chunkOrder[chunkIdx];
                int[,] targetArray = new int[source.Length, source[0].Length];
                
                for (int j = 0; j < source.Length; j++)
                {
                    for (int k = 0; k < source[j].Length; k++)
                    {
                        targetArray[j, k] = source[j][k];
                    }
                }
                
                targetChunkOrder[i] = targetArray;
            }
        }
    }

    FillChunkOrder(selectedSetComb[0], ref pre_chunkorder, "pre_chunkorder");
    FillChunkOrder(selectedSetComb[1], ref interm_chunkorder, "interm_chunkorder");
    FillChunkOrder(selectedSetComb[2], ref post_chunkorder, "post_chunkorder");

    // 3. 선택한 set들을 어떤 환경에 할당할지 선택
    int seqtoenv_idx = Random.Range(0, seqtoenv.Count);

    if (seqtoenv_idx < seqtoenv.Count && seqtoenv[seqtoenv_idx].Length >= 3)
    {
        for (int i = 0; i < 5; i++)
        {
            int sourceIdx = seqtoenv[seqtoenv_idx][0][i]; 
            if (sourceIdx >= 0 && sourceIdx < 5 && pre_chunkorder[sourceIdx] != null)
            {
                SbjManager.pre_coinorder[i] = pre_chunkorder[sourceIdx];
            }
        }

        for (int i = 0; i < 5; i++)
        {
            int sourceIdx = seqtoenv[seqtoenv_idx][1][i];
            if (sourceIdx >= 0 && sourceIdx < 5 && interm_chunkorder[sourceIdx] != null)
            {
                SbjManager.interm_coinorder[i] = interm_chunkorder[sourceIdx];
            }
        }

        for (int i = 0; i < 5; i++)
        {
            int sourceIdx = seqtoenv[seqtoenv_idx][2][i]; 
            if (sourceIdx >= 0 && sourceIdx < 5 && post_chunkorder[sourceIdx] != null)
            {
                SbjManager.post_coinorder[i] = post_chunkorder[sourceIdx];
            }
        }
    }

    // 4. 선택한 setcombination에 따라 player starting location 설정하기
    List<Vector3[]> playerstart = new List<Vector3[]>();
    for (int setIdx = 1; setIdx < 11; setIdx++) 
    {
        List<Vector3> positions = new List<Vector3>();

        for (int sub = 0; sub < 3; sub++)
        {
            string objName = sub == 0 ? $"set{setIdx}" : $"set{setIdx} ({sub})";
            GameObject obj = GameObject.Find(objName);

            if (obj != null)
            {
                Vector3 pos = obj.transform.position;
                pos.y = 0.91f; // #coin player height
                positions.Add(pos);
            }
        }

        if (positions.Count > 0)
        {
            playerstart.Add(positions.ToArray());
        }
    }

    List<Vector3[]> pre_playerpositions = new List<Vector3[]>(); // index: pre_coinorder[env_idx][run_idx, trialidx]
    List<Vector3[]> interm_playerpositions = new List<Vector3[]>();
    List<Vector3[]> post_playerpositions = new List<Vector3[]>();

    // 5. 순서를 재배열함
    void FillPlayerStart(int setCombIdx, ref List<Vector3[]> target, string name)
    {
        for (int i = 0; i < 5; i++)
        {
            int chunkIdx = setComb[setCombIdx, i];
            if (chunkIdx >= 0 && chunkIdx < playerstart.Count)
            {
                target.Add(playerstart[chunkIdx]);
            }
        }
    }

    FillPlayerStart(selectedSetComb[0], ref pre_playerpositions, "pre_playerpositions"); // index: pre_playerstart[env_idx][run_idx] 
    FillPlayerStart(selectedSetComb[1], ref interm_playerpositions, "interm_playerpositions");
    FillPlayerStart(selectedSetComb[2], ref post_playerpositions, "post_playerpositions");

    if (seqtoenv_idx < seqtoenv.Count && seqtoenv[seqtoenv_idx].Length >= 3)
    {
        // pre_playerpositions 재배열
        for (int i = 0; i < 5; i++)
        {
            int sourceIdx = seqtoenv[seqtoenv_idx][0][i]; // 첫 번째 행 사용
            if (sourceIdx >= 0 && sourceIdx < pre_playerpositions.Count && pre_playerpositions[sourceIdx] != null)
            {
                SbjManager.pre_playerstart.Add(pre_playerpositions[sourceIdx]);
            }
        }

        // interm_playerpositions 재배열
        for (int i = 0; i < 5; i++)
        {
            int sourceIdx = seqtoenv[seqtoenv_idx][1][i]; // 두 번째 행 사용
            if (sourceIdx >= 0 && sourceIdx < interm_playerpositions.Count && interm_playerpositions[sourceIdx] != null)
            {
                SbjManager.interm_playerstart.Add(interm_playerpositions[sourceIdx]);
            }
        }

        // post_playerpositions 재배열
        for (int i = 0; i < 5; i++)
        {
            int sourceIdx = seqtoenv[seqtoenv_idx][2][i]; // 세 번째 행 사용
            if (sourceIdx >= 0 && sourceIdx < post_playerpositions.Count && post_playerpositions[sourceIdx] != null)
            {
                SbjManager.post_playerstart.Add(post_playerpositions[sourceIdx]);
            }
        }
    }
}

void CoinSetting()
{       
    // 1. 청크 안에서 위치값 정하기
    List<Vector3> chunkCenter = new List<Vector3>();
    float chunkDia = 0f;
    List<Vector3> chunkLoc = new List<Vector3>();

    for (int i = 1; i <= SbjManager.numChunk; i++)
    {
        GameObject chunkObj = GameObject.Find($"chunk{i}"); // 각 청크의 중심좌표
        chunkCenter.Add(chunkObj.transform.position);
    }
    chunkDia = chunkCenter.Count > 0 ? GameObject.Find("chunk1").transform.localScale.x : 0f; // 지름크기
  
    float radius = chunkDia / 6f;
    float y = SbjManager.coinheight;
    foreach (var center in chunkCenter)
    {
        float angle = Random.Range(0f, 360f);
        float dist = Random.Range(0f, radius);
        float x = center.x + Mathf.Cos(angle * Mathf.Deg2Rad) * dist;
        float z = center.z + Mathf.Sin(angle * Mathf.Deg2Rad) * dist;
        Vector3 newPos = new Vector3(x, y, z);
        
        chunkLoc.Add(newPos);
    }
    if (SbjManager.curr_coin_sess == 0)
    {
        SbjManager.curr_player_start = SbjManager.pre_playerstart[SbjManager.curr_coin_env][SbjManager.curr_coin_run];
        
        // 1차원 배열로 추출
        int[] coin_array = new int[SbjManager.pre_coinorder[SbjManager.curr_coin_env].GetLength(1)];
        for (int i = 0; i < SbjManager.pre_coinorder[SbjManager.curr_coin_env].GetLength(1); i++) {
            coin_array[i] = SbjManager.pre_coinorder[SbjManager.curr_coin_env][SbjManager.curr_coin_run, i];
        }
        SbjManager.curr_coin_seq = coin_array;
    }
    else if (SbjManager.curr_coin_sess == 1)
    {
        SbjManager.curr_player_start = SbjManager.interm_playerstart[SbjManager.curr_coin_env][SbjManager.curr_coin_run];
        
        // 1차원 배열로 추출
        int[] coin_array = new int[SbjManager.interm_coinorder[SbjManager.curr_coin_env].GetLength(1)];
        for (int i = 0; i < SbjManager.interm_coinorder[SbjManager.curr_coin_env].GetLength(1); i++) {
            coin_array[i] = SbjManager.interm_coinorder[SbjManager.curr_coin_env][SbjManager.curr_coin_run, i];
        }
        SbjManager.curr_coin_seq = coin_array;
    }
    else if (SbjManager.curr_coin_sess == 2)
    {
        SbjManager.curr_player_start = SbjManager.post_playerstart[SbjManager.curr_coin_env][SbjManager.curr_coin_run];
        
        // 1차원 배열로 추출
        int[] coin_array = new int[SbjManager.post_coinorder[SbjManager.curr_coin_env].GetLength(1)];
        for (int i = 0; i < SbjManager.post_coinorder[SbjManager.curr_coin_env].GetLength(1); i++) {
            coin_array[i] = SbjManager.post_coinorder[SbjManager.curr_coin_env][SbjManager.curr_coin_run, i];
        }
        SbjManager.curr_coin_seq = coin_array;
    }
    // 2. 코인 위치값 할당하기
    for (int trialidx = 0; trialidx < SbjManager.numTrial; trialidx++)
    {
        int chunkIndex = SbjManager.curr_coin_seq[trialidx] - 1;
        SbjManager.coinLoc[trialidx] = chunkLoc[chunkIndex];
    }
}

GameObject[] FindObjectsByNamePrefix(string prefix)
{
    GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
    List<GameObject> matchedObjects = new List<GameObject>();

    foreach (GameObject obj in allObjects)
    {
        if (obj.name.StartsWith(prefix))
        {
            matchedObjects.Add(obj);
        }
    }

    return matchedObjects.ToArray();
}

void Logfile()
{
    string ori_dir = "./output/";
    string tmpDate = System.DateTime.Now.ToString("yyyyMMdd");
    string filenamePrefix = $"{tmpDate}_sbj{SbjManager.sbj_num}";
    SbjManager.FileLogger(ori_dir, filenamePrefix);

    SbjManager.writer1.WriteLine("===Remapping task (initial)");
    SbjManager.writer1.WriteLine($"Subject ID: {SbjManager.sbj_num}");
    SbjManager.writer1.WriteLine($"Target mode:  + {SbjManager.sbj_mode}");
    SbjManager.writer1.WriteLine("Date: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    SbjManager.writer1.WriteLine($"cols start_time start_loc start_rot RT curr_task curr_scene curr_run&curr_trial Response_loc answer_good answer_bad");
    SbjManager.writer1.Flush();

    SbjManager.writer2.WriteLine("===Remapping task (reversal)");
    SbjManager.writer2.WriteLine($"Subject ID: {SbjManager.sbj_num}");
    SbjManager.writer2.WriteLine($"Target mode:  + {SbjManager.sbj_mode}");
    SbjManager.writer2.WriteLine("Date: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    SbjManager.writer2.WriteLine($"cols start_time start_loc start_rot RT curr_task curr_scene curr_run&curr_trial Response_loc(notreversed) answer_good answer_bad(notreversed)");
    SbjManager.writer2.Flush();

    SbjManager.coinWriter1.WriteLine("===Foraging task (pre)");
    SbjManager.coinWriter1.WriteLine($"Subject ID: {SbjManager.sbj_num}");
    SbjManager.coinWriter1.WriteLine("Date: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    SbjManager.coinWriter1.WriteLine($"start_time start_loc start_rot RT curr_task curr_scene sess#_run#_seed# coin_loc chunk_seq");
    SbjManager.coinWriter1.Flush();   
    SbjManager.coinWriter2.WriteLine("===Foraging task (intermediate)");
    SbjManager.coinWriter2.WriteLine($"Subject ID: {SbjManager.sbj_num}");
    SbjManager.coinWriter2.WriteLine("Date: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    SbjManager.coinWriter2.WriteLine($"start_time start_loc start_rot RT curr_task curr_scene sess#_run#_seed# coin_loc chunk_seq");
    SbjManager.coinWriter2.Flush();   
    SbjManager.coinWriter3.WriteLine("===Foraging task  (post)");
    SbjManager.coinWriter3.WriteLine($"Subject ID: {SbjManager.sbj_num}");
    SbjManager.coinWriter3.WriteLine("Date: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    SbjManager.coinWriter3.WriteLine($"start_time start_loc start_rot RT curr_task curr_scene sess#_run#_seed# coin_loc chunk_seq");
    SbjManager.coinWriter3.Flush();   

    SbjManager.timeWriter.WriteLine("===fMRI Time Log");
    SbjManager.timeWriter.WriteLine($"Subject ID: {SbjManager.sbj_num}");
    SbjManager.timeWriter.WriteLine($"Target mode:  + {SbjManager.sbj_mode}");
    SbjManager.timeWriter.WriteLine("Date: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    SbjManager.timeWriter.Flush();
}

int[] GenerateResponseVector()
{
    int[] numbers = { 0, 1 };
    return GenerateRandomSeq(numbers, SbjManager.max_learning_trials, numbers.Length);
}

// condition order(shape, distal, time)
int[] GenerateCondVector()
{
    int[][] predefinedOrders =
    {
        new int[] { 1, 2, 3 },
        new int[] { 2, 3, 1 },
        new int[] { 3, 1, 2 }
    };

    // 0, 1, 2 중 랜덤하게 하나 선택
    int index = UnityEngine.Random.Range(0, predefinedOrders.Length);
    return predefinedOrders[index];
}


// scene order generator(randomize within block)
// Familiarization Phase (2 trials)
int[] GenerateFamVector()
{
    int[] numbers = { 0,1 };
    return GenerateRandomSeq(numbers, SbjManager.sbj_fam_order.Length, numbers.Length);
}

// Learning Phase (6 trials)
int[] GenerateLearnVector()
{
    int[] numbers = { 0,1 };
    return GenerateRandomSeq(numbers, SbjManager.sbj_learn_order.Length, numbers.Length);
}

// Testing Phase (60 trials)
int[] GenerateTestVector()
{
    int[] numbers = { 0, 1, 2, 3, 4};
    return GenerateRandomSeq(numbers, SbjManager.sbj_test_order.Length, numbers.Length);
}
List<int[]> GenerateCoinOrder(int setCount)
{
    List<int[]> coinOrder = new List<int[]>();

    for (int i = 0; i < setCount; i++)
    {
        int[] numbers = { 0, 1, 2, 3, 4 };
        int[] shuffled = numbers.OrderBy(_ => UnityEngine.Random.value).ToArray();
        coinOrder.Add(shuffled);
    }

    return coinOrder;
}
int[] GenerateRandomSeq(int[] numbers, int length, int blockSize)
{
    List<int> finalList = new List<int>();

    int fullBlocks = length / blockSize; 
    int remainder = length % blockSize; 

    for (int i = 0; i < fullBlocks; i++)
    {
        int[] shuffledBlock = numbers.OrderBy(_ => UnityEngine.Random.value).ToArray();
        finalList.AddRange(shuffledBlock);
    }

    if (remainder > 0)
    {
        int[] shuffledBlock = numbers.OrderBy(_ => UnityEngine.Random.value).ToArray();
        finalList.AddRange(shuffledBlock.Take(remainder));
    }

    return finalList.ToArray();
}
public void OnSceneCompleted()
{
    isCompleted = true;
}
void OnGUI()
{
    if (showGUI)
    {
        // 이미 초기화된 멤버 변수 사용
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, bgStyle);
        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 200, 100), guiMessage, guiStyle);
    }

    if (SbjManager.showFixation)
    {
        // 이미 초기화된 멤버 변수 사용
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, bgStyle);

        GUIStyle textStyle = new GUIStyle();
        textStyle.fontSize = 100;
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.normal.textColor = Color.black;

        Rect rect = new Rect(Screen.width / 2 - 25, Screen.height / 2 - 25, 50, 50);
        GUI.Label(rect, "+", textStyle);
    }
}
    void OnDestroy()
    {
        Destroy(blackTexture);
        SbjManager.CloseLogger();
    }
}
