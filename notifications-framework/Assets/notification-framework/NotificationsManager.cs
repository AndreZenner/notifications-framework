using System;
using UnityEngine;
using UnityEngine.Events;

/*
 * The Immersive Notification Framework
 * 
 * ... a simple & open framework to include immersive and adaptive notifications in your Unity application.
 * 
 * Authors: Sören Klingner (DFKI), André Zenner (DFKI)
 * /


/**
 * The context function that needs to be implemented by the developer
 **/
[Serializable]
public abstract class ContextFunction : MonoBehaviour
{
    /**
     * Returns the context rating c between 0 and 1 according to the user's context / situation / state in the virtual environment
     **/
    public abstract float Evaluate();
}

[Serializable]
public class NotificationEvent : UnityEvent<string> {}

/**
 * A notification level.
 **/
[Serializable]
public struct NotificationLevel
{
    // method types, which indicates how the event of the same level is choosen
    public enum SelectionMethod
    {
        random,
        sequence
    }

    [Header("It defines which event will be choosen out of the list")]
    public SelectionMethod selectionMethod;

    [Header("You can add multiple different events to the same notification level")]
    public NotificationEvent[] notificationEvents;

    private int lastIndex;

    // fires an event of a priority depending on a given selection method
    public void FireEvent(string message)
    {
        int index = 0;

        switch (selectionMethod)
        {
            case SelectionMethod.random:
                index = notificationEvents.Length * UnityEngine.Random.Range(0, 1);
                break;
            case SelectionMethod.sequence:
                lastIndex++;
                if (lastIndex >= notificationEvents.Length) lastIndex = 0;
                index = lastIndex;
                break;
            default:
                break;
        }
        notificationEvents[index].Invoke(message);
    }
}

/**
 * Main class of the Immersive Notification Framework
 **/
[RequireComponent(typeof(UDPReceive))]
public class NotificationsManager : MonoBehaviour
{
    [Header("Link your implementation of the Context Function here")]
    public ContextFunction contextFunction;

    [Header("Set the size of the list to the number of different levels of notification")]
    public NotificationLevel[] notifications;     //the length of this array represents the amount of levels of notification N

    //for receiving messages
    private UDPReceive _udpListener;
    private string _lastMessage = ";0";

    private void Start()
    {
        _udpListener = GetComponent<UDPReceive>();
    }

    // check for incomming messages
    void LateUpdate()
    {
        // get the latest message from the smartphone
        string _str = _udpListener.getLatestUDPPacket();
        // strA[0] is the message, strA[1] is the priority p (0 = low; 1 = high)
        string[] _strA = _str.Split(';');

        // if a new message comes in fire an event
        if (_lastMessage != _str && _strA.Length == 2)
        {
            Debug.Log("New Message (priority p = " + _strA[1] + ") : \"" + _strA[0] + "\"");
            _lastMessage = _str;

            ProcessIncomingNotification(_strA[0], float.Parse(_strA[1]));
        }
    }

    /**
     * paramter priority (p) - value between 0 (= low priority) and 1 (= high priority)
     * 
     * Processes an incoming message. 
     * Adaptively chooses a notification level to trigger.
     * Takes into account the user's context and the notification priority.
     **/
    private void ProcessIncomingNotification(string message, float p)
    {
        p = Mathf.Clamp(p, 0, 1);                   //message priority p
        float c = contextFunction.Evaluate();       //context rating c
        int N = notifications.Length;               //number of notification levels N

        float r = ComputeNotificationRating(p, c);  //compute notification rating r - adapt the way the message is presented to the user's context

        int NotificationLevelIndex = Mathf.FloorToInt(r * N);   //find notification level to trigger from
        if (NotificationLevelIndex == N)
            NotificationLevelIndex = N - 1;

        notifications[NotificationLevelIndex].FireEvent(message);   //trigger notification
    }

    /*
     * Implements the computation of the final notification rating r in [0,1]
     * */
    protected virtual float ComputeNotificationRating(float p, float c)
    {
        //default implementation
        //you can implement alternative decision making / adaptation patterns here

        // At the moment the average is taken to determine the final index 
        float r = (p + c) / 2f;     //computes the final notification rating r
        return r;
    }
}
