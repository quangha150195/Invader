using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace external
{

    public class NativeListener : Singleton<NativeListener>
    {
        public interface INativeListener
        {
            void nativeCallBack(string message);
        }
        public delegate void nativeEventCallBack(string message);
        private List<INativeListener> mEventDictionary = new List<INativeListener>();
        public const string kGameObjectName = "[NativeListener]";

        public void Start()
        {
            gameObject.name = kGameObjectName;
        }
        public void receiveMessage(string message)
        {
            for(int i=mEventDictionary.Count-1;i>=0;i--)
            {
                INativeListener listener = mEventDictionary[i];
                if (listener != null)
                {
                    listener.nativeCallBack(message);
                }
                else
                {
                    mEventDictionary.RemoveAt(i);
                }
            }
        }
        public void registerListener(INativeListener listener)
        {
            mEventDictionary.Add(listener);
        }
        public void unRegisterListener(INativeListener listener)
        {
            mEventDictionary.Remove(listener);
        }
    }
}