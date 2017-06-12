using UnityEngine;
using System.Collections;
using System.Net;
using System;
using UnityEngine.UI;
using System.IO;
namespace external
{
    public class HttpDownloadSample : MonoBehaviour
    {
        public InputField tbURL;
        public InputField tbPath;
        public Button btnDownload;
        public Button btnPause;
        public Button btnCancel;
        public Text lbStatus;
        public Text lbSummary;

        public Image imageTest;
        public Image imageTest2;
        IDownloader downloader = null;

        DateTime lastNotificationTime;

        public string FileToDownload
        {
            get
            {
                return tbURL.text;
            }
            set
            {
                tbURL.text = value;
            }
        }

        public string DownloadPath
        {
            get
            {
                return tbPath.text;
            }
            set
            {
                tbPath.text = value;
            }
        }

        void Start()
        {
            tbPath.text = Application.persistentDataPath + "/test.zip";
        }


        public void btnDownload_Click()
        {

            try
            {

                // Check whether the file exists.
                if (File.Exists(tbPath.text.Trim()))
                {
                    Debug.Log("There is already a file with the same name, "
                             + "do you want to delete it? "
                             + "If not, please change the local path. ");
                }

                // Initialize an instance of HttpDownloadClient.
                downloader = new HttpDownloadClient(tbURL.text);

                // Register the events of HttpDownloadClient.
                downloader.DownloadCompleted += DownloadCompleted;
                downloader.DownloadProgressChanged += DownloadProgressChanged;
                downloader.StatusChanged += StatusChanged;
                downloader.DownloadPath = tbPath.text;
                // Start to download file.
                downloader.BeginDownload();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

        }
        void Update()
        {
            if (lastStatusChanged != null)
            {
                StatusChangedHanlder();
                lastStatusChanged = null;
            }
            if (lastDownloadProgressChanged != null)
            {
                DownloadProgressChangedHanlder(lastDownloadProgressChanged);
                lastDownloadProgressChanged = null;
            }
            if (lastDownloadCompleted != null)
            {
                DownloadCompletedHanlder(lastDownloadCompleted);
                lastDownloadCompleted = null;
            }
        }

        EventArgs lastStatusChanged = null;
        void StatusChanged(object sender, EventArgs e)
        {
            lastStatusChanged = e;
        }


        void StatusChangedHanlder()
        {
            // Refresh the status.
            lbStatus.text = downloader.Status.ToString();

            // Refresh the buttons.
            switch (downloader.Status)
            {
                case DownloadStatus.Waiting:
                    btnCancel.interactable = true;
                    btnDownload.interactable = false;
                    btnPause.interactable = false;
                    tbPath.interactable = false;
                    tbURL.interactable = false;
                    break;
                case DownloadStatus.Canceled:
                case DownloadStatus.Completed:
                    btnCancel.interactable = false;
                    btnDownload.interactable = true;
                    btnPause.interactable = false;
                    tbPath.interactable = true;
                    tbURL.interactable = true;
                    break;
                case DownloadStatus.Downloading:
                    btnCancel.interactable = true;
                    btnDownload.interactable = false;
                    btnPause.interactable = true & downloader.IsRangeSupported;
                    tbPath.interactable = false;
                    tbURL.interactable = false;
                    break;
                case DownloadStatus.Paused:
                    btnCancel.interactable = true;
                    btnDownload.interactable = false;

                    // The "Resume" button.
                    btnPause.interactable = true & downloader.IsRangeSupported;
                    tbPath.interactable = false;
                    tbURL.interactable = false;
                    break;
            }

            if (downloader.Status == DownloadStatus.Paused)
            {
                lbSummary.text =
                   String.Format("Received: {0}KB, Total: {1}KB, Time: {2}:{3}:{4}",
                   downloader.DownloadedSize / 1024, downloader.TotalSize / 1024,
                   downloader.TotalUsedTime.Hours, downloader.TotalUsedTime.Minutes,
                   downloader.TotalUsedTime.Seconds);

                btnPause.GetComponentInChildren<Text>().text = "Resume";
            }
            else
            {
                btnPause.GetComponentInChildren<Text>().text = "Pause";
            }


        }



        /// <summary>
        /// Handle DownloadProgressChanged event.
        /// </summary>
        WebDownloadProgressChangedEventArgs lastDownloadProgressChanged = null;
        void DownloadProgressChanged(object sender, WebDownloadProgressChangedEventArgs e)
        {
            lastDownloadProgressChanged = e;
        }
        void DownloadProgressChangedHanlder(WebDownloadProgressChangedEventArgs e)
        {
            // Refresh the summary every second.
            if (DateTime.Now > lastNotificationTime.AddSeconds(1))
            {
                lbSummary.text = String.Format("Received: {0}KB, Total: {1}KB, Speed: {2}KB/s",
                    e.ReceivedSize / 1024, e.TotalSize / 1024, e.DownloadSpeed / 1024);
                //prgDownload.Value = (int)(e.ReceivedSize * 100 / e.TotalSize);
                lastNotificationTime = DateTime.Now;
            }
        }


        WebDownloadCompletedEventArgs lastDownloadCompleted = null;
        void DownloadCompleted(object sender, WebDownloadCompletedEventArgs e)
        {
            lastDownloadCompleted = e;
        }

        void DownloadCompletedHanlder(WebDownloadCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                lbSummary.text =
                    String.Format("Received: {0}KB, Total: {1}KB, Time: {2}:{3}:{4}",
                    e.DownloadedSize / 1024, e.TotalSize / 1024, e.TotalTime.Hours,
                    e.TotalTime.Minutes, e.TotalTime.Seconds);
                //prgDownload.Value = 100;
            }
            else
            {
                lbSummary.text = e.Error.Message;
                Debug.LogError(e.Error.Message);
                //prgDownload.Value = 0;
            }
        }

        public void btnCancel_Click()
        {
            if (downloader != null)
            {
                downloader.Cancel();
            }

        }

        public void btnPause_Click()
        {
            if (downloader.Status == DownloadStatus.Paused)
            {
                downloader.BeginResume();
            }
            else if (downloader.Status == DownloadStatus.Downloading)
            {
                downloader.Pause();
            }
        }

        public void btnDownloadWWW()
        {
            StartCoroutine(downloadWWW());
        }
        private IEnumerator downloadWWW()
        {
            WWW www = new WWW(tbURL.text);
            while (!www.isDone)
            {
                //mPercent = prePercent + percent * www.progress;
                Debug.Log(www.progress);
                yield return null;
            }
            if (!string.IsNullOrEmpty(www.error))
            {
                 Debug.LogError(www.error);
            }
            else
            {
                using (FileStream file = new FileStream(tbPath.text, FileMode.Create))
                {
                    file.Write(www.bytes, 0, www.bytesDownloaded);
                }
                // mPercent = prePercent + percent;
                imageTest2.sprite = Sprite.Create(www.texture,new Rect(0,0,www.texture.width,www.texture.height),Vector2.zero);
                yield break;
            }
        }
    }
}