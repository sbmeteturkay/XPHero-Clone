using UnityEngine;

public class HealthBarDebugger : MonoBehaviour
{
    public HealthBarService HealthBarService;
    public bool showDebugInfo = true;
    
    void Update()
    {
        // Test için - WASD ile health bar test et
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestHealthBar();
        }
        
        // Debug bilgileri
        if (showDebugInfo)
        {
            DrawDebugInfo();
        }
    }
    
    void TestHealthBar()
    {
        // Kameranın 5 birim önüne test health bar'ı koy
        Vector3 testPos = Camera.main.transform.position + Camera.main.transform.forward * 5f;
        
        //HealthBarService.ClearHealthBars();
        //HealthBarService.RegisterHealthBar(testPos, 0.7f, 0.2f); // %70 health, %20 damage
        
        Debug.Log($"Test Health Bar: {testPos}");
    }
    
    void DrawDebugInfo()
    {
        // Kamera bilgileri
        Camera cam = Camera.main;
        if (cam != null)
        {
            // Kamera pozisyonu ve yönü
            Debug.DrawRay(cam.transform.position, cam.transform.forward * 10f, Color.blue);
            Debug.DrawRay(cam.transform.position, cam.transform.up * 2f, Color.green);
            Debug.DrawRay(cam.transform.position, cam.transform.right * 2f, Color.red);
        }
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Box("Health Bar Debug");
        
        if (HealthBarService != null)
        {
            //GUILayout.Label($"Active Health Bars: {healthBarManager.activeCount}");
            GUILayout.Label($"Max Health Bars: {HealthBarService.maxHealthBars}");
        }
        
        Camera cam = Camera.main;
        if (cam != null)
        {
            GUILayout.Label($"Camera Pos: {cam.transform.position}");
            GUILayout.Label($"Camera Rot: {cam.transform.rotation.eulerAngles}");
        }
        
        if (GUILayout.Button("Test Health Bar (Space)"))
        {
            TestHealthBar();
        }
        
        if (GUILayout.Button("Clear All"))
        {
            //HealthBarService.ClearHealthBars();
        }
        
        GUILayout.EndArea();
    }
}