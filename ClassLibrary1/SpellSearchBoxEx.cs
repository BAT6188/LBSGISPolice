using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Text.RegularExpressions;

namespace SplitWord
{
    public class SpellSearchBoxEx : TextBox
    {
        #region Member
        private ListBox searchList;
        private string[] source;
        //private List<string> spellList;
        private int maxItemCount = 5;
        //private SearchMode mode = SearchMode.StartWith;
        #endregion

        public SpellSearchBoxEx()
        {
            searchList = new ListBox();
            searchList.Visible = false;

        }

        private  string[] SpellSearchSource
        {
            get { return source; }
            set
            {
                if (value != null && value.Length >0)
                {
                    source = value;
                    InitSourceSpell();
                    //base.TextChanged += new EventHandler(SpellSearchBoxEx_TextChanged);
                    LostFocus += new EventHandler(SpellSearchBoxEx_LostFocus);
                    searchList.KeyDown += new KeyEventHandler(searchList_KeyDown);
                    searchList.Click += new EventHandler(searchList_Click);
                    searchList.MouseMove += new MouseEventHandler(searchList_MouseMove);
                }
                else
                {
                    this.searchList.Visible = false;
                    //this.Text = "";
                }
            }
        }

        public int MaxItemCount
        {
            get { return maxItemCount; }
            set { maxItemCount = value; }
        }

        //public SearchMode SearchMode
        //{
        //    get { return mode; }
        //    set { mode = value; }
        //}


        //#region Method
        protected virtual void InitSourceSpell()
        {
            //if (spellList == null)
            //{
            //    spellList = new List<string>();
            //}
            //spellList.Clear();
            //foreach (string str in source)
            //{
            //    string s = SplitWord.GetChineseSpell(str);
            //    if (!s.Equals(string.Empty))
            //    {
            //        spellList.Add(s.ToUpper());
            //    }

            //}


            searchList.Items.Clear();

            for (int i = 0; i < source.Length; i++)
            {
                
                searchList.Items.Add(source[i]);
            }

            SetSearchBoxState();
        }

        //protected virtual void SpellSearchBoxEx_TextChanged(object sender, EventArgs e)
        //{
        //    searchList.Items.Clear();
        //    string str = this.Text;
        //    List<string> list = new List<string>();
        //    foreach (string var in spellList)
        //    {
        //        if (SearchMode == SearchMode.Contains)
        //        {
        //            if (var.IndexOf(str.ToUpper()) != -1)
        //            {
        //                list.Add(source[spellList.IndexOf(var)]);
        //                //searchList.Items.Add(source[spellList.IndexOf(var)]);
        //            }
        //        }
        //        else
        //        {
        //            if (var.ToUpper().StartsWith(str.ToUpper()))
        //            {
        //                for (int i = 0; i < spellList.Count; i++)
        //                {
        //                    if (spellList[i].Equals(var) && !list.Contains(source[i]))
        //                    {
        //                        list.Add(source[i]);
        //                       // searchList.Items.Add(source[i]);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    if (Regex.IsMatch(str, "[\u4e00-\u9fa5]+"))
        //    {

        //        foreach (string var in source)
        //        {
        //            if (SearchMode == SearchMode.Contains)
        //            {
        //                if (var.ToUpper().IndexOf(str.ToUpper()) != -1 && !list.Contains(var))
        //                {list.Add(var);
        //                   // searchList.Items.Add(var);
        //                }
        //            }
        //            else
        //            {
        //                if (var.ToUpper().StartsWith(str.ToUpper()) && !list.Contains(var))
        //                {
        //                    list.Add(var);
        //                    //searchList.Items.Add(var);
        //                }
        //            }
        //        }

        //    }
        //    searchList.Items.AddRange(list.ToArray());
        //    SetSearchBoxState();
        //    OnTextChanged(new TextChangedEventArgs(base.Text,list.ToArray()));
        //}
        //public System.Windows.Forms.ListBox.ObjectCollection Items
        //{
        //    get
        //    {
        //        return searchList.Items;
        //    }
        //}

        //#region Event
        //private readonly object textChange = new object();
        //public new event TextChangedEventHandle TextChanged
        //{
        //    add
        //    {
        //        Events.AddHandler(textChange, value);
        //    }
        //    remove
        //    {
        //        Events.RemoveHandler(textChange, value);
        //    }
        //}

        //protected virtual void OnTextChanged(TextChangedEventArgs e)
        //{
        //    TextChangedEventHandle handler = (TextChangedEventHandle)Events[textChange];
        //    if (handler != null)
        //    {
        //        handler(this, e);
        //    }
        //}
        //#endregion

        private void SetSearchBoxState()
        {
            if (searchList.Items.Count > 0)
            {
                searchList.BorderStyle = BorderStyle.FixedSingle;
                searchList.Height = ((searchList.Items.Count >= maxItemCount ? maxItemCount : searchList.Items.Count) + 1) * searchList.ItemHeight;
                searchList.Parent = this.Parent;
                searchList.Location = new System.Drawing.Point(this.Left, this.Bottom);
                searchList.Width = this.Width;
                searchList.BringToFront();
                searchList.Visible = true;
            }
            else
            {
                searchList.Visible = false;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Down && searchList.Visible && (searchList.SelectedIndex != searchList.Items.Count - 1))
            {
                searchList.SelectedIndex = searchList.SelectedIndex + 1;
                searchList.Focus();
            }


        }

        protected virtual void SpellSearchBoxEx_LostFocus(object sender, EventArgs e)
        {
            if (!searchList.Focused)
            {
                searchList.Visible = false;
                //this.Text = "";
            }
            //searchList .
        }

        protected virtual void searchList_MouseMove(object sender, MouseEventArgs e)
        {
            int index = searchList.IndexFromPoint(e.X, e.Y);
            if (index > -1 && index < searchList.Items.Count)
            {
                searchList.SelectedIndex = index;
            }
        }

        protected virtual void searchList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CommitSearchList();
            }
            else if (e.KeyCode == Keys.Up && searchList.SelectedIndex == 0)
            {
                searchList.SelectedIndex = -1;
                this.Focus();

            }
        }

        protected virtual void searchList_Click(object sender, EventArgs e)
        {
            CommitSearchList();
        }

        protected virtual void CommitSearchList()
        {
            if (searchList.SelectedIndex > -1)
            {
                this.Text = searchList.Items[searchList.SelectedIndex].ToString();
                searchList.Visible = false;
                this.Focus();
            }
        }

        public void GetSpellBoxSource(DataTable dt)
        {
            List<string> list = new List<string>();
            
            foreach (DataRow dr in dt.Rows)
            {
                if (!Convert.IsDBNull(dr[0]))
                    list.Add(dr[0].ToString());
            }
            SpellSearchSource = list.ToArray();
        }

        //public string[] GetSpellBoxSource(DataTable dt, string text)
        //{

        //    if (dt == null || !dt.Columns.Contains(text))
        //    {
        //        throw new Exception("不合法参数！");
        //    }
        //    List<string> list = new List<string>();
        //    foreach (DataRow dr in dt.Rows)
        //    {
        //        if (!Convert.IsDBNull(dr[text]))
        //            list.Add(dr[text].ToString());
        //    }
        //    return list.ToArray();
        //}
        //#endregion
    }
    //public enum SearchMode
    //{
    //    StartWith, Contains
    //}

    //public delegate void TextChangedEventHandle(object sender, TextChangedEventArgs e);
    //public class TextChangedEventArgs : EventArgs
    //{
    //    private List<string> items = null;

    //    public List<string> Items
    //    {
    //        get { return items; }
    //    }
    //    private string text = string.Empty;

    //    public string Text
    //    {
    //        get { return text; }
    //    }

    //    public TextChangedEventArgs(string text, string[] item)
    //    {
    //        this.text = text;
    //        this.items = new List<string>() ;
    //        this.items.AddRange(item);
    //    }

    
}
