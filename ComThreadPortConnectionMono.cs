using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using Eloi.Port;
using System.Collections;

public class ComThreadPortConnectionMono : MonoBehaviour
{

    public ComPortInformationHolderMono m_comPortInformationHolderMono;
    public string m_portToFind = "COM3";
    public int m_baudRate = 9600;
    public PortNameListenType m_listenType = PortNameListenType.TextUTF8;
    public bool m_autoLoadAtEnable = false;

    [Header("Events")]
    public UnityEvent<ComPortToInfo, string> m_onTextReceivedOnUnityThread;
    public UnityEvent<ComPortToInfo, byte> m_onByteReceivedOnUnityThread;
    public UnityEvent<ComPortToInfo, byte[]> m_onBytesGroupReceivedOnUnityThread;


    [Header("Debug")]
    public ComThreadPortConnection.KillSwitch m_killSwitchState;
    public ComThreadPortConnection m_portConnection;

    public string m_lastReceivedData = "";



    public void LaunchWithDelay(float time)
    {
        StartCoroutine(LaunchWithDelayCoroutine(time));

    }
    private IEnumerator LaunchWithDelayCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        LoadPortConnection();
    }


    public void OnEnable()
    {
        if (m_autoLoadAtEnable)
            LoadPortConnection();
    }
    public void OnDisable()
    {
        KillConnection();
    }

    public void OnDestroy()
    {
        KillConnection();
        Debug.Log("OnDestroy",this.gameObject);
    }

    public void OnApplicationQuit()
    {
        KillConnection();
    }
    

    private void KillConnection()
    {
        if (IsUsable())
            m_portConnection.KillCurrentConnection();
    }

    public  void LoadPortConnection()
    {
        { 
            m_comPortInformationHolderMono.TryToFindFromContainsText(m_portToFind, true, out bool found, out ComPortToInfo info);
            if (IsUsable())
                m_portConnection.KillCurrentConnection();

            if (found) { 
                m_portConnection = new ComThreadPortConnection(info, out m_killSwitchState, m_baudRate, m_listenType);
                m_portConnection.m_onReceivedMessageOnUnityThread.AddListener((string data) => { m_onTextReceivedOnUnityThread.Invoke(info, data); m_lastReceivedData = data; }) ;
                m_portConnection.m_onReceivedByteOnUnityThread.AddListener((byte data) => { m_onByteReceivedOnUnityThread.Invoke(info, data); m_lastReceivedData = ""+data;  });
                m_portConnection.m_onReceivedBytesGroupOnUnityThread.AddListener((byte[] data) => { m_onBytesGroupReceivedOnUnityThread.Invoke(info, data); m_lastReceivedData = "" + data; });
            }
        }
    }


    public void SendIntegerLittleEndian(int value)
    {
        if (IsUsable())
            m_portConnection.SendIntegerLittleEndian(value);
    }
    public void SendIntegerAsTextWithLineReturn(int value)
    {
        if (IsUsable())
            m_portConnection.SendIntegerAsTextWithLineReturn(value);
    }

    public void SendDataAsTextCharArray(string text)
    {
        if (IsUsable())
            m_portConnection.SendDataAsTextCharArray(text);
    }
    public void SendDataAsBytes(byte[] bytes)
    {
        if (IsUsable())
            m_portConnection.SendDataAsBytes(bytes);
    }
    public void SendDataAsByte(byte byteData)
    {
        if (IsUsable())
            m_portConnection.SendDataAsByte(byteData);
    }
    public bool IsUsable()
    {
        if (m_portConnection != null && this.gameObject.activeInHierarchy)
            return m_portConnection.IsUsable();
        return false;
    }
    public void Update()
    {
        if (IsUsable())
        {
            m_portConnection.FlushQueueDataWaitinginThread();
        }
    
    }

}
    [System.Serializable]
    public class ComThreadPortConnection
    {
        public static List<ComThreadPortConnection> m_threadCreated = new List<ComThreadPortConnection>();

        public static void KillAllThread()
        {
            foreach (ComThreadPortConnection item in m_threadCreated)
            {
                try {

                        if (item != null) { 
                                item.KillCurrentConnection();
                        }
                    }
                catch (System.Exception e)
                {
                    Debug.LogError("Error while trying to kill thread : " + e.Message);
                }
            }
            m_threadCreated.Clear();
        }
    public ComThreadPortConnection(ComPortToInfo port, out KillSwitch killSwith, int baudRare = 9600, PortNameListenType listenType = PortNameListenType.TextUTF8) {

        Add(this);
        m_port = port;
        m_baudRate = baudRare;
        m_listenType = listenType;
        killSwith = m_killSwitch;
        StartComConnectionFromPortName(port.GetComPortId());

    }


    private static void Add(ComThreadPortConnection comThreadPortConnection)
    {
        CancelThreadPortConnectionMono. CreateInstanceIfNoneInScene();
        m_threadCreated.Add(comThreadPortConnection);
    }

    public ComPortToInfo m_port; 
    public int m_baudRate = 9600;
    public PortNameListenType m_listenType = PortNameListenType.TextUTF8;


    public UnityEvent<string> m_onReceivedMessageOnUnityThread= new UnityEvent<string>();
    public UnityEvent<byte> m_onReceivedByteOnUnityThread= new UnityEvent<byte>();
    public UnityEvent<byte[]> m_onReceivedBytesGroupOnUnityThread= new UnityEvent<byte[]>();

    private SerialPort m_serialPort;
    private Thread readThread;
    private bool isRunning = false;
    private string receivedData = "";

    public Queue<string> m_receivedDataQueue = new Queue<string>();

    public Queue<byte[]> m_receivedBytesQueue = new Queue<byte[]>();

    public Queue<byte> m_receivedByteQueue = new Queue<byte>();

    public KillSwitch m_killSwitch = new KillSwitch();

    [System.Serializable]
    public class KillSwitch { 
        public bool m_requestToDie = false;
    }



    private void StartComConnectionFromPortName(string comName)
    {
        for (int i = 0; i < 3; i++) { 
        
            try
            {
                m_serialPort = new SerialPort(comName, m_baudRate);
                m_serialPort.Open();
                readThread = new Thread(() => ReadData(m_killSwitch));
                readThread.Name = comName;
                readThread.Start();
                return;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error while trying to start connection (attempt:{i}) : {e.Message} ");
            }
        }
    }

    public void FlushQueueDataWaitinginThread()
    {
        if (!IsUsable())
            return;
        if (m_receivedDataQueue.Count > 0)
        {
            string data = m_receivedDataQueue.Dequeue();
            m_onReceivedMessageOnUnityThread.Invoke(data);
        }
        if (m_receivedByteQueue.Count > 0)
        {
            byte data = m_receivedByteQueue.Dequeue();
            m_onReceivedByteOnUnityThread.Invoke(data);
        }
        if (m_receivedBytesQueue.Count > 0)
        {
            byte[] data = m_receivedBytesQueue.Dequeue();
            m_onReceivedBytesGroupOnUnityThread.Invoke(data);
        }
    }

    private void ReadData(KillSwitch killSwitch)
    {
        while ( m_serialPort.IsOpen)
        {
            if (ShouldDie())
                return;
           
                if (m_listenType == PortNameListenType.TextUTF8)
                {
                    string data = m_serialPort.ReadLine();
                    if (ShouldDie())
                        break;
                    if (!string.IsNullOrEmpty(data))
                    {
                        receivedData = data;
                        if (receivedData!=null && m_receivedDataQueue != null)
                            m_receivedDataQueue.Enqueue(data);
                    }
                }
                else if (m_listenType == PortNameListenType.ByteByByte)
                {
                    byte data = (byte)m_serialPort.ReadByte();
                    if (ShouldDie())
                        break;
                    if (m_receivedByteQueue != null)
                    m_receivedByteQueue.Enqueue(data);
                }
                else if (m_listenType == PortNameListenType.BytesGroup)
                {
                    if (ShouldDie())
                        break;
                    byte[] data = new byte[m_serialPort.BytesToRead];

                    if (m_serialPort!=null) { 
                        m_serialPort.Read(data, 0, data.Length);
                        if (m_receivedBytesQueue != null) { 
                            m_receivedBytesQueue.Enqueue(data);
                        }
                    }
                }
        }
        KillCurrentConnection();
    }

    private bool ShouldDie()
    {
        
        #if UNITY_EDITOR
            return m_killSwitch.m_requestToDie  ;
            // Check if editor is running from a thread
            // ADD CODE HERE


        #else
            return m_killSwitch.m_requestToDie;
        #endif
    }

    public void SendDataAsTextCharArray(string text)
    {
        Debug.Log("SendDataAsTextCharArray : " + text);
        if (IsUsable())
        {
            string dataToSend = text;
            byte [] data = new byte[dataToSend.Length];
            for (int i = 0; i < dataToSend.Length; i++)
            {
                data[i] = (byte)dataToSend[i];
            }
            m_serialPort.Write(data, 0, data.Length);
        }
    }
    public void SendDataAsTextWithLineReturn(string text)
    {
        if (IsUsable())
        {
            string dataToSend = text + "\n";
            m_serialPort.WriteLine(dataToSend);
        }
    }
    public void SendDataAsBytes(byte[] bytes)
    {
        if (IsUsable())
        {
            m_serialPort.Write(bytes,0, bytes.Length);
        }
    }
    public void SendDataAsByte(byte byteData)
    {
        if (IsUsable())
        {
            m_serialPort.Write(new byte[] { byteData }, 0, 1);
        }
    }


    public void KillCurrentConnection()
    {
        m_killSwitch.m_requestToDie = true;


        if (readThread != null && readThread.IsAlive)
        {
            readThread.Abort();
        }

        if (m_serialPort != null && m_serialPort.IsOpen)
        {
            m_serialPort.Close();
        }
    }

    public bool IsUsable()
    {
        return m_serialPort != null && m_serialPort.IsOpen && !m_killSwitch.m_requestToDie;
    }
    public bool IsNotUsable()
    {
        return !IsUsable();
    }
    public void SendIntegerLittleEndian(int value)
    {
        if (IsUsable())
        {
            byte[] data = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);
            m_serialPort.Write(data, 0, data.Length);
        }
    }

    public void SendIntegerAsTextWithLineReturn(int value)
    {
        string data = value.ToString() + "\n";
        SendDataAsTextCharArray(data);
    }
}
