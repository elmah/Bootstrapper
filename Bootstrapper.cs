#region Copyright (c) 2010 Atif Aziz. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

#region Assembly Information

[assembly: System.Reflection.AssemblyTitle("Elmah.Bootstrapper")]
[assembly: System.Reflection.AssemblyDescription("")]
[assembly: System.Reflection.AssemblyCompany("")]
[assembly: System.Reflection.AssemblyProduct("ELMAH")]
[assembly: System.Reflection.AssemblyCopyright("Copyright \u00a9 2011")]
[assembly: System.Reflection.AssemblyTrademark("")]
[assembly: System.Reflection.AssemblyCulture("")]

[assembly: System.Runtime.InteropServices.ComVisible(false)]

[assembly: System.Reflection.AssemblyVersion("0.9.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("0.9.0.0")]

#if DEBUG
[assembly: System.Reflection.AssemblyConfiguration("DEBUG")]
#else
[assembly: System.Reflection.AssemblyConfiguration("RELEASE")]
#endif

#endregion

[assembly: System.Web.PreApplicationStartMethod(typeof(Elmah.Bootstrapper.Ignition), "Start")]

namespace Elmah.Bootstrapper
{
    #region Imports

    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Web;

    #endregion

    sealed class ErrorLogHandlerMappingModule : HttpModuleBase
    {
        ErrorLogPageFactory _errorLogPageFactory;

        ErrorLogPageFactory HandlerFactory
        {
            get { return _errorLogPageFactory ?? (_errorLogPageFactory = new ErrorLogPageFactory()); }
        }

        protected override void OnInit(HttpApplication application)
        {
            application.Subscribe(h => application.PostMapRequestHandler += h, OnPostMapRequestHandler);
            application.Subscribe(h => application.EndRequest += h, OnEndRequest);
        }

        void OnPostMapRequestHandler(HttpContextBase context)
        {
            var request = context.Request;

            var url = request.FilePath;
            var match = Regex.Match(url, @"(/(?:.*\b)?(?:elmah|errors|errorlog))(/.+)?$",
                                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
                             .BindNum((fst, snd) => new { fst.Success, Url = fst.Value, PathInfo = snd.Value });

            if (!match.Success)
                return;

            url = match.Url;
            // ReSharper disable once PossibleNullReferenceException
            var queryString = request.Url.Query;

            context.RewritePath(url, match.PathInfo,
                                queryString.Length > 0 && queryString[0] == '?'
                                ? queryString.Substring(1)
                                : queryString);

            var pathTranslated = request.PhysicalApplicationPath;
            var factory = HandlerFactory;
            var handler = factory.GetHandler(context, request.HttpMethod, url, pathTranslated);
            if (handler == null)
                return;

            context.Items[this] = new ContextState
            {
                Handler = handler,
                HandlerFactory = factory,
            };

            context.Handler = handler;
        }

        void OnEndRequest(HttpContextBase context)
        {
            var state = context.Items[this] as ContextState;
            if (state == null)
                return;
            state.HandlerFactory.ReleaseHandler(state.Handler);
        }

        sealed class ContextState
        {
            public IHttpHandler Handler;
            public IHttpHandlerFactory HandlerFactory;
        }
    }

    public static class Ignition
    {
        static readonly object Lock = new object();

        static bool _registered;

        public static void Start()
        {
            lock (Lock)
            {
                if (_registered)
                    return;
                StartImpl();
                _registered = true;
            }
        }

        static void StartImpl()
        {
            // TODO Consider what happens if registration fails halfway

            ServiceCenter.Current = GetServiceProvider;

            foreach (var type in DefaultModuleTypeSet)
                RegisterModule(type);
        }

        static void RegisterModule(Type moduleType)
        {
#if NET40
            Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(moduleType);
#else
            HttpApplication.RegisterModule(moduleType);
#endif
        }

        static IEnumerable<Type> DefaultModuleTypeSet
        {
            get
            {
                yield return typeof(ErrorLogModule);
                yield return typeof(ErrorMailModule);
                yield return typeof(ErrorFilterModule);
                yield return typeof(ErrorTweetModule);
                yield return typeof(ErrorLogHandlerMappingModule);
            }
        }

        public static IServiceProvider GetServiceProvider(object context)
        {
            return GetServiceProvider(AsHttpContextBase(context));
        }

        static HttpContextBase AsHttpContextBase(object context)
        {
            if (context == null)
                return null;
            var httpContextBase = context as HttpContextBase;
            if (httpContextBase != null)
                return httpContextBase;
            var httpContext = context as HttpContext;
            return httpContext == null
                 ? null
                 : new HttpContextWrapper(httpContext);
        }

        static readonly object ContextKey = new object();

        static IServiceProvider GetServiceProvider(HttpContextBase context)
        {
            if (context != null)
            {
                var sp = context.Items[ContextKey] as IServiceProvider;
                if (sp != null)
                    return sp;
            }

            var container = new ServiceContainer(ServiceCenter.Default(context));

            if (context != null)
            {
                var logPath = context.Server.MapPath("~/App_Data/errors/xmlstore");
                container.AddService(typeof (ErrorLog), delegate
                {
                    return Directory.Exists(logPath)
                         ? new XmlFileErrorLog(logPath)
                         : (object) new MemoryErrorLog();
                });

                context.Items[ContextKey] = container;
            }

            return container;
        }
    }

    static class WebExtensions
    {
        /// <summary>
        /// Helps with subscribing to <see cref="HttpApplication"/> events
        /// but where the handler
        /// </summary>

        public static void Subscribe(this HttpApplication application,
            Action<EventHandler> subscriber,
            Action<HttpContextBase> handler)
        {
            if (application == null) throw new ArgumentNullException("application");
            if (subscriber == null) throw new ArgumentNullException("subscriber");
            if (handler == null) throw new ArgumentNullException("handler");

            subscriber((sender, _) => handler(new HttpContextWrapper(((HttpApplication)sender).Context)));
        }

        /// <summary>
        /// Same as <see cref="IHttpHandlerFactory.GetHandler"/> except the
        /// HTTP context is typed as <see cref="HttpContextBase"/> instead
        /// of <see cref="HttpContext"/>.
        /// </summary>

        public static IHttpHandler GetHandler(this IHttpHandlerFactory factory,
            HttpContextBase context, string requestType,
            string url, string pathTranslated)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return factory.GetHandler(context.ApplicationInstance.Context, requestType, url, pathTranslated);
        }
    }

    static class RegexExtensions
    {
        public static T BindNum<T>(this Match match, Func<Group, Group, T> resultor)
        {
            if (match == null) throw new ArgumentNullException("match");
            if (resultor == null) throw new ArgumentNullException("resultor");
            var groups = match.Groups;
            return resultor(groups[1], groups[2]);
        }
    }
}