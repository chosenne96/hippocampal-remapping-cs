using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ObjectPositionLogger : MonoBehaviour
{
    private StreamWriter objectWriter;
    private int frameCount = 0;
    private bool isLogging = false;
    private List<Renderer> loggableRenderers = new List<Renderer>();
    private float logInterval = 0.5f; // 0.5초마다 로깅
    private float nextLogTime = 0f;

    void Awake()
    {
    }

    void Start()
    {
        InitializeLogger();
        UpdateLoggableObjects();
    }

    private void UpdateLoggableObjects()
    {
        loggableRenderers.Clear();
        var allRenderers = FindObjectsOfType<Renderer>();
        
        foreach (var renderer in allRenderers)
        {
            // Plant가 포함된 오브젝트는 건너뛰기
            if (renderer.gameObject.name.Contains("Plant"))
            {
                continue;
            }

            // mountain이나 room 태그를 가진 오브젝트만 로깅
            if (renderer.gameObject.CompareTag("mountain") || renderer.gameObject.CompareTag("room"))
            {
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.mesh != null)
                {
                    loggableRenderers.Add(renderer);
                }
            }
        }
    }

    private void InitializeLogger()
    {
        // output 폴더 경로 설정 (실행파일 기준)
        string outputDir = Path.Combine(Application.dataPath, "..", "output");
        Directory.CreateDirectory(outputDir);

        // 현재 날짜와 시간(시) 구하기
        string now = System.DateTime.Now.ToString("yyyyMMdd_HH");
        
        // 오브젝트 위치 로그 파일 생성
        string objectPath = Path.Combine(outputDir, $"object_positions_{now}.csv");
        objectWriter = new StreamWriter(objectPath, false) { AutoFlush = true };
        objectWriter.WriteLine("timestamp,frame,object_name,point_index,screen_x,screen_y,world_x,world_y,world_z");
    }

    void Update()
    {
        // 메인 카메라 체크
        if (!isLogging && Camera.main != null)
        {
            isLogging = true;
            Debug.Log($"Main camera found: {Camera.main.name}. Starting object position logging.");
            UpdateLoggableObjects(); // 카메라가 활성화될 때 오브젝트 목록 업데이트
        }
        else if (isLogging && Camera.main == null)
        {
            isLogging = false;
            Debug.Log("Main camera lost. Pausing object position logging.");
        }

        if (isLogging && Time.time >= nextLogTime)
        {
            frameCount++;
            LogObjectPositions();
            nextLogTime = Time.time + logInterval;
        }
    }

    void LogObjectPositions()
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff");
        
        foreach (var renderer in loggableRenderers)
        {
            if (renderer == null) continue;

            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.mesh == null) continue;

            Bounds bounds = renderer.bounds;
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            // 중심점 기록
            LogPosition(timestamp, renderer.gameObject.name, "center", center);

            // 8개의 모서리 점 기록
            Vector3[] corners = new Vector3[]
            {
                new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z), // 왼쪽 아래 뒤
                new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z), // 오른쪽 아래 뒤
                new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z), // 왼쪽 위 뒤
                new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z), // 오른쪽 위 뒤
                new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z), // 왼쪽 아래 앞
                new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z), // 오른쪽 아래 앞
                new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z), // 왼쪽 위 앞
                new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z)  // 오른쪽 위 앞
            };

            for (int i = 0; i < corners.Length; i++)
            {
                LogPosition(timestamp, renderer.gameObject.name, $"corner_{i}", corners[i]);
            }
        }
    }

    private void LogPosition(string timestamp, string objectName, string pointId, Vector3 worldPos)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        
        // 카메라 뒤에 있는 점은 제외
        if (screenPos.z < 0) return;
        
        // 화면 좌표를 0-1 범위로 정규화
        Vector2 normalizedPos = new Vector2(
            screenPos.x / Screen.width,
            screenPos.y / Screen.height
        );
        
        string line = $"{timestamp},{frameCount},{objectName},{pointId}," +
                     $"{normalizedPos.x:F4},{normalizedPos.y:F4}," +
                     $"{worldPos.x:F4},{worldPos.y:F4},{worldPos.z:F4}";
        
        objectWriter.WriteLine(line);
    }

    void OnApplicationQuit()
    {
        objectWriter?.Close();
    }
} 