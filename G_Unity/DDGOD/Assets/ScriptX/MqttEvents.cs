using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;


public class MqttEvents : MonoBehaviour
{

    // mqtt://xtnuax:pbmh7GQctfJmrxPE@xtnuax.cloud.shiftr.io
    public MqttClient client;
    [Header("服务器设定")]
    public bool debug = true;
    public string ip = "xtnuax.cloud.shiftr.io";
    public int port = 1883;
    public string username = "xtnuax";
    public string password = "pbmh7GQctfJmrxPE";
    public bool autoConnect = true;
    public bool autoReconnect = true;
    [Header("自动订阅主题")]
    public int qos = 0;
    public string[] toppics;
    [Header("MQTT事件绑定")]
    public MqttEvent_OnBegin onBeginCall;
    public MqttEvent_OnConnect onConnectCall;
    public MqttEvent_OnReonnect onReconnectCall;
    public MqttEvent_OnDisconnect onDisconnectCall;
    public MqttEvent_OnMsg onMsgCall;
    private List<MqttMsgData> _MqttMsgDatas = new List<MqttMsgData>();
    private string GetValidatedIP(string ipStr)
    {
        string validatedIP = string.Empty;
        IPAddress ip;
        if (IPAddress.TryParse(ipStr, out ip))
        {
            validatedIP = ip.ToString();
        }
        return validatedIP;
    }
    public static IPAddress[] GetIPsByName(string hostName, bool ip4Wanted, bool ip6Wanted)
    {
        // Check if the hostname is already an IPAddress
        IPAddress outIpAddress;
        if (IPAddress.TryParse(hostName, out outIpAddress) == true)
            return new IPAddress[] { outIpAddress };
        //<----------

        IPAddress[] addresslist = Dns.GetHostAddresses(hostName);

        if (addresslist == null || addresslist.Length == 0)
            return new IPAddress[0];
        //<----------

        if (ip4Wanted && ip6Wanted)
            return addresslist;
        //<----------

        if (ip4Wanted)
            return addresslist.Where(o => o.AddressFamily == AddressFamily.InterNetwork).ToArray();
        //<----------

        if (ip6Wanted)
            return addresslist.Where(o => o.AddressFamily == AddressFamily.InterNetworkV6).ToArray();
        //<----------

        return new IPAddress[0];
    }
    // Use this for initialization
    void Awake()
    {
        // Thread t = new Thread(initMQTT);
        // t.Start();
        initMQTT();
    }
    void initMQTT()
    {

        // create client instance 
        
        client = new MqttClient(GetIPsByName(ip,true,false)[0], port, false, null);

        // register to message received 
        client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
        client.MqttMsgDisconnected += client_MqttMsgDisconnected;
        /*
        //这部分事件不常用,按需添加
        client.MqttMsgSubscribed += client_MqttMsgSubscribed;
        client.MqttMsgUnsubscribed += client_MqttMsgUnsubscribed;
        client.MqttMsgPublished += client_MqttMsgPublished;*/
        onBeginCall.Invoke(this);
        if (autoConnect) connect();

    }
    public void connect()
    {

        string clientId = "CONTROL_CLIENT_" + Guid.NewGuid().ToString();
        if (debug) client.Connect(clientId, username, password);
        if (client.IsConnected)
        {
            if (debug) Debug.Log("[MQTT]连接服务器成功!" + client.IsConnected);
            onConnectCall.Invoke(client);
            //开始订阅主题
            if (toppics.Length > 0)
            {
                byte[] qoss = new byte[toppics.Length];
                for (int i = 0; i < qoss.Length; i++) qoss[i] = (byte)qos;
                client.Subscribe(toppics, qoss);
            }

        }
        else
        {
            if (debug) Debug.LogWarning("[MQTT]连接服务器失败!" + client.IsConnected);
            onDisconnectCall.Invoke(client);
            if (autoReconnect) StartCoroutine(reConnect());
        }
        float ts = Time.time;
    }


    void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string msg = Encoding.UTF8.GetString(e.Message);
        if (debug) Debug.Log("[MQTT]消息抵达: " + e.Topic + " " + msg);
        //这个事件是多线程调用的,我们需要把它转到Update里面让主线程去运行,不然在进行调用资源的时候unity会报错.
        MqttMsgData args = new MqttMsgData();
        args.Message = msg;
        args.Topic = e.Topic;
        _MqttMsgDatas.Add(args);



    }
    void client_MqttMsgDisconnected(object sender, System.EventArgs e)
    {
        if (debug) Debug.Log("[MQTT]服务器连接中断..." + e);
        onDisconnectCall.Invoke(client);
        if (autoReconnect) StartCoroutine(reConnect());
    }
    IEnumerator reConnect()
    {
        yield return new WaitForSeconds(1);
        if (debug) Debug.Log("[MQTT]正在重新连接服务器...");
        onReconnectCall.Invoke(client);
        connect();
    }
    /*
    void client_MqttMsgSubscribed(object sender, System.EventArgs e)
    {
        if (debug) Debug.Log("[MQTT]成功订阅主题: " + e);
    }
    void client_MqttMsgUnsubscribed(object sender, System.EventArgs e)
    {
        if (debug) Debug.Log("[MQTT]取消订阅成功: " + e);
    }
    void client_MqttMsgPublished(object sender, System.EventArgs e)
    {
        if (debug) Debug.Log("[MQTT]发送消息成功: " + e);
    }
        client.Publish(topic, System.Text.Encoding.UTF8.GetBytes(str), qos, retain);
    }*/
    public void send(string topic, string str)
    {
        client.Publish(topic, System.Text.Encoding.UTF8.GetBytes(str), (byte)qos, false);
    }

    //监听断开事件不激活,不知道怎么地了
    //这里用Update监控模拟断开事件
    private bool lastConnected = false;
    void Update()
    {

        //Debug.Log(client.IsConnected);
        if (client.IsConnected != lastConnected)
        {
            lastConnected = client.IsConnected;
            if (!lastConnected)
            {
                onDisconnectCall.Invoke(client);
            }

        }

        //将其他线程转换到主线程激活,不然unity会报错
        while (_MqttMsgDatas.Count > 0)
        {
            MqttMsgData args = _MqttMsgDatas[0];
            _MqttMsgDatas.RemoveAt(0);
            onMsgCall.Invoke(this, args.Topic, args.Message);
        }
    }

}

[Serializable]
public class MqttEvent_OnBegin : UnityEvent<MqttEvents> { }


[Serializable]
public class MqttEvent_OnMsg : UnityEvent<MqttEvents, string, string> { }

[Serializable]
public class MqttEvent_OnDisconnect : UnityEvent<MqttClient> { }

[Serializable]
public class MqttEvent_OnConnect : UnityEvent<MqttClient> { }
[Serializable]
public class MqttEvent_OnReonnect : UnityEvent<MqttClient> { }

struct MqttMsgData
{
    public string Topic { get; set; }
    public string Message { get; set; }
}
