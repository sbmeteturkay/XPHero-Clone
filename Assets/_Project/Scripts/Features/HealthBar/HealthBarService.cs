using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Zenject;

public class HealthBarService : IInitializable, ILateTickable
{
    [Header("Health Bar Settings")]
    public Material healthBarMaterial;
    public Mesh quadMesh;
    public float healthBarWidth = 2f;
    public float healthBarHeight = 0.3f;
    public float offsetY = 1.5f;
    
    [Header("Performance")]
    public int maxHealthBars = 100;
    
    // Enemy tracking için Dictionary
    private Dictionary<GameObject, HealthBarInstance> activeHealthBars = new Dictionary<GameObject, HealthBarInstance>();
    private List<HealthBarInstance> healthBarInstances = new List<HealthBarInstance>();
    private Queue<int> availableSlots = new Queue<int>(); // Boş slot'ları track et
    
    // Instanced rendering için arrays
    private Matrix4x4[] matrices;
    private MaterialPropertyBlock propertyBlock;
    private Vector4[] healthDataArray;
    
    private Camera mainCamera;
    private string healthDataPropertyName = "_HealthData";
    
    // Health bar instance class'ı
    private class HealthBarInstance
    {
        public GameObject enemy;
        public int slotIndex;
        public Vector3 lastPosition;
        public float lastHealthPercent;
        public float lastDamagePercent;
        public bool needsUpdate;
        public bool isActive;
        
        public HealthBarInstance(int slot)
        {
            slotIndex = slot;
            isActive = false;
            needsUpdate = false;
        }
        
        public void Reset()
        {
            enemy = null;
            isActive = false;
            needsUpdate = false;
            lastHealthPercent = 0;
            lastDamagePercent = 0;
        }
    }

    public HealthBarService(Material healthBarMaterial)
    {
        this.healthBarMaterial = healthBarMaterial;
    }
    
    public void Initialize()
    {
        matrices = new Matrix4x4[maxHealthBars];
        propertyBlock = new MaterialPropertyBlock();
        healthDataArray = new Vector4[maxHealthBars];
        
        // Instance pool'unu oluştur
        for (int i = 0; i < maxHealthBars; i++)
        {
            healthBarInstances.Add(new HealthBarInstance(i));
            availableSlots.Enqueue(i);
            healthDataArray[i] = Vector4.zero;
        }
        
        // PropertyBlock'u initialize et
        propertyBlock.SetVectorArray(healthDataPropertyName, healthDataArray);
        
        mainCamera = Camera.main;
        
        if (quadMesh == null)
            CreateQuadMesh();
    }
    
    void CreateQuadMesh()
    {
        quadMesh = new Mesh();
        
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0), // Sol alt
            new Vector3(0.5f, -0.5f, 0),  // Sağ alt  
            new Vector3(-0.5f, 0.5f, 0),  // Sol üst
            new Vector3(0.5f, 0.5f, 0)    // Sağ üst
        };
        
        Vector2[] uvs = new Vector2[]
        {
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1)
        };
        
        int[] triangles = new int[] { 0, 2, 1, 2, 3, 1 };
        Vector3[] normals = new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
        
        quadMesh.vertices = vertices;
        quadMesh.uv = uvs;
        quadMesh.triangles = triangles;
        quadMesh.normals = normals;
        quadMesh.name = "HealthBar Quad";
    }
    
    public bool RegisterHealthBar(GameObject enemy, Vector3 worldPos, float healthPercent, float damagePercent)
    {
        // Zaten kayıtlı mı kontrol et
        if (activeHealthBars.ContainsKey(enemy))
        {
            Debug.LogWarning($"Enemy {enemy.name} already has a health bar registered!");
            return false;
        }
        
        // Boş slot var mı?
        if (availableSlots.Count == 0)
        {
            Debug.LogWarning("No available health bar slots!");
            return false;
        }
        
        int slotIndex = availableSlots.Dequeue();
        HealthBarInstance instance = healthBarInstances[slotIndex];
        
        // Instance'ı setup et
        instance.enemy = enemy;
        instance.isActive = true;
        instance.lastPosition = worldPos;
        instance.lastHealthPercent = healthPercent;
        instance.lastDamagePercent = damagePercent;
        instance.needsUpdate = true;
        
        // Dictionary'e ekle
        activeHealthBars[enemy] = instance;
        
        return true;
    }
    
    public bool UpdateHealthBar(GameObject enemy, Vector3 worldPos, float healthPercent, float damagePercent)
    {
        if (!activeHealthBars.TryGetValue(enemy, out HealthBarInstance instance))
            return false;
        
        // Değişim var mı kontrol et (gereksiz güncellemeleri önle)
        bool positionChanged = Vector3.Distance(instance.lastPosition, worldPos) > 0.01f;
        bool healthChanged = Mathf.Abs(instance.lastHealthPercent - healthPercent) > 0.001f;
        bool damageChanged = Mathf.Abs(instance.lastDamagePercent - damagePercent) > 0.001f;
        
        if (positionChanged || healthChanged || damageChanged)
        {
            instance.lastPosition = worldPos;
            instance.lastHealthPercent = healthPercent;
            instance.lastDamagePercent = damagePercent;
            instance.needsUpdate = true;
        }
        
        return true;
    }
    
    public bool UnregisterHealthBar(GameObject enemy)
    {
        if (!activeHealthBars.TryGetValue(enemy, out HealthBarInstance instance))
            return false;
        
        // Instance'ı reset et
        int slotIndex = instance.slotIndex;
        instance.Reset();
        
        // Array'i temizle
        matrices[slotIndex] = Matrix4x4.zero;
        healthDataArray[slotIndex] = Vector4.zero;
        
        // Dictionary'den kaldır ve slot'ı geri ver
        activeHealthBars.Remove(enemy);
        availableSlots.Enqueue(slotIndex);
        
        return true;
    }
    
    public void ClearAllHealthBars()
    {
        foreach (var kvp in activeHealthBars)
        {
            HealthBarInstance instance = kvp.Value;
            instance.Reset();
            availableSlots.Enqueue(instance.slotIndex);
        }
        
        activeHealthBars.Clear();
        
        // Arrays'leri temizle
        for (int i = 0; i < maxHealthBars; i++)
        {
            matrices[i] = Matrix4x4.zero;
            healthDataArray[i] = Vector4.zero;
        }
    }
    
    public void LateTick()
    {
        if (activeHealthBars.Count == 0) return;
        
        bool anyUpdated = false;
        Vector3 cameraPos = mainCamera.transform.position;
        
        // Kompakt arrays oluştur - sadece aktif olanlar için
        List<Matrix4x4> activeMatrices = new List<Matrix4x4>();
        List<Vector4> activeHealthData = new List<Vector4>();
        
        // Sadece aktif health bar'ları işle
        foreach (var kvp in activeHealthBars)
        {
            HealthBarInstance instance = kvp.Value;
            if (!instance.isActive) continue;
            
            // Billboard rotation ve matrix güncelleme
            Vector3 targetPos = instance.lastPosition + Vector3.up * offsetY;
            Vector3 direction = (targetPos - cameraPos).normalized;
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 scale = new Vector3(healthBarWidth, healthBarHeight, 1);
            
            Matrix4x4 matrix = Matrix4x4.TRS(targetPos, rotation, scale);
            activeMatrices.Add(matrix);
            
            // Health data oluştur
            Vector4 healthData = new Vector4(
                instance.lastHealthPercent, 
                instance.lastDamagePercent, 
                0, 0
            );
            activeHealthData.Add(healthData);
            
            // Original arrays'i de güncelle (debug için)
            matrices[instance.slotIndex] = matrix;
            healthDataArray[instance.slotIndex] = healthData;
            
            if (instance.needsUpdate)
            {
                instance.needsUpdate = false;
                anyUpdated = true;
            }
        }
        
        // Aktif health bar'ları çiz
        if (activeMatrices.Count > 0)
        {
            // Temporary property block oluştur
            MaterialPropertyBlock tempPropertyBlock = new MaterialPropertyBlock();
            tempPropertyBlock.SetVectorArray(healthDataPropertyName, activeHealthData.ToArray());
            
            Graphics.DrawMeshInstanced(
                quadMesh,
                0,
                healthBarMaterial,
                activeMatrices.ToArray(),
                activeMatrices.Count,
                tempPropertyBlock,
                ShadowCastingMode.Off,
                false
            );
        }
    }
    
    // Bu metod artık kullanılmıyor - LateTick içinde inline olarak yapılıyor
    
    // Debug için
    public int GetActiveHealthBarCount()
    {
        return activeHealthBars.Count;
    }
    
    public int GetAvailableSlotCount()
    {
        return availableSlots.Count;
    }
    
    void OnDestroy()
    {
        ClearAllHealthBars();
    }
}