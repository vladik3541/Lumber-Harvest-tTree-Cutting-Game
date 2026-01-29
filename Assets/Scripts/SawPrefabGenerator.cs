using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Помічник для швидкого створення префабів пилок
/// </summary>
public class SawPrefabGenerator
{
#if UNITY_EDITOR
    [MenuItem("Tools/Create Upgrade System/Generate Example Saw Prefabs")]
    public static void GenerateExampleSawPrefabs()
    {
        // Створюємо папку для пилок
        string folderPath = "Assets/UpgradeSystem/SawPrefabs";
        
        if (!AssetDatabase.IsValidFolder("Assets/UpgradeSystem"))
        {
            AssetDatabase.CreateFolder("Assets", "UpgradeSystem");
        }
        
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/UpgradeSystem", "SawPrefabs");
        }
        
        // Створюємо пилки різних рівнів
        CreateSawPrefab("BasicSaw", 0.3f, new Color(0.5f, 0.5f, 0.5f), 360f, folderPath);
        CreateSawPrefab("BronzeSaw", 0.4f, new Color(0.8f, 0.5f, 0.2f), 450f, folderPath);
        CreateSawPrefab("SilverSaw", 0.5f, new Color(0.75f, 0.75f, 0.75f), 540f, folderPath);
        CreateSawPrefab("GoldSaw", 0.6f, new Color(1f, 0.84f, 0f), 630f, folderPath);
        CreateSawPrefab("PlatinumSaw", 0.7f, new Color(0.9f, 0.9f, 1f), 720f, folderPath);
        CreateSawPrefab("DiamondSaw", 0.8f, new Color(0.5f, 1f, 1f), 900f, folderPath);
        CreateSawPrefab("CosmicSaw", 1.0f, new Color(0.5f, 0f, 1f), 1080f, folderPath);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Створено 7 префабів пилок в {folderPath}");
    }
    
    private static void CreateSawPrefab(string name, float size, Color color, float rotationSpeed, string folderPath)
    {
        // Створюємо GameObject
        GameObject sawObject = new GameObject(name);
        
        // Додаємо візуал (циліндр для пилки)
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.name = "Visual";
        visual.transform.SetParent(sawObject.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.Euler(90, 0, 0); // Обертаємо щоб була плоска
        visual.transform.localScale = new Vector3(size, 0.05f, size);
        
        // Створюємо матеріал
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        
        // Для дорогих пилок додаємо emission
        if (name.Contains("Platinum") || name.Contains("Diamond") || name.Contains("Cosmic"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.5f);
        }
        
        visual.GetComponent<Renderer>().material = mat;
        
        // Зберігаємо матеріал
        string matPath = $"{folderPath}/{name}_Material.mat";
        AssetDatabase.CreateAsset(mat, matPath);
        
        // Видаляємо колайдер (не потрібен)
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        
        // Додаємо компонент обертання
        SawRotation rotation = sawObject.AddComponent<SawRotation>();
        
        // Використовуємо Reflection щоб встановити приватні поля
        var rotationSpeedField = typeof(SawRotation).GetField("rotationSpeed", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var rotationAxisField = typeof(SawRotation).GetField("rotationAxis", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (rotationSpeedField != null)
            rotationSpeedField.SetValue(rotation, rotationSpeed);
        if (rotationAxisField != null)
            rotationAxisField.SetValue(rotation, Vector3.forward);
        
        // Для високих рівнів додаємо партикли
        if (name.Contains("Diamond") || name.Contains("Cosmic"))
        {
            GameObject particles = new GameObject("Particles");
            particles.transform.SetParent(sawObject.transform);
            particles.transform.localPosition = Vector3.zero;
            
            ParticleSystem ps = particles.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startSize = 0.1f;
            main.startSpeed = 0.5f;
            main.maxParticles = 20;
            
            var emission = ps.emission;
            emission.rateOverTime = 10;
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = size * 0.5f;
        }
        
        // Зберігаємо як префаб
        string prefabPath = $"{folderPath}/{name}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(sawObject, prefabPath);
        
        // Видаляємо тимчасовий об'єкт зі сцени
        Object.DestroyImmediate(sawObject);
        
        Debug.Log($"Створено префаб: {prefabPath}");
    }
    
    [MenuItem("Tools/Create Upgrade System/Create Custom Saw Prefab")]
    public static void CreateCustomSawPrefabWindow()
    {
        SawPrefabCreatorWindow.ShowWindow();
    }
#endif
}

#if UNITY_EDITOR
public class SawPrefabCreatorWindow : EditorWindow
{
    private string sawName = "CustomSaw";
    private float sawSize = 0.5f;
    private Color sawColor = Color.white;
    private float rotationSpeed = 360f;
    private bool addEmission = false;
    private bool addParticles = false;
    
    [MenuItem("Tools/Create Upgrade System/Saw Prefab Creator")]
    public static void ShowWindow()
    {
        GetWindow<SawPrefabCreatorWindow>("Saw Creator");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Створення префабу пилки", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        sawName = EditorGUILayout.TextField("Назва пилки", sawName);
        sawSize = EditorGUILayout.Slider("Розмір", sawSize, 0.1f, 2.0f);
        sawColor = EditorGUILayout.ColorField("Колір", sawColor);
        rotationSpeed = EditorGUILayout.Slider("Швидкість обертання", rotationSpeed, 100f, 2000f);
        
        EditorGUILayout.Space();
        
        addEmission = EditorGUILayout.Toggle("Додати свічення", addEmission);
        addParticles = EditorGUILayout.Toggle("Додати партикли", addParticles);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Створити префаб"))
        {
            CreateCustomSaw();
        }
    }
    
    private void CreateCustomSaw()
    {
        string folderPath = "Assets/UpgradeSystem/SawPrefabs";
        
        if (!AssetDatabase.IsValidFolder("Assets/UpgradeSystem"))
        {
            AssetDatabase.CreateFolder("Assets", "UpgradeSystem");
        }
        
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/UpgradeSystem", "SawPrefabs");
        }
        
        // Створюємо GameObject
        GameObject sawObject = new GameObject(sawName);
        
        // Додаємо візуал
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.name = "Visual";
        visual.transform.SetParent(sawObject.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.Euler(90, 0, 0);
        visual.transform.localScale = new Vector3(sawSize, 0.05f, sawSize);
        
        // Створюємо матеріал
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = sawColor;
        
        if (addEmission)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", sawColor * 0.5f);
        }
        
        visual.GetComponent<Renderer>().material = mat;
        
        // Зберігаємо матеріал
        string matPath = $"{folderPath}/{sawName}_Material.mat";
        AssetDatabase.CreateAsset(mat, matPath);
        
        // Видаляємо колайдер
        DestroyImmediate(visual.GetComponent<Collider>());
        
        // Додаємо обертання
        SawRotation rotation = sawObject.AddComponent<SawRotation>();
        
        var rotationSpeedField = typeof(SawRotation).GetField("rotationSpeed", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var rotationAxisField = typeof(SawRotation).GetField("rotationAxis", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (rotationSpeedField != null)
            rotationSpeedField.SetValue(rotation, rotationSpeed);
        if (rotationAxisField != null)
            rotationAxisField.SetValue(rotation, Vector3.forward);
        
        // Партикли
        if (addParticles)
        {
            GameObject particles = new GameObject("Particles");
            particles.transform.SetParent(sawObject.transform);
            particles.transform.localPosition = Vector3.zero;
            
            ParticleSystem ps = particles.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = sawColor;
            main.startSize = 0.1f;
            main.startSpeed = 0.5f;
            main.maxParticles = 20;
            
            var emission = ps.emission;
            emission.rateOverTime = 10;
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = sawSize * 0.5f;
        }
        
        // Зберігаємо як префаб
        string prefabPath = $"{folderPath}/{sawName}.prefab";
        PrefabUtility.SaveAsPrefabAsset(sawObject, prefabPath);
        
        // Видаляємо зі сцени
        DestroyImmediate(sawObject);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Створено префаб: {prefabPath}");
        EditorUtility.DisplayDialog("Успіх", $"Префаб {sawName} створено!", "OK");
    }
}
#endif
