using UnityEngine;
using UnityEngine.Rendering;

public class UnityDebugFixer : MonoBehaviour
{
    private void Awake()
    {
        DebugManager.instance.enableRuntimeUI = false;
    }
}
