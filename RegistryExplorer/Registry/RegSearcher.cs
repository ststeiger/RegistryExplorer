
namespace RegistryExplorer.Registry
{


    enum RegSearchLookAt
	{
        Keys = 1,
        Values = 2,
        Data = 4
	} 


    class RegSearchArgs
    {
        RegSearchLookAt lookAt;
        bool lookAtKeys;
        bool lookAtValues;
        bool lookAtData;
        bool lookAtValuesOrData;

        public bool LookAtKeys
        { 
            get { return lookAtKeys; } 
        }
        
        public bool LookAtValues 
        { 
            get { return lookAtValues; } 
        }
        
        public bool LookAtData 
        { 
            get { return lookAtData; } 
        }
        
        public bool LookAtValuesOrData 
        { 
            get { return lookAtValuesOrData; } 
        }

        public bool MatchCase { get; set; }        

        public RegSearchLookAt LookAt 
        { 
            get
            {
                return lookAt;
            }
            set
            {
                lookAt = value;
                lookAtKeys = (lookAt & RegSearchLookAt.Keys) == RegSearchLookAt.Keys;
                lookAtValues = (lookAt & RegSearchLookAt.Values) == RegSearchLookAt.Values;
                lookAtData = (lookAt & RegSearchLookAt.Data) == RegSearchLookAt.Data;
                lookAtValuesOrData = lookAtValues || lookAtData;                
            }
        }
        
        public Microsoft.Win32.RegistryKey[] RootKeys { get; set; }
        
        public string Pattern { get; set; }
        
        public bool UseRegEx { get; set; }

        public RegSearchArgs(Microsoft.Win32.RegistryKey[] regKeys, string pattern, bool matchCase, RegSearchLookAt lookAt, bool useRegEx)
        {
            RootKeys = regKeys;
            Pattern = pattern;
            MatchCase = matchCase;
            LookAt = lookAt;
            UseRegEx = useRegEx;
        }
    }

    class RegSearchMatch
    {
        public string Key { get; private set; }
        public string Value { get; private set; }
        public string Data { get; private set; }

        public RegSearchMatch(string key, string value, string data)
        {
            Key = key;
            Value = value;
            Data = data;
        }

        public override string ToString()
        {
            return string.Format("({0}:{1}:{2})", Key, Value, Data);
        }
    }

    class MatchFoundEventArgs : System.EventArgs
    {
        public RegSearchMatch Match { get; private set; }
        
        public MatchFoundEventArgs(RegSearchMatch match)
        {
            Match = match;
        }
    }

    class SearchCompleteEventArgs : System.EventArgs
    {
        public System.Collections.Generic.List<RegSearchMatch> Matches { get; private set; }

        public SearchCompleteEventArgs(System.Collections.Generic.List<RegSearchMatch> matches)
        {
            Matches = matches;
        }
    }

    class RegSearcher
    {
        System.ComponentModel.BackgroundWorker worker;
        public event System.EventHandler<SearchCompleteEventArgs> SearchComplete;
        public event System.EventHandler<MatchFoundEventArgs> MatchFound;
        RegSearchArgs searchArgs;
        System.Collections.Generic.List<RegSearchMatch> matches;
        System.Collections.Generic.Queue<string> pendingKeys;
        RegistryExplorer.Comparers.Comparer comparer;

        public RegSearcher()
        {
            worker = new System.ComponentModel.BackgroundWorker() {WorkerSupportsCancellation = true, WorkerReportsProgress = true};
            worker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(worker_ProgressChanged);
        }

        public void Start(RegSearchArgs args)
        {
            searchArgs = args;
            if (args.UseRegEx)
                comparer = new RegistryExplorer.Comparers.RegexComparer(args.Pattern, !args.MatchCase);
            else
                comparer = new RegistryExplorer.Comparers.StringComparer(args.Pattern, !args.MatchCase);

            matches = new System.Collections.Generic.List<RegSearchMatch>();
            worker.RunWorkerAsync();
        }

        void worker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            MatchFound(this, new MatchFoundEventArgs((RegSearchMatch)e.UserState));
        }

        public void Stop()
        {
            if (worker.IsBusy)
            {
                lock (worker)
                {
                    worker.CancelAsync();
                    System.Threading.Monitor.Wait(worker); 
                }
            }
        }

        public bool IsBusy 
        {
            get { return worker.IsBusy; }
        }

        void worker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            SearchComplete(this, new SearchCompleteEventArgs(matches));
        }

        void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {            
            foreach (Microsoft.Win32.RegistryKey key in searchArgs.RootKeys)
                Search(key);
        }

        void Search(Microsoft.Win32.RegistryKey rootKey)
        {
            string rootKeyName = rootKey.Name.Substring(rootKey.Name.LastIndexOf('\\') + 1);
            ProcessKey(rootKey, rootKeyName);

            Microsoft.Win32.RegistryKey subKey = null;
            string keyName;
            string parentPath;
            int cropIndex = rootKey.Name.Length + 1;
            pendingKeys = new System.Collections.Generic.Queue<string>(rootKey.GetSubKeyNames());

            while (pendingKeys.Count > 0)
            {
                if (worker.CancellationPending)
                {
                    lock (worker)
                    {
                        System.Threading.Monitor.Pulse(worker); // for synchronous Stop()
                        return;
                    }                    
                }
                keyName = pendingKeys.Dequeue();

                try
                {
                    subKey = rootKey.OpenSubKey(keyName);
                }
                catch (System.Security.SecurityException)
                {
                    subKey = null;
                }

                if (subKey != null)
                {
                    ProcessKey(subKey, keyName);
                    parentPath = subKey.Name.Substring(cropIndex) + '\\';
                    EnqueueSubKeys(subKey, parentPath);
                }
            }
        }

        private void EnqueueSubKeys(Microsoft.Win32.RegistryKey key, string parentPath)
        {
            foreach (string name in key.GetSubKeyNames())
                pendingKeys.Enqueue(string.Concat(parentPath, name));
        }

        private void ProcessKey(Microsoft.Win32.RegistryKey key, string keyName)
        {
            if (searchArgs.LookAtKeys)
                MatchKey(key, keyName);
            if (searchArgs.LookAtValuesOrData)
            {
                foreach (string valueName in key.GetValueNames())
                {
                    if (worker.CancellationPending) return;
                    if (searchArgs.LookAtValues)
                        MatchValue(key, valueName);
                    if (searchArgs.LookAtData)
                        MatchData(key, valueName);
                }
            }
        }

        private void MatchData(Microsoft.Win32.RegistryKey key, string valueName)
        {
            string valueData;
            valueData = RegValue.ToString(key.GetValue(valueName, string.Empty));
            if (comparer.IsMatch(valueData))
                AddMatch(key.Name, valueName, valueData);
        }

        private void MatchValue(Microsoft.Win32.RegistryKey key, string valueName)
        {
            if (comparer.IsMatch(valueName))
                    AddMatch(key.Name, valueName, "-");
        }

        private void MatchKey(Microsoft.Win32.RegistryKey key, string keyName)
        {
            if (comparer.IsMatch(keyName))
                AddMatch(key.Name, "-", "-");
        }

        private void AddMatch(string key, string value, string data)
        {
            RegSearchMatch match = new RegSearchMatch(key, value, data);
            if (MatchFound != null)
                worker.ReportProgress(0, match);
            else if (SearchComplete != null)
                matches.Add(match);
        }


    }


}
