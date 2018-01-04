// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

using FirstFloor.ModernUI.Shell.Standard;

namespace FirstFloor.ModernUI.Shell
{
    public static class SingleInstance<TApplication> where TApplication : Application, ISingleInstanceApp
    {
        private const string Delimiter = ":";

        private const string ChannelNameSuffix = "SingeInstanceIPCChannel";

        private const string RemoteServiceName = "SingleInstanceApplicationService";

        private const string IpcProtocol = "ipc://";

        private static Mutex singleInstanceMutex;

        private static IpcServerChannel channel;

        private static IList<string> commandLineArgs;

        public static IList<string> CommandLineArgs { get { return commandLineArgs; } }

        public static bool InitializeAsFirstInstance(string uniqueName)
        {
            commandLineArgs = GetCommandLineArgs(uniqueName);

            var applicationIdentifier = uniqueName + Environment.UserName;

            var channelName = String.Concat(applicationIdentifier, Delimiter, ChannelNameSuffix);

            bool firstInstance;
            singleInstanceMutex = new Mutex(true, applicationIdentifier, out firstInstance);
            if(firstInstance)
            {
                CreateRemoteService(channelName);
            }
            else
            {
                SignalFirstInstance(channelName, commandLineArgs);
            }

            return firstInstance;
        }

        public static void Cleanup()
        {
            if(singleInstanceMutex != null)
            {
                singleInstanceMutex.Close();
                singleInstanceMutex = null;
            }

            if(channel != null)
            {
                ChannelServices.UnregisterChannel(channel);
                channel = null;
            }
        }

        private static IList<string> GetCommandLineArgs(string uniqueApplicationName)
        {
            string[] args = null;
            if(AppDomain.CurrentDomain.ActivationContext == null)
            {
                args = Environment.GetCommandLineArgs();
            }
            else
            {
                var appFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), uniqueApplicationName);

                var cmdLinePath = Path.Combine(appFolderPath, "cmdline.txt");
                if(File.Exists(cmdLinePath))
                {
                    try
                    {
                        using(TextReader reader = new StreamReader(cmdLinePath, Encoding.Unicode))
                        {
                            args = NativeMethods.CommandLineToArgvW(reader.ReadToEnd());
                        }

                        File.Delete(cmdLinePath);
                    }
                    catch(IOException) {}
                }
            }

            if(args == null)
            {
                args = new string[] {};
            }

            return new List<string>(args);
        }

        private static void CreateRemoteService(string channelName)
        {
            var serverProvider = new BinaryServerFormatterSinkProvider {TypeFilterLevel = TypeFilterLevel.Full};
            IDictionary props = new Dictionary<string, string>();

            props["name"] = channelName;
            props["portName"] = channelName;
            props["exclusiveAddressUse"] = "false";

            channel = new IpcServerChannel(props, serverProvider);

            ChannelServices.RegisterChannel(channel, true);

            var remoteService = new IPCRemoteService();
            RemotingServices.Marshal(remoteService, RemoteServiceName);
        }

        private static void SignalFirstInstance(string channelName, IList<string> args)
        {
            var secondInstanceChannel = new IpcClientChannel();
            ChannelServices.RegisterChannel(secondInstanceChannel, true);

            var remotingServiceUrl = IpcProtocol + channelName + "/" + RemoteServiceName;

            var firstInstanceRemoteServiceReference = (IPCRemoteService) RemotingServices.Connect(typeof(IPCRemoteService), remotingServiceUrl);

            if(firstInstanceRemoteServiceReference != null)
            {
                firstInstanceRemoteServiceReference.InvokeFirstInstance(args);
            }
        }

        private static object ActivateFirstInstanceCallback(object arg)
        {
            var args = arg as IList<string>;
            ActivateFirstInstance(args);
            return null;
        }

        private static void ActivateFirstInstance(IList<string> args)
        {
            if(Application.Current == null)
            {
                return;
            }

            ((TApplication) Application.Current).SignalExternalCommandLineArgs(args);
        }

        private class IPCRemoteService : MarshalByRefObject
        {
            public void InvokeFirstInstance(IList<string> args)
            {
                if(Application.Current != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(ActivateFirstInstanceCallback), args);
                }
            }

            public override object InitializeLifetimeService()
            {
                return null;
            }
        }
    }
}