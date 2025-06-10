using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Tobii.Research;
using System.Linq;
using UnityEngine.SceneManagement;  // SceneManager 사용을 위해 추가
using UnityEngine.UI;  // UI 사용을 위해 추가

public class GazeLogger : MonoBehaviour
{
    private static GazeLogger instance;
    private IEyeTracker eyeTracker;
    private StreamWriter gazeWriter;
    private Vector2? latestGazePoint = null;
    private int frameCount = 0;
    private string currentLogPath;
    private float lastLogTime = 0f;
    private const float LOG_INTERVAL = 0.1f;  // 0.1초 간격
    private Camera playerCamera;  // MainCamera 태그가 붙은 카메라
    
    [SerializeField] private bool showGazeMarker = true;  // 시선 마커 표시 여부
    private GameObject gazeMarker;  // 시선 마커 오브젝트
    private Canvas gazeCanvas;  // 시선 마커용 캔버스

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            InitializeLogger();
            SceneManager.sceneLoaded += OnSceneLoaded;  // 씬 로드 이벤트 구독
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 로드될 때마다 카메라 다시 찾기
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<Camera>();
        Debug.Log($"새로운 씬에서 카메라를 찾았습니다: {playerCamera?.name ?? "없음"}");
        
        // 새 씬에서 시선 마커 다시 생성
        if (showGazeMarker)
        {
            CreateGazeMarker();
        }
    }

    private void CreateGazeMarker()
    {
        // 기존 마커가 있다면 제거
        if (gazeMarker != null)
        {
            Destroy(gazeMarker);
        }
        if (gazeCanvas != null)
        {
            Destroy(gazeCanvas.gameObject);
        }

        // 시선 마커용 캔버스 생성
        GameObject canvasObject = new GameObject("GazeMarkerCanvas");
        DontDestroyOnLoad(canvasObject);
        gazeCanvas = canvasObject.AddComponent<Canvas>();
        gazeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        gazeCanvas.sortingOrder = 1000;  // 최상위에 표시

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // 시선 마커 생성
        gazeMarker = new GameObject("GazeMarker");
        gazeMarker.transform.SetParent(gazeCanvas.transform);

        Image markerImage = gazeMarker.AddComponent<Image>();
        markerImage.color = Color.red;
        markerImage.raycastTarget = false;  // 마우스 클릭 방해하지 않도록

        RectTransform rectTransform = gazeMarker.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(20, 20);  // 마커 크기
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        Debug.Log("시선 마커가 생성되었습니다.");
    }

    private void InitializeLogger()
    {
        eyeTracker = EyeTrackingOperations.FindAllEyeTrackers().FirstOrDefault();

        // MainCamera 태그를 가진 카메라 찾기
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<Camera>();
        Debug.Log($"카메라를 찾았습니다: {playerCamera?.name ?? "없음"}");

        // 시선 마커 생성
        if (showGazeMarker)
        {
            CreateGazeMarker();
        }

        // output 폴더 경로 설정 (실행파일 기준)
        string outputDir = Path.Combine(Application.dataPath, "..", "output");
        Directory.CreateDirectory(outputDir);

        // 현재 날짜와 시간(시) 구하기
        string now = System.DateTime.Now.ToString("yyyyMMdd_HH");
        
        // 시선 로그 파일 생성
        currentLogPath = Path.Combine(outputDir, $"gaze_log_{now}.csv");
        
        // 파일이 존재하지 않는 경우에만 헤더 작성
        bool fileExists = File.Exists(currentLogPath);
        gazeWriter = new StreamWriter(currentLogPath, true) { AutoFlush = true };
        
        if (!fileExists)
        {
            gazeWriter.WriteLine("timestamp,frame,gaze_x,gaze_y,hit_object,camera_position,camera_rotation,currtask,currscene");
        }

        // gaze 이벤트 등록
        eyeTracker.GazeDataReceived += OnGazeDataReceived;
    }

    void Update()
    {
        frameCount++;
    }

    void OnGazeDataReceived(object sender, GazeDataEventArgs e)
    {
        Vector2 screenPoint = new Vector2(
            (float)e.LeftEye.GazePoint.PositionOnDisplayArea.X * Screen.width,
            (float)(1 - e.LeftEye.GazePoint.PositionOnDisplayArea.Y) * Screen.height  // Y축 뒤집기
        );

        // 시선 마커 업데이트
        if (showGazeMarker && gazeMarker != null)
        {
            RectTransform rectTransform = gazeMarker.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = screenPoint;
        }

        // 현재 시간이 마지막 로깅 시간 + 간격보다 클 때만 로깅
        if (Time.time >= lastLogTime + LOG_INTERVAL)
        {
            // 기존 로깅 코드에서는 정규화된 좌표 사용
            Vector2 normalizedPoint = new Vector2(
                (float)e.LeftEye.GazePoint.PositionOnDisplayArea.X,
                (float)e.LeftEye.GazePoint.PositionOnDisplayArea.Y
            );

            string timestamp = System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff");
            string hitName = "NaN";
            string camPos = "NaN";
            string camRot = "NaN";
            string currentTask = "NaN";
            string currentScene = "NaN";

            // GUI나 fixation 표시 중일 때
            if (RemapManager_bhv.showGUI || SbjManager.showFixation)
            {
                currentTask = "instruction";
            }
            // 그 외의 경우
            else if (Learning_bhv.showFeedback)
            {
                currentTask = "feedback";
            }
            else
            {
                if (playerCamera != null)
                {
                    Ray ray = playerCamera.ScreenPointToRay(screenPoint);
                    Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 0.1f); // 레이캐스트 시각화
                    
                    if (Physics.Raycast(ray, out RaycastHit hit, 20000f))
                    {
                        hitName = hit.collider.gameObject.name;
                        Debug.Log($"레이캐스트 히트: {hitName}, 거리: {hit.distance}");
                        
                        // 첫 번째 Raycast가 wall에 닿았다면
                        if (hit.collider.gameObject.name.Contains("wall"))
                        {
                            // wall을 통과하는 새로운 Raycast
                            Ray newRay = new Ray(hit.point + ray.direction * 0.1f, ray.direction);
                            if (Physics.Raycast(newRay, out RaycastHit secondHit, 3000f))
                            {
                                // wall 뒤의 오브젝트가 mountain인 경우에만 감지
                                if (secondHit.collider.gameObject.name.Contains("mountain"))
                                {
                                    hitName = secondHit.collider.gameObject.name;
                                }
                            }
                        }
                    }

                    camPos = playerCamera.transform.position.ToString();
                    camRot = playerCamera.transform.eulerAngles.ToString();
                }

                currentTask = string.IsNullOrEmpty(SbjManager.curr_task) ? "NaN" : SbjManager.curr_task;
                currentScene = string.IsNullOrEmpty(SbjManager.curr_scene) ? "NaN" : SbjManager.curr_scene;
            }

            gazeWriter.WriteLine($"{timestamp},{frameCount},{normalizedPoint.x:F4},{normalizedPoint.y:F4},{hitName},{camPos},{camRot},{currentTask},{currentScene}");
            lastLogTime = Time.time;  // 마지막 로깅 시간 업데이트
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;  // 씬 로드 이벤트 구독 해제
            eyeTracker.GazeDataReceived -= OnGazeDataReceived;
            gazeWriter?.Close();
            gazeWriter = null;
            
            if (gazeMarker != null) Destroy(gazeMarker);
            if (gazeCanvas != null) Destroy(gazeCanvas.gameObject);
        }
    }

    void OnApplicationQuit()
    {
        eyeTracker.GazeDataReceived -= OnGazeDataReceived;
        gazeWriter?.Close();
    }
}