using UnityEngine;


[ExecuteInEditMode]
public class CancelThreadPortConnectionMono : MonoBehaviour
{

    public static CancelThreadPortConnectionMono m_instance;
    public static bool HasInstance() { return m_instance != null; }
    public static void CreateInstanceIfNoneInScene()
    {
        if (!HasInstance())
        {
            GameObject go = new GameObject("CancelThreadPortConnectionMono");
            m_instance = go.AddComponent<CancelThreadPortConnectionMono>();
        }
    }

    public void Update()
    {
        if (Application.isPlaying == false)
            ComThreadPortConnection.KillAllThread();

    }
    public void Awake()
    {
        m_instance = this;
    }
    public void OnDestroy()
    {
        ComThreadPortConnection.KillAllThread();
    }
    public void OnApplicationQuit()
    {
        ComThreadPortConnection.KillAllThread();
    }
    public void OnDisable()
    {
        ComThreadPortConnection.KillAllThread();
    }
}
