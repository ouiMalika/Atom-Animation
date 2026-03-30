using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoleculeManager : MonoBehaviour
{
    public GameObject atomPrefab;
    public Material Mat_H;
    public Material Mat_C;

    private List<GameObject> atoms = new List<GameObject>();
    private Coroutine currentAnimation;

    public void PlayAnimationFromFolder(string folderName)
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(Animate(folderName));
    }

    private IEnumerator Animate(string folder)
    {
        TextAsset[] frames = Resources.LoadAll<TextAsset>(folder);
        System.Array.Sort(frames, (a, b) => a.name.CompareTo(b.name));

        if (frames.Length == 0)
        {
            Debug.LogWarning($"No frames found in Resources/{folder}");
            yield break;
        }

        if (atoms.Count == 0)
        {
            string[] lines = frames[0].text.Split('\n');
            foreach (string line in lines)
            {
                string[] parts = line.Split(' ');
                if (parts.Length < 4) continue;

                string element = parts[0];
                float x = float.Parse(parts[1]);
                float y = float.Parse(parts[2]);
                float z = float.Parse(parts[3]);

                GameObject atom = Instantiate(atomPrefab, new Vector3(x, y, z), Quaternion.identity);

                // Assign material based on element
                if (element == "H")
                    atom.GetComponent<Renderer>().material = Mat_H;
                else if (element == "C")
                    atom.GetComponent<Renderer>().material = Mat_C;

                atoms.Add(atom);
            }
        }

        foreach (TextAsset frame in frames)
        {
            string[] lines = frame.text.Split('\n');
            for (int i = 0; i < atoms.Count && i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(' ');
                if (parts.Length < 4) continue;

                float x = float.Parse(parts[1]);
                float y = float.Parse(parts[2]);
                float z = float.Parse(parts[3]);

                atoms[i].transform.localPosition = new Vector3(x, y, z);
            }

            yield return null;
        }
    }
}
