using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace external
{
    public static class DownloaderHelper
    {
        public static HttpWebRequest InitializeHttpWebRequest(IDownloader downloader)
        {
            ServicePointManager.ServerCertificateValidationCallback = (o, certificate, chain, errors) => true;
            var webRequest = (HttpWebRequest)WebRequest.Create(downloader.Url);

            if (downloader.Credentials != null)
            {
                webRequest.Credentials = downloader.Credentials;
            }
            else
            {
                webRequest.Credentials = CredentialCache.DefaultCredentials;
            }

            if (downloader.Proxy != null)
            {
                webRequest.Proxy = downloader.Proxy;
            }
            else
            {
                webRequest.Proxy = WebRequest.DefaultWebProxy;
            }

            return webRequest;
        }
    }
    public enum DownloadStatus
    {
        Initialized,
        Waiting,
        Downloading,
        Pausing,
        Paused,
        Canceling,
        Canceled,
        Completed
    }
    public interface IDownloader
    {
        #region Basic settings of a WebDownloader.

        Uri Url { get; }
        string DownloadPath { get; set; }
        string DownloadPathTemp { get; set; }
        long TotalSize { get; set; }

        ICredentials Credentials { get; set; }
        IWebProxy Proxy { get; set; }

        #endregion


        #region Support the "Pause", "Resume" and Multi-Threads feature.

        bool IsRangeSupported { get; set; }
        int StartPoint { get; set; }
        int EndPoint { get; set; }

        #endregion

        #region The downloaded data and status.

        int DownloadedSize { get; }
        int CachedSize { get; }

        bool HasChecked { get; set; }
        DownloadStatus Status { get; }
        TimeSpan TotalUsedTime { get; }

        #endregion

        #region Advanced settings of a WebDownloader

        int BufferSize { get; set; }
        int BufferCountPerNotification { get; set; }
        int MaxCacheSize { get; set; }

        #endregion


        event EventHandler<WebDownloadCompletedEventArgs> DownloadCompleted;
        event EventHandler<WebDownloadProgressChangedEventArgs> DownloadProgressChanged;
        event EventHandler StatusChanged;

        void CheckUrl();

        void BeginDownload();
        void Download();

        void Pause();

        void Resume();
        void BeginResume();

        void Cancel();
    }
    public class WebDownloadCompletedEventArgs : EventArgs
    {
        public Int64 DownloadedSize { get; private set; }
        public Int64 TotalSize { get; private set; }
        public Exception Error { get; private set; }
        public TimeSpan TotalTime { get; private set; }
        public FileInfo DownloadedFile { get; private set; }

        public WebDownloadCompletedEventArgs(
            FileInfo downloadedFile, Int64 downloadedSize,
            Int64 totalSize, TimeSpan totalTime, Exception ex)
        {
            this.DownloadedFile = downloadedFile;
            this.DownloadedSize = downloadedSize;
            this.TotalSize = totalSize;
            this.TotalTime = totalTime;
            this.Error = ex;
        }
    }
    public class WebDownloadProgressChangedEventArgs : EventArgs
    {
        public Int64 ReceivedSize { get; private set; }
        public Int64 TotalSize { get; private set; }
        public int DownloadSpeed { get; private set; }

        public WebDownloadProgressChangedEventArgs(Int64 receivedSize,
            Int64 totalSize, int downloadSpeed)
        {
            this.ReceivedSize = receivedSize;
            this.TotalSize = totalSize;
            this.DownloadSpeed = downloadSpeed;
        }
    }
    public class HttpDownloadClient : IDownloader
    {
        // Used when creates or writes a file.
        static object fileLocker = new object();

        object statusLocker = new object();


        // The Url of the file to be downloaded.
        public Uri Url { get; private set; }

        // The local path to store the file.
        // If there is no file with the same name, a new file will be created.
        public string DownloadPath { get; set; }
        public string DownloadPathTemp { get; set; }

        // Ask the server for the file size and store it.
        // Use the CheckUrl method to initialize this property internally.
        public long TotalSize { get; set; }
        public DateTime LastModified { get; set; }

        public ICredentials Credentials { get; set; }

        public IWebProxy Proxy { get; set; }

        /// <summary>
        /// Specify whether the remote server supports "Accept-Ranges" header.
        /// Use the CheckUrl method to initialize this property internally.
        /// </summary>
        public bool IsRangeSupported { get; set; }

        // The properties StartPoint and EndPoint can be used in the multi-thread download scenario, and
        // every thread starts to download a specific block of the whole file. 
        public int StartPoint { get; set; }

        public int EndPoint { get; set; }


        // The size of downloaded data that has been writen to local file.
        public int DownloadedSize { get; private set; }

        public int CachedSize { get; private set; }

        public bool HasChecked { get; set; }


        DownloadStatus status;

        // If status changed, fire StatusChanged event.
        public DownloadStatus Status
        {
            get
            {
                return status;
            }
            private set
            {
                lock (statusLocker)
                {
                    if (status != value)
                    {
                        status = value;
                        this.OnStatusChanged(EventArgs.Empty);
                    }
                }
            }
        }

        // Store the used time spent in downloading data. The value does not include
        // the paused time and it will only be updated when the download is paused, 
        // canceled or completed.
        private TimeSpan usedTime = new TimeSpan();

        private DateTime lastStartTime;

        /// <summary>
        /// If the status is Downloading, then the total time is usedTime. Else the total 
        /// should include the time used in current download thread.
        /// </summary>
        public TimeSpan TotalUsedTime
        {
            get
            {
                if (this.Status != DownloadStatus.Downloading)
                {
                    return usedTime;
                }
                else
                {
                    return usedTime.Add(DateTime.Now - lastStartTime);
                }
            }
        }

        // The time and size in last DownloadProgressChanged event. These two fields
        // are used to calculate the download speed.
        private DateTime lastNotificationTime;
        private Int64 lastNotificationDownloadedSize;

        // If get a number of buffers, then fire DownloadProgressChanged event.
        public int BufferCountPerNotification { get; set; }

        // Set the BufferSize when read data in Response Stream.
        public int BufferSize { get; set; }

        // The cache size in memory.
        public int MaxCacheSize { get; set; }

        public event EventHandler<WebDownloadProgressChangedEventArgs> DownloadProgressChanged;

        public event EventHandler<WebDownloadCompletedEventArgs> DownloadCompleted;

        public event EventHandler StatusChanged;

        /// <summary>
        /// Download the whole file.
        /// </summary>
        public HttpDownloadClient(string url)
            : this(url, 0)
        {
        }

        /// <summary>
        /// Download the file from a start point to the end.
        /// </summary>
        public HttpDownloadClient(string url, int startPoint)
            : this(url, startPoint, int.MaxValue)
        {
        }

        /// <summary>
        /// Download a block of the file. The default buffer size is 1KB, memory cache is
        /// 1MB, and buffer count per notification is 64.
        /// </summary>
        public HttpDownloadClient(string url, int startPoint, int endPoint)
            : this(url, startPoint, endPoint, 1048576, 1048576, 64)
            //: this(url, startPoint, endPoint, 1024, 1048576, 64)
        {
        }

        public HttpDownloadClient(string url, int startPoint,
            int endPoint, int bufferSize, int cacheSize, int bufferCountPerNotification)
        {

            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
            this.BufferSize = bufferSize;
            this.MaxCacheSize = cacheSize;
            this.BufferCountPerNotification = bufferCountPerNotification;

            this.Url = new Uri(url, UriKind.Absolute);

            // Set the default value of IsRangeSupported.
            this.IsRangeSupported = true;

            // Set the Initialized status.
            this.status = DownloadStatus.Initialized;
        }



        /// <summary>
        /// Check the Uri to find its size, and whether it supports "Pause". 
        /// </summary>
        public void CheckUrl()
        {
            // Check the file information on the remote server.
            var webRequest =   DownloaderHelper.InitializeHttpWebRequest(this);

            using (var response = webRequest.GetResponse())
            {
                foreach (var header in response.Headers.AllKeys)
                {
                    if (header.Equals("Accept-Ranges", StringComparison.OrdinalIgnoreCase))
                    {
                        IsRangeSupported = true;
                    }
                }
                TotalSize = response.ContentLength;
                LastModified = ((HttpWebResponse)response).LastModified;

                StartPoint = 0;
                EndPoint = int.MaxValue;
            }

        }


        void CheckFileOrCreateFile()
        {
            // Lock other threads or processes to prevent from creating the file.
            lock (fileLocker)
            {
                FileInfo fileTemp = new FileInfo(DownloadPathTemp);

                if (fileTemp.Exists)
                {
                    if (IsRangeSupported && fileTemp.CreationTime == LastModified)
                    {
                        DownloadedSize = (int)fileTemp.Length;
                    }
                    else
                    {
                        DownloadedSize = 0;
                        File.Delete(DownloadPathTemp);
                        FileStream file = File.Create(DownloadPathTemp);
                        file.Close();
                    }
                }
                else
                {
                    DownloadedSize = 0;
                    FileStream file = File.Create(DownloadPathTemp);
                    file.Close();
                }
                fileTemp = new FileInfo(DownloadPathTemp);
                fileTemp.CreationTime = LastModified;
            }
        }

        void CheckUrlAndFile()
        {
            CheckUrl();
            CheckFileOrCreateFile();

            this.HasChecked = true;
        }

        void EnsurePropertyValid()
        {
            if (this.StartPoint < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "StartPoint cannot be less then 0. ");
            }

            if (this.EndPoint < this.StartPoint)
            {
                throw new ArgumentOutOfRangeException(
                    "EndPoint cannot be less then StartPoint ");
            }

            if (this.BufferSize < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "BufferSize cannot be less then 0. ");
            }

            if (this.MaxCacheSize < this.BufferSize)
            {
                throw new ArgumentOutOfRangeException(
                    "MaxCacheSize cannot be less then BufferSize ");
            }

            if (this.BufferCountPerNotification <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "BufferCountPerNotification cannot be less then 0. ");
            }
        }


        /// <summary>
        /// Start to download.
        /// </summary>
        public void Download()
        {

            // Only idle download client can be started.
            if (this.Status != DownloadStatus.Initialized)
            {
                throw new ApplicationException("Only Initialized download client can be started.");
            }

            this.Status = DownloadStatus.Waiting;

            // Start to download in the same thread.
            DownloadInternal(null);
        }


        /// <summary>
        /// Start to download using ThreadPool.
        /// </summary>
        public void BeginDownload()
        {

            // Only idle download client can be started.
            if (this.Status != DownloadStatus.Initialized)
            {
                throw new ApplicationException("Only Initialized download client can be started.");
            }

            this.Status = DownloadStatus.Waiting;

            ThreadPool.QueueUserWorkItem(DownloadInternal);
        }

        /// <summary>
        /// Pause the download.
        /// </summary>
        public void Pause()
        {
            // Only idle or downloading client can be paused.
            switch (this.Status)
            {
                case DownloadStatus.Downloading:
                    this.Status = DownloadStatus.Pausing;
                    break;
                default:
                    throw new ApplicationException("Only downloading client can be paused.");
            }
        }

        /// <summary>
        /// Resume the download.
        /// </summary>
        public void Resume()
        {
            // Only paused client can be resumed.
            if (this.Status != DownloadStatus.Paused)
            {
                throw new ApplicationException("Only paused client can be resumed.");
            }

            this.Status = DownloadStatus.Waiting;

            // Start to download in the same thread.
            DownloadInternal(null);
        }

        /// <summary>
        /// Resume the download using ThreadPool.
        /// </summary>
        public void BeginResume()
        {
            // Only paused client can be resumed.
            if (this.Status != DownloadStatus.Paused)
            {
                throw new ApplicationException("Only paused client can be resumed.");
            }
            if (string.IsNullOrEmpty(this.DownloadPath))
            {
                throw new ApplicationException("not set DownloadPath yet.");
            }

            this.Status = DownloadStatus.Waiting;

            ThreadPool.QueueUserWorkItem(DownloadInternal);
        }

        /// <summary>
        /// Cancel the download.
        /// </summary>
        public void Cancel()
        {
            if (this.Status == DownloadStatus.Initialized
                || this.Status == DownloadStatus.Waiting
                || this.Status == DownloadStatus.Completed
                || this.Status == DownloadStatus.Paused
                || this.Status == DownloadStatus.Canceled)
            {
                this.Status = DownloadStatus.Canceled;
            }
            else if (this.Status == DownloadStatus.Canceling
                || this.Status == DownloadStatus.Pausing
                || this.Status == DownloadStatus.Downloading)
            {
                this.Status = DownloadStatus.Canceling;
            }
        }


        /// <summary>
        /// Download the data using HttpWebRequest. It will read a buffer of bytes from the
        /// response stream, and store the buffer to a MemoryStream cache first.
        /// If the cache is full, or the download is paused, canceled or completed, write
        /// the data in cache to local file.
        /// </summary>
        void DownloadInternal(object obj)
        {
            if(string.IsNullOrEmpty(DownloadPathTemp))
            {
                DownloadPathTemp = DownloadPath + ".tmp";
            }
            if (this.Status != DownloadStatus.Waiting)
            {
                return;
            }

            HttpWebRequest webRequest = null;
            HttpWebResponse webResponse = null;
            Stream responseStream = null;
            MemoryStream downloadCache = null;
            this.lastStartTime = DateTime.Now;

            try
            {

                if (!HasChecked)
                {
                    CheckUrlAndFile();
                }

                this.EnsurePropertyValid();

                // Set the status.
                this.Status = DownloadStatus.Downloading;

                // Create a request to the file to be  downloaded.
                webRequest = DownloaderHelper.InitializeHttpWebRequest(this);

                // Specify the block to download.
                if (EndPoint != int.MaxValue)
                {
                    webRequest.AddRange((int)(StartPoint + DownloadedSize), (int)EndPoint);
                }
                else
                {
                    webRequest.AddRange((int)(StartPoint + DownloadedSize));
                }

                // Retrieve the response from the server and get the response stream.
                webResponse = (HttpWebResponse)webRequest.GetResponse();

                responseStream = webResponse.GetResponseStream();


                // Cache data in memory.
                downloadCache = new MemoryStream(this.MaxCacheSize);

                byte[] downloadBuffer = new byte[this.BufferSize];

                int bytesSize = 0;
                CachedSize = 0;
                int receivedBufferCount = 0;

                // Download the file until the download is paused, canceled or completed.
                while (true)
                {

                    // Read a buffer of data from the stream.
                    bytesSize = responseStream.Read(downloadBuffer, 0, downloadBuffer.Length);

                    // If the cache is full, or the download is paused, canceled or 
                    // completed, write the data in cache to local file.
                    if (this.Status != DownloadStatus.Downloading
                        || bytesSize == 0
                        || this.MaxCacheSize < CachedSize + bytesSize)
                    {

                        try
                        {
                            // Write the data in cache to local file.
                            WriteCacheToFile(downloadCache, CachedSize);

                            this.DownloadedSize += CachedSize;

                            // Stop downloading the file if the download is paused, 
                            // canceled or completed. 
                            if (this.Status != DownloadStatus.Downloading
                                || bytesSize == 0)
                            {
                                break;
                            }

                            // Reset cache.
                            downloadCache.Seek(0, SeekOrigin.Begin);
                            CachedSize = 0;
                        }
                        catch (Exception ex)
                        {
                            // Fire the DownloadCompleted event with the error.
                            this.OnDownloadCompleted(
                                new WebDownloadCompletedEventArgs(
                                    null,
                                    this.DownloadedSize,
                                    this.TotalSize,
                                    this.TotalUsedTime,
                                    ex));
                            return;
                        }

                    }

                    // Write the data from the buffer to the cache in memory.
                    downloadCache.Write(downloadBuffer, 0, bytesSize);

                    CachedSize += bytesSize;

                    receivedBufferCount++;

                    // Fire the DownloadProgressChanged event.
                    if (receivedBufferCount == this.BufferCountPerNotification)
                    {
                        InternalDownloadProgressChanged(CachedSize);
                        receivedBufferCount = 0;
                    }
                }


                // Update the used time when the current doanload is stopped.
                usedTime = usedTime.Add(DateTime.Now - lastStartTime);

                // Update the status of the client. Above loop will be stopped when the 
                // status of the client is pausing, canceling or completed.
                if (this.Status == DownloadStatus.Pausing)
                {
                    this.Status = DownloadStatus.Paused;
                }
                else if (this.Status == DownloadStatus.Canceling)
                {
                    this.Status = DownloadStatus.Canceled;
                }
                else
                {
                    if (File.Exists(DownloadPath))
                    {
                        File.Delete(DownloadPath);
                    }
                    File.Move(DownloadPathTemp, DownloadPath);
                    this.Status = DownloadStatus.Completed;
                    return;
                }

            }
            catch (Exception ex)
            {
                // Fire the DownloadCompleted event with the error.
                this.OnDownloadCompleted(
                    new WebDownloadCompletedEventArgs(
                        null,
                        this.DownloadedSize,
                        this.TotalSize,
                        this.TotalUsedTime,
                        ex));
                return;
            }
            finally
            {
                // When the above code has ended, close the streams.
                if (responseStream != null)
                {
                    responseStream.Close();
                }
                if (webResponse != null)
                {
                    webResponse.Close();
                }
                if (downloadCache != null)
                {
                    downloadCache.Close();
                }
            }
        }





        /// <summary>
        /// Write the data in cache to local file.
        /// </summary>
        void WriteCacheToFile(MemoryStream downloadCache, int cachedSize)
        {
            // Lock other threads or processes to prevent from writing data to the file.
            lock (fileLocker)
            {
                using (FileStream fileStream = new FileStream(DownloadPathTemp, FileMode.Open))
                {
                    byte[] cacheContent = new byte[cachedSize];
                    downloadCache.Seek(0, SeekOrigin.Begin);
                    downloadCache.Read(cacheContent, 0, cachedSize);
                    fileStream.Seek(DownloadedSize + StartPoint, SeekOrigin.Begin);
                    fileStream.Write(cacheContent, 0, cachedSize);
                }
            }
        }

        /// <summary>
        /// The method will be called by the OnStatusChanged method.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDownloadCompleted(WebDownloadCompletedEventArgs e)
        {
            if (e.Error != null && this.status != DownloadStatus.Canceled)
            {
                this.Status = DownloadStatus.Completed;
            }

            if (DownloadCompleted != null)
            {
                DownloadCompleted(this, e);
            }
        }

        /// <summary>
        /// Calculate the download speed and fire the  DownloadProgressChanged event.
        /// </summary>
        /// <param name="cachedSize"></param>
        private void InternalDownloadProgressChanged(int cachedSize)
        {
            int speed = 0;
            DateTime current = DateTime.Now;
            TimeSpan interval = current - lastNotificationTime;

            if (interval.TotalSeconds < 60)
            {
                speed = (int)Math.Floor((this.DownloadedSize + cachedSize - this.lastNotificationDownloadedSize) / interval.TotalSeconds);
            }
            lastNotificationTime = current;
            lastNotificationDownloadedSize = this.DownloadedSize + cachedSize;

            this.OnDownloadProgressChanged(new WebDownloadProgressChangedEventArgs
                           (this.DownloadedSize + cachedSize, this.TotalSize, speed));


        }

        protected virtual void OnDownloadProgressChanged(WebDownloadProgressChangedEventArgs e)
        {
            if (DownloadProgressChanged != null)
            {
                DownloadProgressChanged(this, e);
            }
        }

        protected virtual void OnStatusChanged(EventArgs e)
        {
            switch (this.Status)
            {
                case DownloadStatus.Waiting:
                case DownloadStatus.Downloading:
                case DownloadStatus.Paused:
                case DownloadStatus.Canceled:
                case DownloadStatus.Completed:
                    if (this.StatusChanged != null)
                    {
                        this.StatusChanged(this, e);
                    }
                    break;
                default:
                    break;
            }

            if (this.status == DownloadStatus.Canceled)
            {
                Exception ex = new Exception("Downloading is canceled by user's request. ");
                this.OnDownloadCompleted(
                    new WebDownloadCompletedEventArgs(
                        null,
                        this.DownloadedSize,
                        this.TotalSize,
                        this.TotalUsedTime,
                        ex));
            }

            if (this.Status == DownloadStatus.Completed)
            {
                this.OnDownloadCompleted(
                    new WebDownloadCompletedEventArgs(
                        new FileInfo(this.DownloadPath),
                        this.DownloadedSize,
                        this.TotalSize,
                        this.TotalUsedTime,
                        null));
            }
        }
    }
}
