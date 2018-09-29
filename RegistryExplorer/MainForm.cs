
using System.Windows.Forms;


namespace RegistryExplorer
{


    partial class MainForm : Form
    {
        RegistryExplorer.Registry.RegSearcher searcher;
        System.DateTime searchStartTime;
        bool searchStarted;

        RegistryExplorer.Properties.Settings settings;
        RegistryExplorer.Collections.EventDictionary<string, string> favorites;
        

        public MainForm()
        {
            InitializeComponent();
            searcher = new RegistryExplorer.Registry.RegSearcher();

            searcher.SearchComplete += new System.EventHandler<
                RegistryExplorer.Registry.SearchCompleteEventArgs>(searcher_SearchComplete);

            searcher.MatchFound += new System.EventHandler<
                RegistryExplorer.Registry.MatchFoundEventArgs>(searcher_MatchFound);

            favorites = new RegistryExplorer.Collections.EventDictionary<string, string>();

            favorites.ItemAdded += new System.EventHandler<
                RegistryExplorer.Collections.ItemEventArgs<string, string>
                >(favorites_ItemAdded);

            favorites.ItemRemoved += new System.EventHandler<
                RegistryExplorer.Collections.ItemEventArgs<string, string>
                >(favorites_ItemRemoved);
        }


        void favorites_ItemRemoved(object sender, RegistryExplorer.Collections.ItemEventArgs<string, string> e)
        {
            favoritesToolStripMenuItem.DropDownItems.RemoveByKey(e.Item.Key);
        }


        void favorites_ItemAdded(object sender, RegistryExplorer.Collections.ItemEventArgs<string, string> e)
        {
            AddFavoriteMenuItem(e.Item.Key, e.Item.Value);
        }
        

        TreeNode CreateNode()
        {
            TreeNode node = new TreeNode();
            return node; 
        }
        

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            
            settings = RegistryExplorer.Properties.Settings.Default;
            LoadSettings();
            AddRootKeys();
            LoadFavorites();
            if (settings.LastKey != string.Empty)
                JumpToKey(settings.LastKey);

            // if (!Environment.Is64BitProcess)
            if (System.IntPtr.Size * 8 != 64)
                qWORDValuePopupMenuItem.Visible = false;
        }


        private void LoadFavorites()
        {
            RegistryExplorer.Registry.RegKey favoritesKey = RegistryExplorer.Registry.RegKey.Parse(
                RegistryExplorer.Registry.RegistryExplorerr.RegistryFavoritePath
            );

            if (favoritesKey == null)
                return;

            System.Collections.Generic.List<RegistryExplorer.Registry.RegValue> values =
                RegistryExplorer.Registry.RegistryExplorerr.GetValues(favoritesKey.Key);

            if (values.Count > 0)
            {                
                values.ForEach(val =>
                                {
                                    string key = val.Data.ToString();
                                    //removing "My Computer\" set by RegEdit
                                    key = key.Substring(key.IndexOf('\\') + 1);
                                    favorites[val.Name] = key;
                                });
            }
        }


        private void AddFavoriteMenuItem(string name, string key)
        {
            ToolStripItem item = new ToolStripMenuItem(name);
            item.Tag = key;
            item.Name = name;
            favoritesToolStripMenuItem.DropDownItems.Add(item);
            item.Click += new System.EventHandler(favoriteMenuItem_Click);
        }


        void favoriteMenuItem_Click(object sender, System.EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            JumpToKey(item.Tag.ToString());
        }


        private void AddRootKeys()
        {
            AddRootKey(Microsoft.Win32.Registry.ClassesRoot);
            AddRootKey(Microsoft.Win32.Registry.CurrentUser);
            AddRootKey(Microsoft.Win32.Registry.LocalMachine);
            AddRootKey(Microsoft.Win32.Registry.Users);
            AddRootKey(Microsoft.Win32.Registry.CurrentConfig);
        }


        private void AdjustControls()
        {
            //bug fix: gbSearch.Width is incorrect when docking is enabled.
            int gbSearchWidth = this.Width - gbSearch.Left * 2 - tbSearch.Left * 2 - 8;
            btnFind.Left = gbSearchWidth - btnFind.Width - 6;
            txtPattern.Width = gbSearchWidth - txtPattern.Left - btnFind.Width - 12;
            txtBranch.Width = gbSearchWidth - txtBranch.Left - btnFind.Width - 12;
        }


        void AddRootKey(Microsoft.Win32.RegistryKey key)
        {
            TreeNode node = CreateNode(key.Name, key.Name, key);
            tvwKeys.Nodes.Add(node);
            node.Nodes.Add(CreateNode());
        }


        private TreeNode CreateNode(string key, string text, object tag)
        {
            TreeNode node = CreateNode();
            node.Text = text;
            node.Name = key;
            node.Tag = tag;
            return node;
        }


        void AddKeyToTree(TreeNode parent, RegistryExplorer.Registry.RegKey subKey)
        {
            Microsoft.Win32.RegistryKey key = subKey.Key;
            TreeNode newNode = CreateNode(key.Name, subKey.Name, key);
            parent.Nodes.Add(newNode);
            if (key.SubKeyCount > 0)
                newNode.Nodes.Add(CreateNode());
        }


        private void tvwKeys_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode parentNode = e.Node;
            if (parentNode.FirstNode.Tag == null)
            {
                using (new BusyCursor(this))
                {
                    LoadSubKeys(parentNode);
                }
            }
        }


        private void LoadSubKeys(TreeNode parentNode)
        {
            tvwKeys.SuspendLayout();

            parentNode.Nodes.Clear();
            Microsoft.Win32.RegistryKey key = (Microsoft.Win32.RegistryKey) parentNode.Tag;
            var subKeys = RegistryExplorer.Registry.RegistryExplorerr.GetSubKeys(key);

            subKeys.Sort(delegate (RegistryExplorer.Registry.RegKey a, RegistryExplorer.Registry.RegKey b)
                {
                    // return b.Name.CompareTo(a.Name); // DESC
                    return a.Name.CompareTo(b.Name); // ASC 
                }
            );

            
            foreach (RegistryExplorer.Registry.RegKey subKey in subKeys)
                AddKeyToTree(parentNode, subKey);

            tvwKeys.ResumeLayout();
        }


        private void tvwKeys_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Microsoft.Win32.RegistryKey key = e.Node.Tag as Microsoft.Win32.RegistryKey;
            LoadValues(key);
        }


        private void LoadValues(Microsoft.Win32.RegistryKey key)
        {            
            toolStripStatusLabel1.Text = key.Name;
            lstValues.Items.Clear();
            System.Collections.Generic.List<RegistryExplorer.Registry.RegValue> values = 
                RegistryExplorer.Registry.RegistryExplorerr.GetValues(key);

            if (values != null)
            {
                if (values.Count == 0)
                    AddValueToList(key, CreateDefaultValue());
                else
                {
                    lstValues.SuspendLayout();

                    RegistryExplorer.Registry.RegValue defaultValue = CreateDefaultValue();


                    // if (values.SingleOrDefault((val) => val.Name == defaultValue.Name) == null)
                    //      AddValueToList(key, defaultValue);


                    bool hasDefaultValue = false;
                    foreach (RegistryExplorer.Registry.RegValue val in values)
                    {
                        if (val.Name == defaultValue.Name)
                        {
                            hasDefaultValue = true;
                            break;
                        }
                            
                    }

                    if(!hasDefaultValue)
                        AddValueToList(key, defaultValue);

                    foreach (RegistryExplorer.Registry.RegValue value in values)
                        AddValueToList(key, value);

                    lstValues.ResumeLayout();
                }
            }                
        }


        private static RegistryExplorer.Registry.RegValue CreateDefaultValue()
        {
            return new RegistryExplorer.Registry.RegValue(string.Empty, Microsoft.Win32.RegistryValueKind.String, "(value not set)");
        }


        private ListViewItem AddValueToList(Microsoft.Win32.RegistryKey key, RegistryExplorer.Registry.RegValue value)
        {
            ListViewItem item = lstValues.Items.Add(value.Name);
            item.ImageKey = GetValueTypeIcon(value.Kind);
            item.Name = value.Name;
            item.Tag = key;

            item.SubItems.Add(RegistryExplorer.Registry.Extensions.ToDataType(value.Kind));
            ListViewItem.ListViewSubItem subItem = item.SubItems.Add(value.ToString());
            subItem.Tag = value;
            return item;
        }


        private string GetValueTypeIcon(Microsoft.Win32.RegistryValueKind registryValueKind)
        {
            if (registryValueKind == Microsoft.Win32.RegistryValueKind.ExpandString ||
                registryValueKind == Microsoft.Win32.RegistryValueKind.MultiString ||
                registryValueKind == Microsoft.Win32.RegistryValueKind.String)
                return "ascii";
            else
                return "binary";
        }


        private void MainForm_Resize(object sender, System.EventArgs e)
        {
            AdjustControls();
        }


        private void btnFind_Click(object sender, System.EventArgs e)
        {
            if (btnFind.Text == "F&ind")
            {
                Microsoft.Win32.RegistryKey[] keys;

                if (cmbSearch.Text == "All Hives")
                {
                    keys = new Microsoft.Win32.RegistryKey[cmbSearch.Items.Count];
                    for (int i = 0; i < cmbSearch.Items.Count; i++)
                        keys[i] = RegistryExplorer.Registry.RegUtility.ParseRootKey(cmbSearch.Items[i].ToString());
                }
                else
                    keys = new Microsoft.Win32.RegistryKey[] {
                        RegistryExplorer.Registry.RegUtility.ParseRootKey(cmbSearch.Text)
                    };

                if (txtBranch.Text != string.Empty)
                {
                    keys[0] = keys[0].OpenSubKey(txtBranch.Text);
                    if (keys[0] == null)
                    {
                        UIUtility.DisplayError(this, RegistryExplorer.Properties.Resources.Error_InvalidKey, txtBranch);
                        return;
                    }
                }

                RegistryExplorer.Registry.RegSearchArgs searchArgs = GetSearchArgs(keys);
                StartSearch();
                try
                {                    
                    searcher.Start(searchArgs);
                }
                catch (System.ArgumentException ex)
                {
                    toolStripStatusLabel1.Text = "Ready.";
                    UIUtility.DisplayError(this, ex.Message, txtPattern);
                    EnableSearch();
                    return;
                }
                searchStarted = true;                
            }
            else
            {
                btnFind.Enabled = false;
                searcher.Stop();
            }
        }


        private void StartSearch()
        {
            DisableSearch();
            lstResults.Items.Clear();
            toolStripStatusLabel1.Text = "Searching...";
            searchStartTime = System.DateTime.Now;
        }


        private RegistryExplorer.Registry.RegSearchArgs GetSearchArgs(Microsoft.Win32.RegistryKey[] keys)
        {
            RegistryExplorer.Registry.RegSearchLookAt lookAt = GetSearchTarget();
            RegistryExplorer.Registry.RegSearchArgs searchArgs = 
                new RegistryExplorer.Registry.RegSearchArgs(keys, txtPattern.Text, chkMatchCase.Checked, lookAt, chkUseRegex.Checked);

            return searchArgs;
        }


        private RegistryExplorer.Registry.RegSearchLookAt GetSearchTarget()
        {
            RegistryExplorer.Registry.RegSearchLookAt lookAt = 0;
            if (chkLookAtData.Checked)
                lookAt |= RegistryExplorer.Registry.RegSearchLookAt.Data;
            if (chkLookAtValues.Checked)
                lookAt |= RegistryExplorer.Registry.RegSearchLookAt.Values;
            if (chkLookAtKeys.Checked)
                lookAt |= RegistryExplorer.Registry.RegSearchLookAt.Keys;
            return lookAt;
        }               


        void searcher_MatchFound(object sender, RegistryExplorer.Registry.MatchFoundEventArgs e)
        {
            AddResultToListView(e.Match);
        }


        private void DisableSearch()
        {            
            btnFind.Text = "&Cancel";
            txtPattern.Enabled = chkLookAtKeys.Enabled = txtBranch.Enabled =
                chkLookAtValues.Enabled = chkLookAtData.Enabled = cmbSearch.Enabled = 
                chkMatchCase.Enabled = chkUseRegex.Enabled = false;
        }


        void searcher_SearchComplete(object sender, RegistryExplorer.Registry.SearchCompleteEventArgs e)
        {
            double seconds = System.DateTime.Now.Subtract(searchStartTime).TotalSeconds;
            int matches = lstResults.Items.Count;
            btnFind.Enabled = true;
            EnableSearch();
            if (tabControl1.SelectedIndex == 1)
            {
                toolStripStatusLabel1.Text = string.Format("Found {0} matches in {1} seconds.", matches, seconds);
                searchStarted = false;
            }
        }


        private void AddResultToListView(RegistryExplorer.Registry.RegSearchMatch result)
        {
            var item = lstResults.Items.Add(result.Key);
            item.Tag = result;
            item.SubItems.Add(RegistryExplorer.Registry.RegUtility.GetRegValueName(result.Value));
            item.SubItems.Add(result.Data);
        }


        private void EnableSearch()
        {
            btnFind.Text = "F&ind";
            txtPattern.Enabled = chkLookAtKeys.Enabled = txtBranch.Enabled =
                chkLookAtValues.Enabled = chkLookAtData.Enabled = cmbSearch.Enabled =
                chkMatchCase.Enabled = chkUseRegex.Enabled = true;
        }


        private void CheckedChanged(object sender, System.EventArgs e)
        {
            btnFind.Enabled = (chkLookAtKeys.Checked || chkLookAtValues.Checked || chkLookAtData.Checked) &&
                txtPattern.Text != string.Empty;
        }


        private void txtPattern_TextChanged(object sender, System.EventArgs e)
        {
            btnFind.Enabled = (chkLookAtKeys.Checked || chkLookAtValues.Checked || chkLookAtData.Checked) &&
                txtPattern.Text != string.Empty;
        }


        private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                if (searchStarted && searcher.IsBusy)
                    toolStripStatusLabel1.Text = "Searching...";
                else if (searchStarted)
                {
                    toolStripStatusLabel1.Text = "Search complete.";
                    searchStarted = false;
                }
                else
                    toolStripStatusLabel1.Text = "Ready.";
            }
            else
                toolStripStatusLabel1.Text = string.Empty;
        }


        private void aboutRegistryExplorerToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            (new AboutBox()).ShowDialog(this);
        }


        private void findToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            OnFindAction();            
        }


        private void OnFindAction()
        {
            string key = GetSelectedKey();
            if (key != string.Empty)
            {
                string hive;
                string branch;
                RegistryExplorer.Registry.RegUtility.SplitKey(key, out hive, out branch);

                if (!searcher.IsBusy)
                {
                    cmbSearch.SelectedItem = hive;
                    txtBranch.Text = branch;
                }
            }
            tabControl1.SelectedTab = tbSearch;
            txtPattern.Focus();
        }


        void CreatePopupMenu()
        {
            newPopupMenuItem.Visible =
                popupMenuSeparatorNew.Visible = GetNewMenuState();

            modifyPopupMenuItem.Visible =
                popupMenuSeperatorModify.Visible = GetModifyMenuState();

            if (ActiveControl == tvwKeys)
            {
                expandPopupMenuItem.Visible = true;
                expandPopupMenuItem.Enabled = tvwKeys.SelectedNode.Nodes.Count > 0;
                expandPopupMenuItem.Text = tvwKeys.SelectedNode.IsExpanded ? "Collapse" : "Expand";
            }
            else
                expandPopupMenuItem.Visible = false;
            
            refreshPopupMenuItem.Enabled = GetRefreshMenuState();
            deletePopupMenuItem.Enabled = GetDeleteMenuState();
            
            exportPopupMenuItem.Visible = 
                popupMenuSeperatorExport.Visible = (ActiveControl != lstValues);
            exportPopupMenuItem.Enabled = (ActiveControl != lstResults) || lstResults.SelectedItems.Count == 1;

            copyKeyNamePopupMenuItem.Visible =                 
                popupMenuSeperatorCopyKeyName.Visible = GetCopyMenuState();
        }


        private bool GetNewMenuState()
        {
            if ((ActiveControl == tvwKeys || (ActiveControl == lstValues && lstValues.Items.Count>0)) 
                && tvwKeys.SelectedNode != null)
                return true;
            return false;
        }


        private bool GetModifyMenuState()
        {
            if (ActiveControl == lstValues && lstValues.SelectedItems.Count == 1)
                return true;
            return false;
        }


        private void lstResults_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && lstResults.SelectedItems.Count > 0)
                DisplayPopupMenu(lstResults, e);
        }


        private void lstValues_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                DisplayPopupMenu(lstValues, e);
        }


        private void DisplayPopupMenu(Control source, MouseEventArgs e)
        {
            CreatePopupMenu();
            contextMenuStrip1.Show(source, e.X, e.Y);
        }


        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                if (ActiveControl == lstValues)
                    foreach (ListViewItem item in lstValues.Items)
                        item.Selected = true;
                else if (ActiveControl == lstResults)
                    foreach (ListViewItem item in lstResults.Items)
                        item.Selected = true;
            }
        }


        private void tvwKeys_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && tvwKeys.SelectedNode != null)
                DisplayPopupMenu(tvwKeys, e);
        }


        private string GetSelectedKey()
        {
            if (ActiveControl == tvwKeys && tvwKeys.SelectedNode != null)
                return tvwKeys.SelectedNode.Name;
            else if (ActiveControl == lstValues && lstValues.SelectedItems.Count > 0)
                return lstValues.SelectedItems[0].Tag.ToString();
            else if (ActiveControl == lstResults && lstResults.SelectedItems.Count > 0)
                return lstResults.SelectedItems[0].Text;
            else
                return string.Empty;
        }


        private void copyKeyNameToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            OnCopyKeyNameAction();
        }


        private void OnCopyKeyNameAction()
        {
            string key = GetSelectedKey();
            if (key != string.Empty)
                Clipboard.SetText(key);
        }


        private void tvwKeys_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // Bug fix: treeview doesnt change the selection on right click
            if (e.Button == MouseButtons.Right)
                tvwKeys.SelectedNode = e.Node;
        }


        private void editToolStripMenuItem_DropDownOpening(object sender, System.EventArgs e)
        {            
            CreateEditMenu();
        }


        private void CreateEditMenu()
        {
            newToolStripMenuItem.Visible =
                toolStripMenuSeperatorNew.Visible = GetNewMenuState();
            modifyToolStripMenuItem.Visible = 
                toolStripMenuSeperatorModify.Visible = GetModifyMenuState();
            refreshToolStripMenuItem.Enabled = GetRefreshMenuState();
            copyKeyNameToolStripMenuItem.Visible = 
                toolStripMenuSeperatorCopyKeyName.Visible = GetCopyMenuState();
            deleteToolStripMenuItem.Enabled = GetDeleteMenuState();            
        }


        private bool GetDeleteMenuState()
        {
            if (ActiveControl == tvwKeys && tvwKeys.SelectedNode != null)
                return (tvwKeys.SelectedNode.Level != 0);
            else if (ActiveControl is ListView && ((ListView)ActiveControl).SelectedItems.Count > 0)
                return true;
            return false;
        }


        private bool GetRefreshMenuState()
        {
            if ((ActiveControl == tvwKeys || (ActiveControl == lstValues && lstValues.Items.Count>0)) 
                && tvwKeys.SelectedNode != null)
                return true;
            else
                return false;
        }


        private bool GetCopyMenuState()
        {
            if ((ActiveControl == tvwKeys && tvwKeys.SelectedNode != null) ||
                (ActiveControl == lstResults && lstResults.SelectedItems.Count == 1))
                return true;
            return false;
        }


        private void deleteToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            OnDeleteAction();
        }


        private void OnRefreshAction()
        {
            if (tvwKeys.SelectedNode == null)
                return;

            using (new BusyCursor(this))
            {
                if (ActiveControl == tvwKeys || ActiveControl == lstValues)
                {
                    string lastSelectedKey = tvwKeys.SelectedNode.FullPath;
                    string lastSelectedValue = GetSelectedValue();

                    RefreshTreeView();

                    TreeNode[] matches = tvwKeys.Nodes.Find(lastSelectedKey, true);
                    if (matches.Length > 0)
                    {
                        tvwKeys.SelectedNode = matches[0];
                        SetSelectedValue(lastSelectedValue);
                    }
                }
            }
        }


        private void RefreshValues()
        {
            string key = GetSelectedValue();
            Microsoft.Win32.RegistryKey regKey = tvwKeys.SelectedNode.Tag as Microsoft.Win32.RegistryKey;
            if (regKey != null)
                LoadValues(regKey);
            SetSelectedValue(key);
        }


        private void SetSelectedValue(string key)
        {
            ListViewItem item = lstValues.Items[key];
            if (item != null)
                item.Selected = true;
        }


        private string GetSelectedValue()
        {
            if (lstValues.SelectedItems.Count == 1)
                return lstValues.SelectedItems[0].Name;
            else
                return string.Empty;
        }


        private void RefreshTreeView()
        {
            TreeNode targetNode;
            if (tvwKeys.SelectedNode == null)
                return;

            if (tvwKeys.SelectedNode.IsExpanded)
                targetNode = tvwKeys.SelectedNode;
            else if (tvwKeys.SelectedNode.Level == 0)
                return;
            else
                targetNode = tvwKeys.SelectedNode.Parent;

            bool error = false;
            do
                try
                {
                    LoadSubKeys(targetNode);
                }
                catch (System.IO.IOException)
                {
                    error = true;
                    targetNode = targetNode.Parent;
                }
            while (error && targetNode.Level > 0);
        }


        private void OnDeleteAction()
        {
            if (ActiveControl == tvwKeys)
                DeleteTreeKey();
            else if (ActiveControl == lstValues)
                DeleteListValue();
            else if (ActiveControl == lstResults)
                DeleteListEntry();
        }


        private void DeleteListEntry()
        {
            if (UIUtility.ConfirmAction(this, RegistryExplorer.Properties.Resources.Confirm_DeleteEntries, "Entry Delete", true))
                if (!DeleteEntries())
                    UIUtility.WarnUser(this, RegistryExplorer.Properties.Resources.Error_DeleteEntriesFail);
        }


        private void DeleteListValue()
        {
            if (UIUtility.ConfirmAction(this, RegistryExplorer.Properties.Resources.Confirm_DeleteValue, "Value Delete", true))
                if (!DeleteValues())
                    UIUtility.WarnUser(this, RegistryExplorer.Properties.Resources.Error_DeleteValueFail);
        }


        private void DeleteTreeKey()
        {
            if (UIUtility.ConfirmAction(this, RegistryExplorer.Properties.Resources.Confirm_DeleteKey, "Key Delete", true))
                if (RegistryExplorer.Registry.RegUtility.DeleteKey(tvwKeys.SelectedNode.Tag.ToString()))
                    tvwKeys.SelectedNode.Remove();
                else
                    UIUtility.WarnUser(this, RegistryExplorer.Properties.Resources.Error_DeleteKeyFail);
        }


        private bool DeleteEntries()
        {
            bool success = true;
            foreach (ListViewItem item in lstResults.SelectedItems)
            {
                RegistryExplorer.Registry.RegSearchMatch match = 
                    (RegistryExplorer.Registry.RegSearchMatch) item.Tag;

                // if value is not specified
                if (match.Value == "-")
                {
                    if (RegistryExplorer.Registry.RegUtility.DeleteKey(match.Key))
                        item.Remove();
                    else
                        success = false;
                }
                else
                    if (RegistryExplorer.Registry.RegUtility.DeleteValue(match.Key, match.Value))
                        item.Remove();
                    else
                        success = false;
            }
            return success;
        }


        private bool DeleteValues()
        {
            bool success = true;
            foreach (ListViewItem item in lstValues.SelectedItems)
            {
                if (RegistryExplorer.Registry.RegUtility.DeleteValue(item.Tag.ToString(), item.Text))
                    item.Remove();
                else
                    success = false;
            }
            return success;
        }


        private void exitToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (searcher != null)
                searcher.Stop();
            SaveSettings();
        }


        private void refreshToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            OnRefreshAction();
        }


        private void tvwKeys_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && GetDeleteMenuState())
                DeleteTreeKey();
        }


        private void expandPopupMenuItem_Click(object sender, System.EventArgs e)
        {
            if (tvwKeys.SelectedNode.IsExpanded)
                tvwKeys.SelectedNode.Collapse();
            else
                tvwKeys.SelectedNode.Expand();
        }


        private void LoadSettings()
        {
            if (settings.Location.X != -1)
                this.Location = settings.Location;
            this.Size = settings.Size;
            
            chkLookAtKeys.Checked = settings.LookAtKeys;
            chkLookAtValues.Checked = settings.LookAtValues;
            chkLookAtData.Checked = settings.LookAtData;
            chkMatchCase.Checked = settings.MatchCase;
            chkUseRegex.Checked = settings.UseRegEx;
            cmbSearch.SelectedItem = settings.SearchHive.Clone();
            lstValues.Columns[0].Width = settings.ValWidth1;
            lstValues.Columns[1].Width = settings.ValWidth2;
            lstValues.Columns[2].Width = settings.ValWidth3;
            lstResults.Columns[0].Width = settings.ResWidth1;
            lstResults.Columns[1].Width = settings.ResWidth2;
            lstResults.Columns[2].Width = settings.ResWidth3;

            if (settings.Maximized)
                this.WindowState = FormWindowState.Maximized;
        }


        private void SaveSettings()
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                settings.Maximized = false;
                settings.Size = this.Size;
                settings.Location = this.Location;
            }
            else
                settings.Maximized = true;

            settings.LookAtKeys = chkLookAtKeys.Checked;
            settings.LookAtValues = chkLookAtValues.Checked;
            settings.LookAtData = chkLookAtData.Checked;
            settings.MatchCase = chkMatchCase.Checked;
            settings.UseRegEx = chkUseRegex.Checked;
            settings.SearchHive = cmbSearch.SelectedItem.ToString();
            settings.ValWidth1 = lstValues.Columns[0].Width;
            settings.ValWidth2 = lstValues.Columns[1].Width;
            settings.ValWidth3 = lstValues.Columns[2].Width;
            settings.ResWidth1 = lstResults.Columns[0].Width;
            settings.ResWidth2 = lstResults.Columns[1].Width;
            settings.ResWidth3 = lstResults.Columns[2].Width;
            if (tvwKeys.SelectedNode != null)
                settings.LastKey = tvwKeys.SelectedNode.Name;
            settings.Save();
        }


        private void lstResults_DoubleClick(object sender, System.EventArgs e)
        {
            if (lstResults.SelectedItems.Count == 1)
            {
                RegistryExplorer.Registry.RegSearchMatch match = 
                    lstResults.SelectedItems[0].Tag as RegistryExplorer.Registry.RegSearchMatch;

                if (JumpToKey(match.Key))
                {
                    if (match.Value != "-")
                    {
                        string valueName = RegistryExplorer.Registry.RegUtility.GetRegValueName(match.Value);
                        ListViewItem item = lstValues.Items[valueName];
                        if (item != null)
                        {
                            item.Selected = true;
                            lstValues.Focus();
                        }
                    }
                }
            }
        }


        private bool JumpToKey(string key)
        {
            tabControl1.SelectedTab = tbExplorer;
            string[] tokens = key.Split('\\');
            TreeNode node = tvwKeys.Nodes[tokens[0]];
            if (node == null) 
                return false;
            SelectAndExpand(node);
            System.Text.StringBuilder path = new System.Text.StringBuilder(node.Name);
            for (int i = 1; i < tokens.Length; i++)
            {
                path.Append('\\');
                path.Append(tokens[i]);
                node = node.Nodes[path.ToString()];
                if (node == null)
                    return false;
                SelectAndExpand(node);
            }
            return true;
        }


        private void SelectAndExpand(TreeNode node)
        {
            node.EnsureVisible();
            tvwKeys.SelectedNode = node;
            node.Expand();
        }


        private void txtPattern_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                btnFind.PerformClick();
                e.SuppressKeyPress = true;
            }
        }


        private void lstValues_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && GetDeleteMenuState())
                DeleteListValue();
            else if (e.KeyCode == Keys.Enter)
                OnValueEditAction();
        }


        private void OnValueEditAction()
        {
            if (lstValues.SelectedItems.Count == 1)
            {
                RegistryExplorer.Registry.RegValue value = (RegistryExplorer.Registry.RegValue)
                    lstValues.SelectedItems[0].SubItems[2].Tag;

                if (value.ParentKey != null)
                {
                    RegistryExplorer.Editors.ValueEditor editor = null;

                    switch (value.Kind)
                    {
                        case Microsoft.Win32.RegistryValueKind.Binary:
                            editor = new RegistryExplorer.Editors.BinaryEditor(value);
                            break;                        
                        case Microsoft.Win32.RegistryValueKind.MultiString:
                            editor = new RegistryExplorer.Editors.MultiStringEditor(value);
                            break;
                        case Microsoft.Win32.RegistryValueKind.DWord:
                        case Microsoft.Win32.RegistryValueKind.QWord:
                            editor = new RegistryExplorer.Editors.DWordEditor(value);
                            break;
                        case Microsoft.Win32.RegistryValueKind.String:
                        case Microsoft.Win32.RegistryValueKind.ExpandString:
                            editor = new RegistryExplorer.Editors.StringEditor(value);
                            break;
                        case Microsoft.Win32.RegistryValueKind.Unknown:
                        default:
                            break;
                    }

                    if (editor != null)
                        if (editor.ShowDialog(this) == DialogResult.OK)
                            RefreshValues();
                }
            }
        }


        private void lstResults_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && GetDeleteMenuState())
                DeleteListEntry();
        }


        private void jumpToKeyToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            JumpToKeyDialog dialog = new JumpToKeyDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                if (!JumpToKey(dialog.txtKeyPath.Text))
                    UIUtility.DisplayError(this, RegistryExplorer.Properties.Resources.Error_InvalidKey);
        }

        private void exportToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            OnExportAction();
        }


        private void OnExportAction()
        {
            string key = GetSelectedKey();
            ExportDialog dialog = new ExportDialog();
            dialog.cmbBranch.Text = key;
            dialog.ShowDialog(this);
        }


        private void registryExplorerWebsiteToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            ShellUtility.OpenWebPage("https://github.com/ststeiger/RegistryExplorer");
        }


        private void lstValues_DoubleClick(object sender, System.EventArgs e)
        {
            OnValueEditAction();
        }


        private void modifyToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            OnValueEditAction();
        }


        private void keyToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            OnNewKeyAction();
        }


        private void OnNewKeyAction()
        {
            if (tvwKeys.SelectedNode == null)
                return;
            if (tvwKeys.HasChildren && !tvwKeys.SelectedNode.IsExpanded)
                tvwKeys.SelectedNode.Expand();
            Microsoft.Win32.RegistryKey key = (Microsoft.Win32.RegistryKey) tvwKeys.SelectedNode.Tag;
            string name = RegistryExplorer.Registry.RegUtility.GetNewKeyName(key);
            string path = key.Name + "\\" + name;
            // adding new object() as tag to prevent this key from being deleted on expanding.
            TreeNode node = CreateNode(path, name, new object());
            tvwKeys.SelectedNode.Nodes.Add(node);
            node.EnsureVisible();
            tvwKeys.LabelEdit = true;            
            node.BeginEdit();
        }


        private void tvwKeys_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            tvwKeys.LabelEdit = false;
            string keyName = e.Label == null ? e.Node.Text : e.Label;
            try
            {                
                Microsoft.Win32.RegistryKey readOnlyParent = (Microsoft.Win32.RegistryKey)e.Node.Parent.Tag;
                RegistryExplorer.Registry.RegKey parent = RegistryExplorer.Registry.RegKey.Parse(readOnlyParent.Name, true);
                parent.Key.CreateSubKey(keyName);
                e.Node.Name = parent.Key.Name + "\\" + keyName;
                e.Node.Tag = RegistryExplorer.Registry.RegKey.Parse(e.Node.Name).Key;
            }
            catch
            {
                e.Node.Remove();
                UIUtility.DisplayError(this, RegistryExplorer.Properties.Resources.Error_CreateKeyFail);
            }
        }


        private void stringValueToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            OnNewValueAction(Microsoft.Win32.RegistryValueKind.String);
        }


        private void OnNewValueAction(Microsoft.Win32.RegistryValueKind valueKind)
        {
            if (tvwKeys.SelectedNode == null)
                return;

            Microsoft.Win32.RegistryKey key = (Microsoft.Win32.RegistryKey) tvwKeys.SelectedNode.Tag;
            string name = RegistryExplorer.Registry.RegUtility.GetNewValueName(key);
            
            ListViewItem item = AddValueToList(key, 
                new RegistryExplorer.Registry.RegValue(name, valueKind,
                    RegistryExplorer.Registry.Extensions.GetDefaultData(valueKind)
                )
            );

            lstValues.LabelEdit = true;
            item.BeginEdit();
        }


        private void binaryValueToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            OnNewValueAction(Microsoft.Win32.RegistryValueKind.Binary);
        }


        private void dWORDValueToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            OnNewValueAction(Microsoft.Win32.RegistryValueKind.DWord);
        }


        private void multiStringValueToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            OnNewValueAction(Microsoft.Win32.RegistryValueKind.MultiString);
        }


        private void expandableStringValueToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            OnNewValueAction(Microsoft.Win32.RegistryValueKind.ExpandString);
        }


        private void qWORDValuePopupMenuItem_Click(object sender, System.EventArgs e)
        {
            OnNewValueAction(Microsoft.Win32.RegistryValueKind.QWord);
        }


        private void lstValues_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            lstValues.LabelEdit = false;
            ListViewItem item = lstValues.Items[e.Item];
            string valName = e.Label == null ? item.Text : e.Label;
            try
            {                
                Microsoft.Win32.RegistryKey readOnlyKey = (Microsoft.Win32.RegistryKey)item.Tag;
                Microsoft.Win32.RegistryKey key = RegistryExplorer.Registry.RegKey.Parse(readOnlyKey.Name, true).Key;
                RegistryExplorer.Registry.RegValue value = (RegistryExplorer.Registry.RegValue)item.SubItems[2].Tag;
                key.SetValue(valName, value.Data, value.Kind);
                item.Name = valName;
                item.SubItems[2].Tag = new RegistryExplorer.Registry.RegValue(readOnlyKey, valName);
            }
            catch
            {
                item.Remove();
                UIUtility.DisplayError(this, RegistryExplorer.Properties.Resources.Error_CreateValueFail);
            }
        }


        private void favoritesToolStripMenuItem_DropDownOpening(object sender, System.EventArgs e)
        {
            removeFavoriteToolStripMenuItem.Enabled = (favorites.Count > 0);
            addToFavoritesToolStripMenuItem.Enabled = (tvwKeys.SelectedNode != null);
            toolStripMenuSeperatorFavorites.Visible = (favorites.Count > 0);
        }


        private void addToFavoritesToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            AddToFavoritesDialog dialog = new AddToFavoritesDialog();
            Microsoft.Win32.RegistryKey key = (Microsoft.Win32.RegistryKey)tvwKeys.SelectedNode.Tag;
            int i = key.Name.LastIndexOf('\\');
            dialog.txtName.SelectedText = i >= 0 ? key.Name.Substring(i + 1) : key.Name;
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                if (favorites.ContainsKey(dialog.txtName.Text))
                    UIUtility.DisplayError(this, RegistryExplorer.Properties.Resources.Error_AlreadyFavorite);
                else
                {
                    string path = "My Computer\\" + key.Name;
                    Microsoft.Win32.Registry.SetValue(RegistryExplorer.Registry.RegistryExplorerr.RegistryFavoritePath
                        , dialog.txtName.Text
                        , path
                    );

                    favorites[dialog.txtName.Text] = key.Name;
                }
            }
        }


        private void removeFavoriteToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            RemoveFavoritesDialog dialog = new RemoveFavoritesDialog(favorites);
            dialog.ShowDialog(this);
        }


        private void cmbSearch_SelectedValueChanged(object sender, System.EventArgs e)
        {
            if (cmbSearch.Text == "All Hives")
            {
                txtBranch.Text = string.Empty;
                txtBranch.Enabled = false;
            }
            else
                txtBranch.Enabled = true;
        }   
        

    }


}