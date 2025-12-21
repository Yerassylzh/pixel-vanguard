using UnityEngine;

public class AutoFPS60Setter : MonoBehaviour
{
    private void Awake()
    {
        QualitySettings.vSyncCount = 0; 
        Application.targetFrameRate = 60;
    }
}
