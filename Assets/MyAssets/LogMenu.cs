using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LogMenu : MonoBehaviour
{
    [SerializeField]
    private Text m_textUI = null;
    private string queuedText = "";  // 追加: キューイングするテキストを保持する変数

    private void Awake()
    {
        Application.logMessageReceived += OnLogMessage;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnLogMessage;  // 注意: '-=' を使ってイベントを外します
    }

    private void Update()
    {
        if (!string.IsNullOrEmpty(queuedText))
        {
            m_textUI.text += queuedText;
            queuedText = "";
        }
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

        // m_textUI.text += i_logText + System.Environment.NewLine;  // この行をコメントアウト
        queuedText += i_logText + System.Environment.NewLine;  // 追加: テキストをキューに追加
    }
}  // class LogMenu  //referenxe URL https://www.urablog.xyz/entry/2017/04/25/195351