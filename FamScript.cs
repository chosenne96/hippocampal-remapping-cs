// using UnityEngine;
// using UnityEngine.UI;
// using System.IO;
// using System.Linq;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine.SceneManagement;

// public class FamScript : MonoBehaviour
// {
//     // gameobject
//     FirstPersonAIO player;
//     private GameObject playerObject;
//     GameObject target1, target2;
//     GameObject goodmush, badmush;
//     GameObject text;
//     GameObject greenMarker, blueMarker, secondcam;
//     GameObject notifyBgnd, notifyText;
//     GameObject marker;
//     private Vector3 lastPlayerPosition;
//     private Quaternion lastPlayerRotation;
//     private Vector3 mirrTarget1, mirrTarget2;


//     private string ori_dir, bhv1_dir, bhv2_dir, time_dir, coin1_dir, coin2_dir, coin3_dir, tmpDate;
//     private bool isSceneChanging = false;
//     private bool writingtime = false;
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
//         secondcam = GameObject.Find("spectatorCamera");
//         greenMarker = GameObject.Find("GreenMarker");
//         blueMarker = GameObject.Find("BlueMarker");
//         notifyBgnd = GameObject.Find("NotifyBgnd");
//         notifyText = GameObject.Find("NotifyText");
//         marker = GameObject.Find("Marker");
//         target1 = GameObject.Find("target1");
//         target2 = GameObject.Find("target2");
//         goodmush = GameObject.Find("goodmush");
//         badmush = GameObject.Find("badmush");

//         secondcam.SetActive(false);
//         marker.SetActive(false);
//         greenMarker.SetActive(false);
//         blueMarker.SetActive(false);
//         notifyBgnd.SetActive(false);
//         notifyText.SetActive(false);
//         target1.SetActive(false);
//         target2.SetActive(false);
        
//         player.GetComponent<FirstPersonAIO>().playerCanMove = true;
//         player.GetComponent<FirstPersonAIO>().useHeadbob = false;
//         player.GetComponent<FirstPersonAIO>().canJump = false;
//         player.GetComponent<FirstPersonAIO>().useStamina = false;
//         player.GetComponent<AudioSource>().enabled = false;
//         player.GetComponent<FirstPersonAIO>().enableCameraShake = false;
//         player.GetComponent<FirstPersonAIO>().cameraSmoothing = 10f;
        
        
//         StartCoroutine(SceneSetting());
//     }

//     void Update()
//     {
//         // Make mushrooms look at player but keep y rotation at 0
//         Vector3 directionToPlayer = player.transform.position - goodmush.transform.position;
//         directionToPlayer.y = 0; // Keep y rotation at 0
//         goodmush.transform.rotation = Quaternion.LookRotation(directionToPlayer);

//         directionToPlayer = player.transform.position - badmush.transform.position;
//         directionToPlayer.y = 0; // Keep y rotation at 0
//         badmush.transform.rotation = Quaternion.LookRotation(directionToPlayer);
//     }

//     IEnumerator SceneSetting()
//     {       
//         Camera playerCamera = player.GetComponentInChildren<Camera>();

//         // AIO starting position
//         player.transform.position = new Vector3(0f, 0.91f, -10f);
//         player.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
//         SbjManager.start_loc = player.transform.position;

//         Vector3 directionToCenter = (new Vector3(0f, 0f, 0f) - player.transform.position).normalized;
//         Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToCenter.x, 0f, directionToCenter.z)); // 수평 회전만 적용
//         player.transform.rotation = lookRotation;
        
//         yield return new WaitForSeconds(6f);
//         SbjManager.showFixation = false; RemapManager.fmriWriter.WriteLine($"fix_start" + "\t" + RemapManager.fmriTimer.ToString());
//         RemapManager.fmriWriter.WriteLine($"scene_started" + "\t" + RemapManager.fmriTimer.ToString());
//         RemapManager.fmriWriter.Flush();
//         writingtime = true;
//         StartCoroutine(LogPlayerPosition());
//         SbjManager.start_time = System.DateTime.Now.ToString("HH:mm:ss.fff");
//         SbjManager.start_rot = player.transform.rotation.eulerAngles;

//         // target location
//         if (SbjManager.curr_run == 0) // 1st fam run
//         {
//             goodmush.transform.position = SbjManager.context_target[SbjManager.sbj_fam_order[SbjManager.curr_order], 0];
//             badmush.transform.position = SbjManager.context_target[SbjManager.sbj_fam_order[SbjManager.curr_order], 1];
//             Debug.Log($"{goodmush.transform.position}");
//             Debug.Log($"{badmush.transform.position}");
//         }
//         else if (SbjManager.curr_run == 1) // 2nd fam run
//         {
//             goodmush.transform.position = SbjManager.context_target[SbjManager.sbj_fam_order[SbjManager.curr_order], 1];
//             badmush.transform.position = SbjManager.context_target[SbjManager.sbj_fam_order[SbjManager.curr_order], 0];
//             Debug.Log($"{goodmush.transform.position}");
//             Debug.Log($"{badmush.transform.position}");
//         }

//         if (SbjManager.curr_task == "fam")
//         {   
//             SbjManager.writer1.WriteLine($"{SbjManager.start_time}s {SbjManager.start_loc} {SbjManager.start_rot} {System.DateTime.Now.ToString("HH:mm:ss.fff")}s {SbjManager.curr_task} {SbjManager.curr_scene} {SbjManager.curr_run}_{SbjManager.curr_trial} {player.transform.position} {SbjManager.context_target[SbjManager.sbj_fam_order[SbjManager.curr_order], 0]} {SbjManager.context_target[SbjManager.sbj_fam_order[SbjManager.curr_order], 1]}"); 
//             SbjManager.writer1.Flush();
//         }
//         else
//         {
//             SbjManager.writer2.WriteLine($"{SbjManager.start_time}s {SbjManager.start_loc} {SbjManager.start_rot} {System.DateTime.Now.ToString("HH:mm:ss.fff")}s {SbjManager.curr_task} {SbjManager.curr_scene} {SbjManager.curr_run}_{SbjManager.curr_trial} {player.transform.position} {SbjManager.context_target[SbjManager.sbj_fam_order[SbjManager.curr_order], 0]} {SbjManager.context_target[SbjManager.sbj_fam_order[SbjManager.curr_order], 1]}"); 
//             SbjManager.writer2.Flush();
//         }

//         // task player
//         yield return new WaitForSeconds(30f); // fam duration (30s)
//         writingtime = false;
//         SbjManager.showFixation = true; RemapManager.fmriWriter.WriteLine($"fix_start" + "\t" + RemapManager.fmriTimer.ToString());
//         secondcam.transform.position = new Vector3(playerCamera.transform.position.x, playerCamera.transform.position.y, playerCamera.transform.position.z);
//         secondcam.transform.rotation = playerCamera.transform.rotation;
//         secondcam.SetActive(true);
//         playerCamera.enabled = false;
//         RemapManager.fmriWriter.WriteLine($"scene_ended" + "\t" + RemapManager.fmriTimer.ToString());
//         RemapManager.fmriWriter.Flush();
//         // FindObjectOfType<RemapManager>()?.OnSceneCompleted();
//         FindObjectOfType<RemapManager_bhv>()?.OnSceneCompleted();
//         // FindObjectOfType<RemapManager_eng>()?.OnSceneCompleted();
//     }
    
//     IEnumerator LogPlayerPosition()
//     {
//         while (writingtime)
//         {
//             Vector3 pos = player.transform.position;
//             string time = System.DateTime.Now.ToString("HH:mm:ss.fff");

//             SbjManager.timeWriter.WriteLine($"{SbjManager.curr_task} {pos} {time}");
//             SbjManager.timeWriter.Flush();

//             yield return new WaitForSeconds(0.5f); 
//         }
//     }


//     void OnDestroy()
//     {
//         SbjManager.CloseLogger();
//     }
// }