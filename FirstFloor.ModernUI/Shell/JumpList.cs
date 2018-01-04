// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

using FirstFloor.ModernUI.Shell.Standard;

namespace FirstFloor.ModernUI.Shell
{
    public enum JumpItemRejectionReason
    {
        None,

        InvalidItem,

        NoRegisteredHandler,

        RemovedByUser,
    }

    public sealed class JumpItemsRejectedEventArgs : EventArgs
    {
        public JumpItemsRejectedEventArgs() : this(null, null) {}

        public JumpItemsRejectedEventArgs(IList<JumpItem> rejectedItems, IList<JumpItemRejectionReason> reasons)
        {
            if((rejectedItems == null && reasons != null) || (reasons == null && rejectedItems != null)
               || (rejectedItems != null && reasons != null && rejectedItems.Count != reasons.Count))
            {
                throw new ArgumentException("The counts of rejected items doesn't match the count of reasons.");
            }

            if(rejectedItems != null)
            {
                RejectedItems = new List<JumpItem>(rejectedItems).AsReadOnly();
                RejectionReasons = new List<JumpItemRejectionReason>(reasons).AsReadOnly();
            }
            else
            {
                RejectedItems = new List<JumpItem>().AsReadOnly();
                RejectionReasons = new List<JumpItemRejectionReason>().AsReadOnly();
            }
        }

        public IList<JumpItem> RejectedItems { get; private set; }
        public IList<JumpItemRejectionReason> RejectionReasons { get; private set; }
    }

    public sealed class JumpItemsRemovedEventArgs : EventArgs
    {
        public JumpItemsRemovedEventArgs() : this(null) {}

        public JumpItemsRemovedEventArgs(IEnumerable<JumpItem> removedItems)
        {
            RemovedItems = removedItems != null ? new List<JumpItem>(removedItems).AsReadOnly() : new List<JumpItem>().AsReadOnly();
        }

        public IList<JumpItem> RemovedItems { get; private set; }
    }

    [ContentProperty("JumpItems")]
    public sealed class JumpList : ISupportInitialize
    {
        private static readonly object s_lock = new object();
        private static readonly Dictionary<Application, JumpList> s_applicationMap = new Dictionary<Application, JumpList>();
        private static readonly string _FullName;

        #region Converter methods

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static IShellLinkW CreateLinkFromJumpTask(JumpTask jumpTask, bool allowSeparators)
        {
            Debug.Assert(jumpTask != null);

            if(string.IsNullOrEmpty(jumpTask.Title))
            {
                if(!allowSeparators || !string.IsNullOrEmpty(jumpTask.CustomCategory))
                {
                    return null;
                }
            }
            var link = (IShellLinkW) Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid(CLSID.ShellLink)));
            try
            {
                var appPath = _FullName;
                if(!string.IsNullOrEmpty(jumpTask.ApplicationPath))
                {
                    appPath = jumpTask.ApplicationPath;
                }
                link.SetPath(appPath);

                if(!string.IsNullOrEmpty(jumpTask.WorkingDirectory))
                {
                    link.SetWorkingDirectory(jumpTask.WorkingDirectory);
                }
                if(!string.IsNullOrEmpty(jumpTask.Arguments))
                {
                    link.SetArguments(jumpTask.Arguments);
                }

                if(jumpTask.IconResourceIndex != -1)
                {
                    var resourcePath = _FullName;
                    if(!string.IsNullOrEmpty(jumpTask.IconResourcePath))
                    {
                        if(jumpTask.IconResourcePath.Length >= Win32Value.MAX_PATH)
                        {
                            return null;
                        }
                        resourcePath = jumpTask.IconResourcePath;
                    }
                    link.SetIconLocation(resourcePath, jumpTask.IconResourceIndex);
                }
                if(!string.IsNullOrEmpty(jumpTask.Description))
                {
                    link.SetDescription(jumpTask.Description);
                }
                var propStore = (IPropertyStore) link;
                using(var pv = new PROPVARIANT())
                {
                    var pkey = default(PKEY);
                    if(!string.IsNullOrEmpty(jumpTask.Title))
                    {
                        pv.SetValue(jumpTask.Title);
                        pkey = PKEY.Title;
                    }
                    else
                    {
                        pv.SetValue(true);
                        pkey = PKEY.AppUserModel_IsDestListSeparator;
                    }
                    propStore.SetValue(ref pkey, pv);
                }
                propStore.Commit();
                var retLink = link;
                link = null;
                return retLink;
            }
            catch(Exception)
            {
                return null;
            }
            finally
            {
                Utility.SafeRelease(ref link);
            }
        }

        private static IShellItem2 GetShellItemForPath(string path)
        {
            if(string.IsNullOrEmpty(path))
            {
                return null;
            }
            var iidShellItem2 = new Guid(IID.ShellItem2);
            object unk;
            var hr = NativeMethods.SHCreateItemFromParsingName(path, null, ref iidShellItem2, out unk);

            if(hr == (HRESULT) Win32Error.ERROR_FILE_NOT_FOUND || hr == (HRESULT) Win32Error.ERROR_PATH_NOT_FOUND)
            {
                hr = HRESULT.S_OK;
                unk = null;
            }
            hr.ThrowIfFailed();
            return (IShellItem2) unk;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static IShellItem2 CreateItemFromJumpPath(JumpPath jumpPath)
        {
            Debug.Assert(jumpPath != null);
            try
            {
                return GetShellItemForPath(Path.GetFullPath(jumpPath.Path));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        private static JumpItem GetJumpItemForShellObject(object shellObject)
        {
            var shellItem = shellObject as IShellItem2;
            var shellLink = shellObject as IShellLinkW;
            if(shellItem != null)
            {
                var path = new JumpPath {Path = shellItem.GetDisplayName(SIGDN.DESKTOPABSOLUTEPARSING),};
                return path;
            }
            if(shellLink != null)
            {
                var pathBuilder = new StringBuilder((int) Win32Value.MAX_PATH);
                shellLink.GetPath(pathBuilder, pathBuilder.Capacity, null, SLGP.RAWPATH);
                var argsBuilder = new StringBuilder((int) Win32Value.INFOTIPSIZE);
                shellLink.GetArguments(argsBuilder, argsBuilder.Capacity);
                var descBuilder = new StringBuilder((int) Win32Value.INFOTIPSIZE);
                shellLink.GetDescription(descBuilder, descBuilder.Capacity);
                var iconBuilder = new StringBuilder((int) Win32Value.MAX_PATH);
                int iconIndex;
                shellLink.GetIconLocation(iconBuilder, iconBuilder.Capacity, out iconIndex);
                var dirBuilder = new StringBuilder((int) Win32Value.MAX_PATH);
                shellLink.GetWorkingDirectory(dirBuilder, dirBuilder.Capacity);
                var task = new JumpTask
                {
                    ApplicationPath = pathBuilder.ToString(),
                    Arguments = argsBuilder.ToString(),
                    Description = descBuilder.ToString(),
                    IconResourceIndex = iconIndex,
                    IconResourcePath = iconBuilder.ToString(),
                    WorkingDirectory = dirBuilder.ToString(),
                };
                using(var pv = new PROPVARIANT())
                {
                    var propStore = (IPropertyStore) shellLink;
                    var pkeyTitle = PKEY.Title;
                    propStore.GetValue(ref pkeyTitle, pv);

                    task.Title = pv.GetValue() ?? "";
                }
                return task;
            }

            Debug.Assert(false);
            return null;
        }

        private static string ShellLinkToString(IShellLinkW shellLink)
        {
            var pathBuilder = new StringBuilder((int) Win32Value.MAX_PATH);
            shellLink.GetPath(pathBuilder, pathBuilder.Capacity, null, SLGP.RAWPATH);
            string title = null;

            using(var pv = new PROPVARIANT())
            {
                var propStore = (IPropertyStore) shellLink;
                var pkeyTitle = PKEY.Title;
                propStore.GetValue(ref pkeyTitle, pv);

                title = pv.GetValue() ?? "";
            }
            var argsBuilder = new StringBuilder((int) Win32Value.INFOTIPSIZE);
            shellLink.GetArguments(argsBuilder, argsBuilder.Capacity);

            return pathBuilder.ToString().ToUpperInvariant() + title.ToUpperInvariant() + argsBuilder;
        }

        #endregion

        private Application _application;

        private bool? _initializing;

        private List<JumpItem> _jumpItems;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static JumpList()
        {
            _FullName = NativeMethods.GetModuleFileName(IntPtr.Zero);
        }

        public JumpList() : this(null, false, false)
        {
            _initializing = null;
        }

        public JumpList(IEnumerable<JumpItem> items, bool showFrequent, bool showRecent)
        {
            _jumpItems = items != null ? new List<JumpItem>(items) : new List<JumpItem>();
            ShowFrequentCategory = showFrequent;
            ShowRecentCategory = showRecent;

            _initializing = false;
        }

        public bool ShowFrequentCategory { get; set; }

        public bool ShowRecentCategory { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<JumpItem> JumpItems { get { return _jumpItems; } }

        private bool _IsUnmodified { get { return _initializing == null && JumpItems.Count == 0 && !ShowRecentCategory && !ShowFrequentCategory; } }

        private static string _RuntimeId
        {
            get
            {
                string appId;
                var hr = NativeMethods.GetCurrentProcessExplicitAppUserModelID(out appId);
                if(hr == HRESULT.E_FAIL)
                {
                    hr = HRESULT.S_OK;
                    appId = null;
                }
                hr.ThrowIfFailed();
                return appId;
            }
        }

        #region ISupportInitialize Members

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "BeginInit")]
        public void BeginInit()
        {
            if(!_IsUnmodified)
            {
                throw new InvalidOperationException("Calls to BeginInit cannot be nested.");
            }
            _initializing = true;
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "EndInit")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "BeginInit")]
        public void EndInit()
        {
            if(_initializing != true)
            {
                throw new NotSupportedException("Can't call EndInit without first calling BeginInit.");
            }
            _initializing = false;

            ApplyFromApplication();
        }

        #endregion

        public static void AddToRecentCategory(string itemPath)
        {
            Verify.FileExists(itemPath, "itemPath");
            itemPath = Path.GetFullPath(itemPath);
            NativeMethods.SHAddToRecentDocs(itemPath);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static void AddToRecentCategory(JumpPath jumpPath)
        {
            Verify.IsNotNull(jumpPath, "jumpPath");
            AddToRecentCategory(jumpPath.Path);
        }

        public static void AddToRecentCategory(JumpTask jumpTask)
        {
            Verify.IsNotNull(jumpTask, "jumpTask");

            if(Utility.IsOSWindows7OrNewer)
            {
                var shellLink = CreateLinkFromJumpTask(jumpTask, false);
                try
                {
                    if(shellLink != null)
                    {
                        NativeMethods.SHAddToRecentDocs(shellLink);
                    }
                }
                finally
                {
                    Utility.SafeRelease(ref shellLink);
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "JumpList")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "EndInit")]
        public void Apply()
        {
            if(_initializing == true)
            {
                throw new InvalidOperationException("The JumpList can't be applied until EndInit has been called.");
            }

            _initializing = false;
            _ApplyList();
        }

        private void ApplyFromApplication()
        {
            if(_initializing != true && !_IsUnmodified)
            {
                _initializing = false;
            }
            if(_application == Application.Current && _initializing == false)
            {
                _ApplyList();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "JumpLists")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "Standard.Verify.IsApartmentState(System.Threading.ApartmentState,System.String)")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void _ApplyList()
        {
            Debug.Assert(_initializing == false);
            Verify.IsApartmentState(ApartmentState.STA, "JumpLists can only be effected on STA threads.");

            if(!Utility.IsOSWindows7OrNewer)
            {
                RejectEverything();
                return;
            }
            List<JumpItem> successList;
            List<_RejectedJumpItemPair> rejectedList;
            List<_ShellObjectPair> removedList;
            try
            {
                _BuildShellLists(out successList, out rejectedList, out removedList);
            }
            catch(Exception)
            {
                Assert.Fail();
                RejectEverything();
                return;
            }
            _jumpItems = successList;

            var rejectedHandler = JumpItemsRejected;
            var removedHandler = JumpItemsRemovedByUser;
            if(rejectedList.Count > 0 && rejectedHandler != null)
            {
                var items = new List<JumpItem>(rejectedList.Count);
                var reasons = new List<JumpItemRejectionReason>(rejectedList.Count);
                foreach(var rejectionPair in rejectedList)
                {
                    items.Add(rejectionPair.JumpItem);
                    reasons.Add(rejectionPair.Reason);
                }
                rejectedHandler(this, new JumpItemsRejectedEventArgs(items, reasons));
            }
            if(removedList.Count > 0 && removedHandler != null)
            {
                var items = new List<JumpItem>(removedList.Count);
                foreach(var shellMap in removedList)
                {
                    if(shellMap.JumpItem != null)
                    {
                        items.Add(shellMap.JumpItem);
                    }
                }
                if(items.Count > 0)
                {
                    removedHandler(this, new JumpItemsRemovedEventArgs(items));
                }
            }
        }

        private void _BuildShellLists(out List<JumpItem> successList, out List<_RejectedJumpItemPair> rejectedList, out List<_ShellObjectPair> removedList)
        {
            List<List<_ShellObjectPair>> categories = null;
            removedList = null;
            var destinationList = CLSID.CoCreateInstance<ICustomDestinationList>(CLSID.DestinationList);
            try
            {
                var appId = _RuntimeId;
                if(!string.IsNullOrEmpty(appId))
                {
                    destinationList.SetAppID(appId);
                }

                uint slotsVisible;
                var removedIid = new Guid(IID.ObjectArray);
                var objectsRemoved = (IObjectArray) destinationList.BeginList(out slotsVisible, ref removedIid);

                removedList = GenerateJumpItems(objectsRemoved);

                successList = new List<JumpItem>(JumpItems.Count);

                rejectedList = new List<_RejectedJumpItemPair>(JumpItems.Count);

                categories = new List<List<_ShellObjectPair>> {new List<_ShellObjectPair>()};

                foreach(var jumpItem in JumpItems)
                {
                    if(jumpItem == null)
                    {
                        rejectedList.Add(new _RejectedJumpItemPair {JumpItem = jumpItem, Reason = JumpItemRejectionReason.InvalidItem});
                        continue;
                    }
                    object shellObject = null;
                    try
                    {
                        shellObject = GetShellObjectForJumpItem(jumpItem);

                        if(shellObject == null)
                        {
                            rejectedList.Add(new _RejectedJumpItemPair {Reason = JumpItemRejectionReason.InvalidItem, JumpItem = jumpItem});
                            continue;
                        }

                        if(ListContainsShellObject(removedList, shellObject))
                        {
                            rejectedList.Add(new _RejectedJumpItemPair {Reason = JumpItemRejectionReason.RemovedByUser, JumpItem = jumpItem});
                            continue;
                        }
                        var shellMap = new _ShellObjectPair {JumpItem = jumpItem, ShellObject = shellObject};
                        if(string.IsNullOrEmpty(jumpItem.CustomCategory))
                        {
                            categories[0].Add(shellMap);
                        }
                        else
                        {
                            var categoryExists = false;
                            foreach(var list in categories)
                            {
                                if(list.Count > 0 && list[0].JumpItem.CustomCategory == jumpItem.CustomCategory)
                                {
                                    list.Add(shellMap);
                                    categoryExists = true;
                                    break;
                                }
                            }
                            if(!categoryExists)
                            {
                                categories.Add(new List<_ShellObjectPair> {shellMap});
                            }
                        }

                        shellObject = null;
                    }
                    finally
                    {
                        Utility.SafeRelease(ref shellObject);
                    }
                }

                categories.Reverse();
                if(ShowFrequentCategory)
                {
                    destinationList.AppendKnownCategory(KDC.FREQUENT);
                }
                if(ShowRecentCategory)
                {
                    destinationList.AppendKnownCategory(KDC.RECENT);
                }

                foreach(var categoryList in categories)
                {
                    if(categoryList.Count > 0)
                    {
                        var categoryHeader = categoryList[0].JumpItem.CustomCategory;
                        AddCategory(destinationList, categoryHeader, categoryList, successList, rejectedList);
                    }
                }
                destinationList.CommitList();

                successList.Reverse();
            }
            finally
            {
                Utility.SafeRelease(ref destinationList);
                if(categories != null)
                {
                    foreach(var list in categories)
                    {
                        _ShellObjectPair.ReleaseShellObjects(list);
                    }
                }

                _ShellObjectPair.ReleaseShellObjects(removedList);
            }
        }

        private static bool ListContainsShellObject(List<_ShellObjectPair> removedList, object shellObject)
        {
            Debug.Assert(removedList != null);
            Debug.Assert(shellObject != null);
            if(removedList.Count == 0)
            {
                return false;
            }

            var shellItem = shellObject as IShellItem;
            if(shellItem != null)
            {
                foreach(var shellMap in removedList)
                {
                    var removedItem = shellMap.ShellObject as IShellItem;
                    if(removedItem != null)
                    {
                        if(0 == shellItem.Compare(removedItem, SICHINT.CANONICAL | SICHINT.TEST_FILESYSPATH_IF_NOT_EQUAL))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            var shellLink = shellObject as IShellLinkW;
            if(shellLink != null)
            {
                foreach(var shellMap in removedList)
                {
                    var removedLink = shellMap.ShellObject as IShellLinkW;
                    if(removedLink != null)
                    {
                        var removedLinkString = ShellLinkToString(removedLink);
                        var linkString = ShellLinkToString(shellLink);
                        if(removedLinkString == linkString)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            return false;
        }

        private static object GetShellObjectForJumpItem(JumpItem jumpItem)
        {
            var jumpPath = jumpItem as JumpPath;
            var jumpTask = jumpItem as JumpTask;

            if(jumpPath != null)
            {
                return CreateItemFromJumpPath(jumpPath);
            }
            if(jumpTask != null)
            {
                return CreateLinkFromJumpTask(jumpTask, true);
            }

            Debug.Assert(false);
            return null;
        }

        private static List<_ShellObjectPair> GenerateJumpItems(IObjectArray shellObjects)
        {
            Debug.Assert(shellObjects != null);
            var retList = new List<_ShellObjectPair>();
            var unknownIid = new Guid(IID.Unknown);
            var count = shellObjects.GetCount();
            for(uint i = 0; i < count; ++i)
            {
                var unk = shellObjects.GetAt(i, ref unknownIid);
                JumpItem item = null;
                try
                {
                    item = GetJumpItemForShellObject(unk);
                }
                catch(Exception e)
                {
                    if(e is NullReferenceException || e is SEHException)
                    {
                        throw;
                    }
                }
                retList.Add(new _ShellObjectPair {ShellObject = unk, JumpItem = item});
            }
            return retList;
        }

        private static void AddCategory(ICustomDestinationList cdl, string category, List<_ShellObjectPair> jumpItems, List<JumpItem> successList,
            List<_RejectedJumpItemPair> rejectionList, bool isHeterogenous = true)
        {
            Debug.Assert(jumpItems.Count != 0);
            Debug.Assert(cdl != null);
            var shellObjectCollection = (IObjectCollection) Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid(CLSID.EnumerableObjectCollection)));
            foreach(var itemMap in jumpItems)
            {
                shellObjectCollection.AddObject(itemMap.ShellObject);
            }
            var hr = string.IsNullOrEmpty(category) ? cdl.AddUserTasks(shellObjectCollection) : cdl.AppendCategory(category, shellObjectCollection);
            if(hr.Succeeded)
            {
                for(var i = jumpItems.Count; --i >= 0;)
                {
                    successList.Add(jumpItems[i].JumpItem);
                }
            }
            else
            {
                if(isHeterogenous && hr == HRESULT.DESTS_E_NO_MATCHING_ASSOC_HANDLER)
                {
                    Utility.SafeRelease(ref shellObjectCollection);
                    var linksOnlyList = new List<_ShellObjectPair>();
                    foreach(var itemMap in jumpItems)
                    {
                        if(itemMap.JumpItem is JumpPath)
                        {
                            rejectionList.Add(new _RejectedJumpItemPair {JumpItem = itemMap.JumpItem, Reason = JumpItemRejectionReason.NoRegisteredHandler});
                        }
                        else
                        {
                            linksOnlyList.Add(itemMap);
                        }
                    }
                    if(linksOnlyList.Count > 0)
                    {
                        Debug.Assert(jumpItems.Count != linksOnlyList.Count);
                        AddCategory(cdl, category, linksOnlyList, successList, rejectionList, false);
                    }
                }
                else
                {
                    Debug.Assert(HRESULT.DESTS_E_NO_MATCHING_ASSOC_HANDLER != hr);

                    foreach(var item in jumpItems)
                    {
                        rejectionList.Add(new _RejectedJumpItemPair {JumpItem = item.JumpItem, Reason = JumpItemRejectionReason.InvalidItem});
                    }
                }
            }
        }

        private void RejectEverything()
        {
            var handler = JumpItemsRejected;
            if(handler == null)
            {
                _jumpItems.Clear();
                return;
            }
            if(_jumpItems.Count > 0)
            {
                var reasons = new List<JumpItemRejectionReason>(JumpItems.Count);
                for(var i = 0; i < JumpItems.Count; ++i)
                {
                    reasons.Add(JumpItemRejectionReason.InvalidItem);
                }

                var args = new JumpItemsRejectedEventArgs(JumpItems, reasons);
                _jumpItems.Clear();
                handler(this, args);
            }
        }

        public event EventHandler<JumpItemsRejectedEventArgs> JumpItemsRejected;
        public event EventHandler<JumpItemsRemovedEventArgs> JumpItemsRemovedByUser;

        private class _RejectedJumpItemPair
        {
            public JumpItem JumpItem { get; set; }
            public JumpItemRejectionReason Reason { get; set; }
        }

        private class _ShellObjectPair
        {
            public JumpItem JumpItem { get; set; }

            public object ShellObject { get; set; }

            public static void ReleaseShellObjects(IEnumerable<_ShellObjectPair> list)
            {
                if(list != null)
                {
                    foreach(var shellMap in list)
                    {
                        var o = shellMap.ShellObject;
                        shellMap.ShellObject = null;
                        Utility.SafeRelease(ref o);
                    }
                }
            }
        }

        #region Attached Property Methods

        public static void SetJumpList(Application application, JumpList value)
        {
            Verify.IsNotNull(application, "application");
            lock(s_lock)
            {
                JumpList oldValue;
                if(s_applicationMap.TryGetValue(application, out oldValue) && oldValue != null)
                {
                    oldValue._application = null;
                }

                s_applicationMap[application] = value;
                if(value != null)
                {
                    value._application = application;
                }
            }
            if(value != null)
            {
                value.ApplyFromApplication();
            }
        }

        public static JumpList GetJumpList(Application application)
        {
            Verify.IsNotNull(application, "application");
            JumpList value;
            s_applicationMap.TryGetValue(application, out value);
            return value;
        }

        #endregion
    }
}