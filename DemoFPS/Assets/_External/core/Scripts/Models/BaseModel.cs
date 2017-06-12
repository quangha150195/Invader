using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class BaseModel<T> : external.Singleton<T> where T : MonoBehaviour
{
    protected bool m_IsOK=false;
    protected bool m_IsFail=false;
    protected bool m_IsStop = false;
    public virtual bool IsOK
    {
        get { return m_IsOK; }
    }
    public bool IsFail
    {
        get { return m_IsFail; }
    }
    public virtual void init()
    {
        m_IsOK = false;
    }
}
