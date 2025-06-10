using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SbjManager : MonoBehaviour
{
    // ===== current status =====
    public static string curr_task = ""; // "fam" "learning" "testing" "coin"
    public static string curr_phase = "";
    public static string curr_env = "";
    public static int curr_cond = 0;
    public static int curr_trial = 0;
    public static int curr_run = 0;
    public static int curr_order = 0; // fam, learn, test order 중에서 현재 씬의 인덱스!
    public static string curr_scene = ""; // or int curr_scene = 0;
    public static int[] env_rep = new int[5];
    public static int curr_coin_sess = 0;
    public static int curr_coin_run = 0;
    public static int curr_coin_trial= 0;
    public static int curr_coin_env = 0;
    public static int[] curr_coin_seq = new int[4]; // current sequence set (환경별 4개 코인 인덱스)
    public static Vector3 curr_player_start;

    public static int flower_idx = 0;
    public static Sprite[,] flowerSeq = new Sprite[3, 24]; // # of trials
    public static string[,] remapflowers = new string[5, 4];
    // public static int numFlower = 20;
    public static bool showFixation = false;

    // ===== player input settings =====
    public static KeyCode leftKey = KeyCode.Alpha1;
    public static KeyCode rightKey = KeyCode.Alpha3;
    public static KeyCode forwardKey = KeyCode.Alpha2;
    public static KeyCode selectKey = KeyCode.Alpha4;
    public static KeyCode exitKey = KeyCode.Escape;
    public static bool key_input = false;
    // === fmri taskplayer === 
    public static bool task_start = false;
    public static bool task_end = false;
    public static bool space_in = false;
    // ===== sbj info =====
    public static string sbj_num;
    public static int sbj_cond = 0;
    public static string sbj_date;
    public static string sbj_mode;
    public static int sbj_task = 1;
    public static int[] sbj_cond_order = new int[3];
    public static int[] sbj_fam_order = new int[2]; // # of fam, learning, testing trial
    public static int[] sbj_learn_order = new int[8]; // learning: 8
    public static int[] sbj_test_order = new int[40]; // testing: 20
    public static List<int[]> sbj_coin_order; // # of environments
    public static List<int[][]> sbj_seq_order;
    public static int[] sbj_chunk_order = new int[5]; // sequence order
    public static Vector3[,] context_target = new Vector3[2, 2]; // target location

    // ===== coin foraging task =====
    public static int numSess = 3; // pre - intermediate - post session
    public static int numTrial = 4; // 수정: 4
    public static int numEnv = 5; 
    public static int numChunk = 9;
    public static float coinheight = 0.09f;
    public static Vector3[] coinLoc = new Vector3[8];
    public static List<int[]> selectedChunk;
    public static int[][,] pre_coinorder = new int[5][,];  // pre_coinorder[env_idx][curr_coin_run]
    public static int[][,] interm_coinorder = new int[5][,];
    public static int[][,] post_coinorder = new int[5][,];
    public static List<Vector3[]> pre_playerstart = new List<Vector3[]>(); // pre_playerstart[env_idx][curr_coin_run]
    public static List<Vector3[]> interm_playerstart = new List<Vector3[]>();
    public static List<Vector3[]> post_playerstart = new List<Vector3[]>();

    // ===== location & environment =====
    public static Vector3 start_loc;
    public static Vector3 start_rot;
    public static string start_time;

    // ===== file logging =====

    public static string bhv1File;
    public static string bhv2File;
    public static string timeFile;
    public static string coin1File;
    public static string coin2File;
    public static string coin3File;
    public static StreamWriter writer1;
    public static StreamWriter writer2;
    public static StreamWriter timeWriter;
    public static StreamWriter coinWriter1;
    public static StreamWriter coinWriter2;
    public static StreamWriter coinWriter3;

    public static bool learning_end = false;
    public static List<float> dist_error = new List<float>();
    public static float learning_threshold = 5f; // 7vm threshold
    public static int max_learning_trials = 12; // Maximum number of learning trials to prevent infinite loops
    public static int min_learning_trials = 8; // Minimum number of learning trials before checking threshold
    public static int[] response_order = new int[15]; // learning: 8
    public static float target_idx = 0f;

    public static void FileLogger(string output_dir, string filenamePrefix)
    {
        // 모든 로그 파일은 하나의 디렉토리에
        bhv1File = Path.Combine(output_dir, $"{filenamePrefix}_remap1.txt");
        bhv2File = Path.Combine(output_dir, $"{filenamePrefix}_remap2.txt");
        timeFile = Path.Combine(output_dir, $"{filenamePrefix}_time.txt");
        coin1File = Path.Combine(output_dir, $"{filenamePrefix}_coin1.txt");
        coin2File = Path.Combine(output_dir, $"{filenamePrefix}_coin2.txt");
        coin3File = Path.Combine(output_dir, $"{filenamePrefix}_coin3.txt");

        if (writer1 == null) writer1 = new StreamWriter(bhv1File, true);
        if (writer2 == null) writer2 = new StreamWriter(bhv2File, true);
        if (timeWriter == null) timeWriter = new StreamWriter(timeFile, true);
        if (coinWriter1 == null) coinWriter1 = new StreamWriter(coin1File, true);
        if (coinWriter2 == null) coinWriter2 = new StreamWriter(coin2File, true);
        if (coinWriter3 == null) coinWriter3 = new StreamWriter(coin3File, true);
    }


    public static void CloseLogger()
    {
        if (writer1 != null)
        {
            writer1.Close();
            writer1 = null;
        }
        if (writer2 != null)
        {
            writer2.Close();
            writer2 = null;
        }

        if (timeWriter != null)
        {
            timeWriter.Close();
            timeWriter = null;
        }

        if (coinWriter1 != null)
        {
            coinWriter1.Close();
            coinWriter1 = null;
        }
        if (coinWriter2 != null)
        {
            coinWriter2.Close();
            coinWriter2 = null;
        }
        if (coinWriter3 != null)
        {
            coinWriter3.Close();
            coinWriter3 = null;
        }
    }



    // ===== Unity Awake =====
    void Awake()
    {
        string ori_dir = "./output/";
        string tmpDate = System.DateTime.Now.ToString("yyyyMMdd");
        string filenamePrefix = $"{tmpDate}_sbj{sbj_num}";

        // 디렉토리는 하나만
        if (!Directory.Exists(ori_dir)) Directory.CreateDirectory(ori_dir);

        // 이제 디렉토리 대신 파일명만 넘김
        SbjManager.FileLogger(ori_dir, filenamePrefix);
    }

}