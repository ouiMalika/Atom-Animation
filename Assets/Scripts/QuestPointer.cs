using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using System.Collections.Generic;

/// <summary>
/// Attaches to the Right Controller GameObject (child of XR Origin > Camera Offset).
/// Draws a laser pointer and drives SimulationMenu button hover/press via trigger.
///
/// SETUP:
///   1. Expand XR Origin > Camera Offset > Right Controller
///   2. Add this component to "Right Controller"
///   3. Assign simulationMenu reference in Inspector
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class QuestPointer : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public SimulationMenu simulationMenu;

    [Header("Settings")]
    public float maxRayDistance = 10f;
    public Color laserColor = new Color(0.2f, 0.6f, 1f, 0.8f);
    public float laserWidth = 0.003f;

    private LineRenderer laser;
    private bool triggerWasPressedLastFrame = false;

    void Awake()
    {
        laser = GetComponent<LineRenderer>();
        laser.positionCount = 2;
        laser.startWidth = laserWidth;
        laser.endWidth = laserWidth * 0.3f;
        laser.material = CreateLaserMaterial();
        laser.startColor = laserColor;
        laser.endColor = new Color(laserColor.r, laserColor.g, laserColor.b, 0f);
        laser.useWorldSpace = true;
        laser.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        laser.receiveShadows = false;
    }

    void Update()
    {
        bool triggerDown = GetTrigger();

        // Raycast from controller forward
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        bool didHit = Physics.Raycast(ray, out hit, maxRayDistance);

        // Update laser endpoints
        Vector3 laserEnd = didHit ? hit.point : ray.origin + ray.direction * maxRayDistance;
        laser.SetPosition(0, ray.origin);
        laser.SetPosition(1, laserEnd);

        // Check which button (if any) was hit
        Button hitButton = null;
        if (didHit && simulationMenu != null)
        {
            // Check if the collider belongs to an Anneal or Tensile button
            Transform t = hit.collider.transform;
            while (t != null)
            {
                Button uiBtn = t.GetComponent<Button>();
                if (uiBtn == simulationMenu.annealButton || uiBtn == simulationMenu.tensileButton)
                {
                    hitButton = uiBtn;
                    break;
                }
                t = t.parent;
            }
        }

        // Hover
        if (simulationMenu != null)
            simulationMenu.SetHover(hitButton);

        // Trigger press (rising edge)
        if (triggerDown && !triggerWasPressedLastFrame)
        {
            if (simulationMenu != null && hitButton != null)
                simulationMenu.PressHovered();
        }

        if (!triggerDown && triggerWasPressedLastFrame && simulationMenu != null)
            simulationMenu.ClearHover(); // refresh colors after release

        triggerWasPressedLastFrame = triggerDown;
    }

    bool GetTrigger()
    {
        // Primary: XR InputDevices (works with OpenXR on Quest 3S)
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
        foreach (var device in devices)
        {
            bool pressed;
            if (device.TryGetFeatureValue(CommonUsages.triggerButton, out pressed) && pressed)
                return true;
            // Also check analog trigger axis (> 0.5 threshold)
            float axis;
            if (device.TryGetFeatureValue(CommonUsages.trigger, out axis) && axis > 0.5f)
                return true;
        }
        return false;
    }

    Material CreateLaserMaterial()
    {
        // Use Sprites/Default (URP-compatible unlit), fallback to Standard
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        Material mat = new Material(shader != null ? shader : Shader.Find("Standard"));
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        return mat;
    }
}
