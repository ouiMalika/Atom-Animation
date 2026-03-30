using UnityEngine;

public class AnimationSwitcher : MonoBehaviour
{
    public MoleculeManager moleculeManager;

    public void PlayAnneal()
    {
        Debug.Log("PlayAnneal triggered");
        moleculeManager.PlayAnimationFromFolder("anneal");
    }

    public void PlayTensile()
    {
        Debug.Log("PlayTensile triggered");
        moleculeManager.PlayAnimationFromFolder("tensile");
    }

    public void PlayCustom()
    {
        Debug.Log("PlayCustom triggered");
        moleculeManager.PlayAnimationFromFolder("custom");
    }
}
