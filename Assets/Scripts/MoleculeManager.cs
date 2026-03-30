using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Loads and animates XYZ molecular dynamics frames from Resources subfolders.
/// Attach to an empty GameObject in the scene. Assign atomPrefab, hydrogenMaterial, carbonMaterial.
/// Fallback folder: "txt_frames" (existing data). Target folders: "anneal", "tensile".
/// </summary>
public class MoleculeManager : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject atomPrefab;
    public Material hydrogenMaterial;
    public Material carbonMaterial;

    [Header("Settings")]
    public float frameDelay = 0.05f; // 20 FPS
    public string startFolder = "anneal";

    private List<GameObject> atoms = new List<GameObject>();
    private List<List<Vector3>> allFrames = new List<List<Vector3>>();
    private List<int> atomTypes = new List<int>(); // 1=H, 6=C
    private int currentFrame = 0;
    private Coroutine animCoroutine;
    private string loadedFolder = "";

    void Start()
    {
        LoadSimulation(startFolder);
    }

    /// <summary>Called by SimulationMenu buttons.</summary>
    public void LoadSimulation(string folder)
    {
        if (folder == loadedFolder) return;

        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        foreach (var a in atoms) if (a != null) Destroy(a);
        atoms.Clear();
        allFrames.Clear();
        atomTypes.Clear();
        currentFrame = 0;

        string resolvedFolder = LoadAllFrames(folder);
        loadedFolder = resolvedFolder;

        if (allFrames.Count == 0)
        {
            Debug.LogError($"[MoleculeManager] No frames found in Resources/{folder}/ or Resources/txt_frames/");
            return;
        }

        CreateAtoms();
        animCoroutine = StartCoroutine(PlayAnimation());
    }

    /// <summary>Returns the actual folder used (may fall back to txt_frames).</summary>
    string LoadAllFrames(string folder)
    {
        // Try requested folder first, then fall back
        string[] candidates = { folder, "txt_frames" };
        string activeFolder = null;

        foreach (string candidate in candidates)
        {
            TextAsset probe = Resources.Load<TextAsset>($"{candidate}/frame_000");
            if (probe != null)
            {
                activeFolder = candidate;
                break;
            }
        }

        if (activeFolder == null)
        {
            Debug.LogError("[MoleculeManager] Cannot find frame_000 in any Resources subfolder.");
            return folder;
        }

        if (activeFolder != folder)
            Debug.LogWarning($"[MoleculeManager] Folder '{folder}' not found, using '{activeFolder}' instead.");

        // Detect total frame count
        int frameCount = 0;
        for (int i = 0; i < 2000; i++)
        {
            if (Resources.Load<TextAsset>($"{activeFolder}/frame_{i:000}") == null)
            {
                frameCount = i;
                break;
            }
        }

        Debug.Log($"[MoleculeManager] Loading {frameCount} frames from Resources/{activeFolder}/");

        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        for (int i = 0; i < frameCount; i++)
        {
            TextAsset txtFile = Resources.Load<TextAsset>($"{activeFolder}/frame_{i:000}");
            if (txtFile == null) continue;

            string[] lines = txtFile.text.Split('\n');
            List<Vector3> positions = new List<Vector3>();

            // Line 0: atom count, Line 1: frame header, Lines 2+: SYMBOL x y z
            for (int j = 2; j < lines.Length; j++)
            {
                string line = lines[j].Trim();
                if (string.IsNullOrEmpty(line)) continue;
                string[] parts = line.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4) continue;

                if (!float.TryParse(parts[1], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float x)) continue;
                if (!float.TryParse(parts[2], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float y)) continue;
                if (!float.TryParse(parts[3], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float z)) continue;

                Vector3 pos = new Vector3(x, y, z);
                positions.Add(pos);

                if (i == 0)
                    atomTypes.Add(parts[0] == "H" ? 1 : 6);

                min = Vector3.Min(min, pos);
                max = Vector3.Max(max, pos);
            }

            if (positions.Count > 0)
                allFrames.Add(positions);
        }

        // Normalize: center, scale to 1 m bounding box
        Vector3 center = (min + max) / 2f;
        float maxExtent = Mathf.Max(max.x - min.x, Mathf.Max(max.y - min.y, max.z - min.z));
        float scale = (maxExtent > 0f) ? 1.0f / maxExtent : 1f;

        // Molecule is a flat sheet in XY plane — rotate 90° around X to face user
        Quaternion faceForward = Quaternion.Euler(90f, 0f, 0f);
        Vector3 worldOffset = new Vector3(0f, 1.6f, -2f); // eye level, 2 m ahead

        for (int f = 0; f < allFrames.Count; f++)
            for (int a = 0; a < allFrames[f].Count; a++)
            {
                Vector3 local = (allFrames[f][a] - center) * scale;
                allFrames[f][a] = faceForward * local + worldOffset;
            }

        return activeFolder;
    }

    void CreateAtoms()
    {
        if (atomPrefab == null) { Debug.LogError("[MoleculeManager] atomPrefab is not assigned!"); return; }
        int atomCount = allFrames[0].Count;
        for (int i = 0; i < atomCount; i++)
        {
            int typeIdx = (i < atomTypes.Count) ? atomTypes[i] : 6;
            GameObject atom = Instantiate(atomPrefab, allFrames[0][i], Quaternion.identity);
            Renderer r = atom.GetComponent<Renderer>();
            if (r != null)
                r.material = (typeIdx == 1) ? hydrogenMaterial : carbonMaterial;
            atoms.Add(atom);
        }
    }

    IEnumerator PlayAnimation()
    {
        while (true)
        {
            if (allFrames.Count > 0)
            {
                List<Vector3> frame = allFrames[currentFrame % allFrames.Count];
                for (int i = 0; i < atoms.Count && i < frame.Count; i++)
                    if (atoms[i] != null) atoms[i].transform.position = frame[i];

                currentFrame = (currentFrame + 1) % allFrames.Count;
            }
            yield return new WaitForSeconds(frameDelay);
        }
    }
}
