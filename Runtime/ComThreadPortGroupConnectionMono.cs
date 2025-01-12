using Eloi.Port;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;




public class ComThreadPortGroupConnectionMono : MonoBehaviour
{
    public ComPortInformationHolderMono m_comPortInformationHolderMono;
    public string[] m_comToFind = new string[] { "98D371F70675", "98D331F71FD3", "98D361F74044", "98D331F71DA0" };
    public int m_baudRate = 9600;
    public PortNameListenType m_listenType = PortNameListenType.TextUTF8;
    public bool m_autoLoadAtEnable = true;
    public ComPortInformationHolderMono m_informationHolder;

    [Header("Events")]
    public UnityEvent<ComPortToInfo, string> m_onTextReceivedOnUnityThread;
    public UnityEvent<ComPortToInfo, byte> m_onByteReceivedOnUnityThread;
    public UnityEvent<ComPortToInfo, byte[]> m_onBytesGroupReceivedOnUnityThread;


    [Header("Debug")]
    public List<ComThreadPortConnectionMono> m_portConnections = new List<ComThreadPortConnectionMono>();

    public void OnEnable()
    {
        throw new Exception("I failed to fin a way to kill the thread in all condition, so I will not use it for now.");
        if (m_autoLoadAtEnable) 
            LaunchWithDelay();
    }
    public void LaunchWithDelay()
    {
        StartCoroutine(LaunchWithDelayCoroutine());
    }

    private IEnumerator LaunchWithDelayCoroutine()
    {
        yield return new WaitForSeconds(1);
        LoadPortConnections();
    }

    public void OnDisable()
    {
        Debug.Log("OnDisable", this.gameObject);
        KillConnections();
    }

    public void OnDestroy()
    {
        Debug.Log("OnDestroy", this.gameObject);
        KillConnections();
    }

    public void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit", this.gameObject);
        KillConnections();
    }
    private void KillConnections()
    {
        
        foreach (ComThreadPortConnectionMono item in m_portConnections)
        {
            if (Application.isPlaying)
                Destroy(item.gameObject);
            else
                DestroyImmediate(item.gameObject);
        }
        m_portConnections.Clear();
    }
    public bool m_catchExceptionOnLoad = false;
    private void LoadPortConnections()
    {

        KillConnections();
        foreach (string item in m_comToFind)
        {
            m_comPortInformationHolderMono.TryToFindFromContainsText(item, true, out bool found, out ComPortToInfo info);
            if (found)
            {
                GameObject go = new GameObject("ComThreadPortConnectionMono_" + item);
                ComThreadPortConnectionMono newConnection = go.AddComponent<ComThreadPortConnectionMono>();
                newConnection.m_comPortInformationHolderMono = m_comPortInformationHolderMono;
                newConnection.m_portToFind = item;
                newConnection.m_baudRate = m_baudRate;
                newConnection.m_listenType = m_listenType;
                newConnection.m_autoLoadAtEnable = true;
                newConnection.m_onTextReceivedOnUnityThread = m_onTextReceivedOnUnityThread;
                newConnection.m_onByteReceivedOnUnityThread = m_onByteReceivedOnUnityThread;
                newConnection.m_onBytesGroupReceivedOnUnityThread = m_onBytesGroupReceivedOnUnityThread;
                go.transform.SetParent(this.transform);
                m_portConnections.Add(newConnection);
                newConnection.LaunchWithDelay(1);
            }
        }
       
    }

    public void SendIntegerLittleEndian(int value)
    {
        foreach (ComThreadPortConnectionMono item in m_portConnections)
        {
            item.SendIntegerLittleEndian(value);
        }
    }

    public void SendDataAsTextCharArray(string text)
    {
        foreach (ComThreadPortConnectionMono item in m_portConnections)
        {
            item.SendDataAsTextCharArray(text);
        }
    }

    public void SendDataAsTextWithLineReturn(string text)
    {
        foreach (ComThreadPortConnectionMono item in m_portConnections)
        {
            item.SendDataAsTextCharArray(text+"\n");
        }
    }
    public void SendIntegerAsTextWithLineReturn(int value)
    {
        foreach (ComThreadPortConnectionMono item in m_portConnections)
        {
            item.SendIntegerAsTextWithLineReturn(value);
        }
    }
    public void SendDataAsBytes(byte[] bytes)
    {
        foreach (ComThreadPortConnectionMono item in m_portConnections)
        {
            item.SendDataAsBytes(bytes);
        }
    }
    public void SendDataAsByte(byte byteData)
    {
        foreach (ComThreadPortConnectionMono item in m_portConnections)
        {
            item.SendDataAsByte(byteData);
        }
    
    }
   
}
