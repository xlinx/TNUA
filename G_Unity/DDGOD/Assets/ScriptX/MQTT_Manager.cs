using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using UnityEngine;
using UnityEngine.UI;

public class MQTT_Manager : MonoBehaviour
{
    [SerializeField] private InputField ip;
    [SerializeField] private InputField port;
    [SerializeField] private InputField subscribe_chanel;
    [SerializeField] private InputField publish_chanel;
    [SerializeField] private InputField publish_content;
    [SerializeField] private Button connect_btn;
    [SerializeField] private Button subscribe_btn;

    [SerializeField] private Button publish_btn;
    [SerializeField] private Text receive_Message_text;
    private MqttClient client;

    private string receive_message;
    // Use this for initialization
    void Start () {
        //連接按鈕監聽事件
		connect_btn.onClick.AddListener(() =>
		{
		    string txtIP = ip.text;
		    string txtPort = port.text;
		    string clientId = Guid.NewGuid().ToString();
            //服務器默認密碼是這個
		    string username = "admin";
		    string password = "password";
		    client = new MqttClient(IPAddress.Parse(txtIP), int.Parse(txtPort), false, null);

            client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
            client.MqttMsgSubscribed += Client_MqttMsgSubscribed; ;
		    client.Connect(clientId, username, password);

		});
        //訂閱按鈕監聽事件
        subscribe_btn.onClick.AddListener(() =>
        {
            if (client != null&&subscribe_chanel.text!="")
            {
                client.Subscribe(new string[] { subscribe_chanel.text }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            }
        });

        //發佈按鈕監聽事件
        publish_btn.onClick.AddListener(() =>
        {
            if (client != null && publish_chanel.text != "")
            {
                client.Publish(publish_chanel.text, System.Text.Encoding.UTF8.GetBytes(publish_content.text), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            }
        });

    }

    private void Client_MqttMsgSubscribed(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgSubscribedEventArgs e)
    {
        Debug.Log("訂閱" + e.MessageId);
    }

    private void Client_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
    {
        string message=System.Text.Encoding.UTF8.GetString(e.Message);
        receive_message = message;
        Debug.Log("接收到消息是"+message);
    }

    // Update is called once per frame
    void Update ()
    {
        receive_Message_text.text = receive_message;
    }
}