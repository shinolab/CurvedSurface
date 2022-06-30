using UnityEngine;
using UnityEngine.UI;

public class LogMenu : MonoBehaviour
{
    [SerializeField]
    private Text m_textUI = null;

    private void Awake()
    {
        Application.logMessageReceived += OnLogMessage;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived += OnLogMessage;
    }

    private void OnLogMessage(string i_logText, string i_stackTrace, LogType i_type)
    {
        if (string.IsNullOrEmpty(i_logText))
        {
            return;
        }

        if (!string.IsNullOrEmpty(i_stackTrace))
        {
            switch (i_type)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    i_logText += System.Environment.NewLine + i_stackTrace;
                    break;
                default:
                    break;
            }
        }

        switch (i_type)
        {
            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception:
                i_logText = string.Format("<color=red>{0}</color>", i_logText);
                break;
            case LogType.Warning:
                i_logText = string.Format("<color=yellow>{0}</color>", i_logText);
                break;
            default:
                i_logText = string.Format("<color=black>{0}</color>", i_logText);
                break;
        }

        m_textUI.text += i_logText + System.Environment.NewLine;

    }

} // class LogMenu  //referenxe URL https://www.urablog.xyz/entry/2017/04/25/195351