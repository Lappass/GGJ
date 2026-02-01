using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public class MaskMakerTool : EditorWindow
{
    private GameObject maskRoot;
    private Sprite baseImage;
    private List<SocketDef> socketDefinitions = new List<SocketDef>();
    private Vector2 scrollPos;

    [System.Serializable]
    public class SocketDef
    {
        public string name = "New Socket";
        public MaskPartType type = MaskPartType.Eye;
        public Vector2 position; // Relative to center
        public float radius = 50f;
    }

    [MenuItem("Tools/Mask Maker Tool")]
    public static void ShowWindow()
    {
        GetWindow<MaskMakerTool>("Mask Maker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Mask Assembly Setup", EditorStyles.boldLabel);

        // 1. Root Setup
        GUILayout.Space(10);
        GUILayout.Label("1. Setup Base", EditorStyles.label);
        maskRoot = (GameObject)EditorGUILayout.ObjectField("Mask Root (Panel)", maskRoot, typeof(GameObject), true);
        baseImage = (Sprite)EditorGUILayout.ObjectField("Base Sprite", baseImage, typeof(Sprite), false);

        if (GUILayout.Button("Setup/Update Base Image"))
        {
            SetupBase();
        }

        // 2. Sockets Setup
        GUILayout.Space(20);
        GUILayout.Label("2. Define Sockets", EditorStyles.label);

        if (GUILayout.Button("Add Socket Definition"))
        {
            socketDefinitions.Add(new SocketDef());
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));
        for (int i = 0; i < socketDefinitions.Count; i++)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Socket {i + 1}");
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                socketDefinitions.RemoveAt(i);
                i--;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                continue;
            }
            EditorGUILayout.EndHorizontal();

            socketDefinitions[i].name = EditorGUILayout.TextField("Name", socketDefinitions[i].name);
            socketDefinitions[i].type = (MaskPartType)EditorGUILayout.EnumPopup("Type", socketDefinitions[i].type);
            socketDefinitions[i].radius = EditorGUILayout.FloatField("Snap Radius", socketDefinitions[i].radius);
            // Position could be visual, but hard to edit in inspector. 
            // We'll rely on generating GameObjects and letting user move them.
            
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Generate Sockets"))
        {
            GenerateSockets();
        }
        
        // 3. Assembler Setup
        GUILayout.Space(20);
        if (GUILayout.Button("Setup Assembler Component"))
        {
            SetupAssembler();
        }
    }

    private void SetupBase()
    {
        if (maskRoot == null)
        {
            EditorUtility.DisplayDialog("Error", "Assign Mask Root first!", "OK");
            return;
        }

        Image img = maskRoot.GetComponent<Image>();
        if (img == null) img = maskRoot.AddComponent<Image>();
        
        if (baseImage != null)
        {
            img.sprite = baseImage;
            img.SetNativeSize();
        }
    }

    private void GenerateSockets()
    {
        if (maskRoot == null) return;

        foreach (var def in socketDefinitions)
        {
            // Check if exists
            Transform existing = maskRoot.transform.Find(def.name);
            GameObject socketObj;
            
            if (existing != null)
            {
                socketObj = existing.gameObject;
            }
            else
            {
                socketObj = new GameObject(def.name);
                socketObj.transform.SetParent(maskRoot.transform, false);
                socketObj.transform.localPosition = Vector3.zero; // Default to center
                
                // Add an empty image for raycast target if needed, or just keep it empty for pure transform
                // Usually Socket doesn't need Image unless we want to show a "slot" graphic.
                // But for debugging, let's add a small icon or just RectTransform.
                RectTransform rt = socketObj.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(def.radius * 2, def.radius * 2);
            }

            MaskSocket socket = socketObj.GetComponent<MaskSocket>();
            if (socket == null) socket = socketObj.AddComponent<MaskSocket>();

            socket.acceptedType = def.type;
            socket.snapRadius = def.radius;
        }
        
        Debug.Log("Sockets Generated!");
    }

    private void SetupAssembler()
    {
        if (maskRoot == null) return;

        MaskAssembler assembler = maskRoot.GetComponent<MaskAssembler>();
        if (assembler == null) assembler = maskRoot.AddComponent<MaskAssembler>();

        // Auto-assign all child sockets
        assembler.requiredSockets = new List<MaskSocket>();
        var sockets = maskRoot.GetComponentsInChildren<MaskSocket>();
        assembler.requiredSockets.AddRange(sockets);
        
        Debug.Log("Assembler Configured!");
    }
}



