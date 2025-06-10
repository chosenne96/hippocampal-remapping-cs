// using UnityEngine;
// using System.IO;
// using System;
// using System.Linq;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine.SceneManagement;

// public class ODTScript : MonoBehaviour
// {
//     private Sprite[] flowerList = new Sprite[20];
//     public Sprite target;
//     public string curr_flower;
//     public float duration, isi;
//     public string start_time;

//     private bool showGUI = false;
//     private bool showFixation = false;
//     private bool showFlower = false;
//     private string guiMessage = "";
//     private Texture2D flowerTexture;
//     private Texture2D backgroundTex;

//     private string ori_dir, bhv1_dir, bhv2_dir, time_dir, coin1_dir, coin2_dir, coin3_dir, odt1_dir, odt2_dir, tmpDate;
    
//     private System.DateTime odtStart;
//     private System.DateTime objStart;
//     private System.DateTime detectTime;
//     private int keyPressCount = 0;
//     private bool objDetect = false;

//     void Awake()
//     {
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
//         SbjManager.FileLogger(bhv1_dir, bhv2_dir, time_dir, coin1_dir, coin2_dir, coin3_dir, tmpDate, SbjManager.sbj_num);

//     }

//     void Start()
//     {
//         backgroundTex = new Texture2D(1, 1);
//         backgroundTex.SetPixel(0, 0, new Color32(217, 217, 217, 255));
//         backgroundTex.Apply();

//         odtStart = System.DateTime.Now;
//         StartCoroutine(PlaySequence());
//     }

//     IEnumerator PlaySequence()
//     {
//         for (int seq = 0; seq < 3; seq++)
//         {
//             for (int i = 0; i < 24; i++)
//             {
//                 Sprite currentSprite = SbjManager.flowerSeq[seq, i];
//                 curr_flower = currentSprite.name;

//                 flowerTexture = SpriteToTexture(currentSprite);
//                 start_time = System.DateTime.Now.ToString("HH:mm:ss");
//                 objStart = System.DateTime.Now;
//                 keyPressCount = 0;
//                 objDetect = false;
//                 detectTime = default;

//                 showFlower = true;
//                 showFixation = false;

//                 duration = 4f;
//                 yield return new WaitForSeconds(duration);

//                 showFlower = false;
//                 flowerTexture = null;
//                 showGUI = false;

//                 TimeSpan startDiff = objStart - odtStart;
//                 TimeSpan detectDiff = objDetect ? detectTime - odtStart : TimeSpan.Zero;

//                 string detectStr = objDetect
//                     ? $"detect: True, detectTime: {detectTime:HH:mm:ss}, detectDelay: {detectDiff.TotalSeconds:F2}s"
//                     : "detect: False";
                
//                 if (SbjManager.curr_odt_run == 0)
//                 {
//                     SbjManager.odtWriter1.WriteLine($"{SbjManager.curr_odt_trial} {curr_flower} {objStart:HH:mm:ss} {startDiff.TotalSeconds:F2}s, {detectStr}");
//                     SbjManager.odtWriter1.Flush();
//                 }
//                 else if (SbjManager.curr_odt_run == 1)
//                 {
//                     SbjManager.odtWriter2.WriteLine($"{SbjManager.curr_odt_trial} {curr_flower} {objStart:HH:mm:ss} {startDiff.TotalSeconds:F2}s, {detectStr}");
//                     SbjManager.odtWriter2.Flush();
//                 }

//                 SbjManager.curr_odt_trial++;

//                 isi = curr_flower == "Magic flowers-82" ? 6f : 4f;

//                 showFixation = true;
//                 yield return new WaitForSeconds(isi);
//                 showFixation = false;
//             }
//         }

//         Debug.Log("ODT ended");
//         FindObjectOfType<RemapManager>()?.OnSceneCompleted();
//     }

//     void Update()
//     {
//         if (showFlower && Input.GetKeyDown(KeyCode.Alpha4))
//         {
//             if (!objDetect)
//             {
//                 objDetect = true;
//                 detectTime = System.DateTime.Now;
//             }

//             showGUI = true;
//             guiMessage = "버튼을 눌렀습니다.";
//         }
//     }

//     void OnGUI()
//     {
//         if (showGUI || showFixation || showFlower)
//         {
//             GUIStyle bgStyle = new GUIStyle();
//             bgStyle.normal.background = backgroundTex;
//             GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, bgStyle);
//         }

//         if (showFlower && flowerTexture != null)
//         {
//             int size = 500;
//             Rect imgRect = new Rect(Screen.width / 2 - size / 2, Screen.height / 2 - size / 2, size, size);
//             GUI.DrawTexture(imgRect, flowerTexture, ScaleMode.ScaleToFit, true);
//         }

//         if (showGUI)
//         {
//             GUIStyle guiStyle = new GUIStyle();
//             guiStyle.fontSize = 60;
//             guiStyle.fontStyle = FontStyle.Bold;
//             guiStyle.normal.textColor = Color.black;
//             guiStyle.alignment = TextAnchor.MiddleCenter;
//             guiStyle.wordWrap = true;

//             Rect messageRect = new Rect(Screen.width / 2 - 600, Screen.height - 200, 1200, 100);
//             GUI.Label(messageRect, guiMessage, guiStyle);
//         }

//         if (showFixation)
//         {
//             GUIStyle textStyle = new GUIStyle();
//             textStyle.fontSize = 100;
//             textStyle.alignment = TextAnchor.MiddleCenter;
//             textStyle.normal.textColor = Color.black;

//             Rect rect = new Rect(Screen.width / 2 - 25, Screen.height / 2 - 25, 50, 50);
//             GUI.Label(rect, "+", textStyle);
//         }
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

//         if (flowerTexture != null)
//         {
//             Destroy(flowerTexture);
//         }

//         return newTex;
//     }

//     void OnDestroy()
//     {
//         SbjManager.CloseLogger();
//     }
// }
