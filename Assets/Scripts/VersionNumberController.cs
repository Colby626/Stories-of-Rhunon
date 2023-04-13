using UnityEngine;
using TMPro;

public class VersionNumberController : MonoBehaviour
{
    private void Start()
    {
        GetComponent<TextMeshProUGUI>().text = "Version: " + Application.version;
    }
}
