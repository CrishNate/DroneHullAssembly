using System;
using Unity.MLAgents.SideChannels;
using UnityEngine;

public class ValidDesignSideChannel : SideChannel
{
    public ValidDesignSideChannel()
    {
        ChannelId = new Guid("621f0a70-4f87-11ea-a6bf-784f4387d1f7");
    }

    protected override void OnMessageReceived(IncomingMessage msg)
    { }

    public void SendMessage(string stringToSend)
    {
        using (var msgOut = new OutgoingMessage())
        {
            msgOut.WriteString(stringToSend);
            QueueMessageToSend(msgOut);
        }
    }
    
    public void SendMessage(int value)
    {
        using (var msgOut = new OutgoingMessage())
        {
            msgOut.WriteInt32(value);
            QueueMessageToSend(msgOut);
        }
    }
    
    public void SendMessage(bool value)
    {
        using (var msgOut = new OutgoingMessage())
        {
            msgOut.WriteBoolean(value);
            QueueMessageToSend(msgOut);
        }
    }
}