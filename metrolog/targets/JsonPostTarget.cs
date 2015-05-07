﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using MetroLog.Internal;
using MetroLog.Layouts;

namespace MetroLog.Targets
{
    public class JsonPostTarget : BufferedTarget
    {
        private IWebClientWrapper webClient;

        public ILoggingEnvironment LoggingEnvironment { get; private set; }

        public Uri Uri { get; private set; }

        public event EventHandler<HttpClientEventArgs> BeforePost;

        public JsonPostTarget()
            : base(new NullLayout(), 1)
        {
            this.webClient = PlatformAdapter.Resolve<IWebClientWrapper>();
            this.webClient.Encoding = System.Text.Encoding.UTF8;
        }

        public JsonPostTarget(int threshold, Uri uri, ILoggingEnvironment loggingEnvironment)
            : this(threshold, uri, loggingEnvironment, new NullLayout())
        {
        }

        public JsonPostTarget(int threshold, Uri uri, Layout layout)
            : this(threshold, uri, null, layout)
        {
        }

        public JsonPostTarget(int threshold, Uri uri, ILoggingEnvironment loggingEnvironment, Layout layout)
            : base(layout, threshold)
        {
            this.Uri = uri;

            if (loggingEnvironment != null)
            {
                this.LoggingEnvironment = loggingEnvironment;
            }
            else
            {
                this.LoggingEnvironment = PlatformAdapter.Resolve<ILoggingEnvironment>();
            }

            this.webClient = PlatformAdapter.Resolve<IWebClientWrapper>();
            this.webClient.Encoding = System.Text.Encoding.UTF8;
        }

        protected override async Task DoFlushAsync(IEnumerable<LogEventInfo> toFlush)
        {
            var wrapper = new JsonPostWrapper(this.LoggingEnvironment, toFlush);
            var json = wrapper.ToJson();

            this.OnBeforePost(new HttpClientEventArgs(this.webClient));

            var headers = new Dictionary<HttpRequestHeader, string> { { HttpRequestHeader.ContentType, "text/json" } };

            await this.webClient.UploadStringAsync(this.Uri, headers, json);
        }

        protected override void DoFlush(IEnumerable<LogEventInfo> toFlush)
        {
            var wrapper = new JsonPostWrapper(this.LoggingEnvironment, toFlush);
            var json = wrapper.ToJson();

            this.OnBeforePost(new HttpClientEventArgs(this.webClient));

            var headers = new Dictionary<HttpRequestHeader, string> { { HttpRequestHeader.ContentType, "text/json" } };

            this.webClient.UploadString(this.Uri, headers, json);
        }

        protected virtual void OnBeforePost(HttpClientEventArgs args)
        {
            if (this.BeforePost != null)
            {
                this.BeforePost(this, args);
            }
        }
    }
}