#if UNITY_5_6_OR_NEWER 
#define HAS_TIMEOUT
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

#if UNITY_5_3
using UnityEngine.Experimental.Networking;
#else
using UnityEngine.Networking;
#endif

namespace CYM
{
    public class AsyncWebRequest
    {
        public UnityWebRequest Request;
        public Exception UploadException;

        public int Timeout = 20; // how long until the webrequest will attempt to abort
#if !HAS_TIMEOUT
        private float _timeoutCounter;
#endif

        public bool RequestIsError
        {
            get
            {
#if UNITY_2017_3
                return Request.isHttpError || Request.isNetworkError;
#elif HAS_TIMEOUT
                return Request.isNetworkError;
#else
                return Request.isError || RequestTimedOut;
#endif
            }
        }

#if !HAS_TIMEOUT
        private bool _requestTimedOut;
#endif
        public bool RequestTimedOut
        {
            get
            {
#if HAS_TIMEOUT
                return Request.error == "Request timeout";
#else
                return _requestTimedOut;
#endif

            }
        }

        public IEnumerator Post(string uri, WWWForm data, Action<UnityWebRequest, Exception> onFinished = null)
        {
            Request = UnityWebRequest.Post(uri, data);
            Request.chunkedTransfer = false; // required so the request sends the content-length header

            yield return sendRequest();

            if (onFinished != null)
                onFinished(Request, UploadException);
        }

        public IEnumerator Get(string uri, Action<UnityWebRequest, Exception> onFinished = null)
        {
            Request = UnityWebRequest.Get(uri);

            yield return sendRequest();

            if (onFinished != null)
                onFinished(Request, UploadException);
        }

        private IEnumerator sendRequest()
        {
#if HAS_TIMEOUT
            Request.timeout = Timeout;
#else
            _timeoutCounter = Timeout;
#endif

            // send the request
            AsyncOperation op = null;
            try
            {
#if UNITY_2017_3
                op = Request.SendWebRequest();
#else
                op = Request.Send();
#endif

            }
            catch (Exception e)
            {
                UploadException = e;
                yield break;
            }

            // block until request is finished
            while (!op.isDone)
            {
                yield return new WaitForEndOfFrame();
#if !HAS_TIMEOUT
                _timeoutCounter -= Time.deltaTime;
                if(_timeoutCounter <= 0)
                {
                    _requestTimedOut = true;
                    Request.Abort();
                    break;
                }
#endif
            }
        }

    }
}
