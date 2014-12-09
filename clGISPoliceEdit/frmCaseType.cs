using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace clGISPoliceEdit
{
    public partial class frmCaseType : Form
    {
        private string strConn = "";
        public frmCaseType(string s)
        {
            InitializeComponent();
            strConn = s;
            InitialComboxs();
        }

        private void InitialComboxs()
        {
            OracelData linkData = new OracelData(strConn);
            string sql = "select distinct(代码1),名称1 from 警综案别 t order by 代码1";
            try
            {
                DataTable dt = linkData.SelectDataBase(sql);
                comboBoxType1.Items.Add("全部");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    comboBoxType1.Items.Add(dt.Rows[i][0].ToString()+" "+dt.Rows[i][1].ToString());
                }
                comboBoxType1.Text = "全部";
            }
            catch (Exception ex)
            {
                ExToLog(ex, "InitialComboxs");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                comboBoxType1.Text = "全部";
                comboBoxType2.Text = "全部";
                textBoxKey.Text = "";
                dataGridView1.DataSource = null;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "button2_Click");
            }
        }

        private void comboBoxType1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strExp = "select distinct(代码2),名称2 from 警综案别 t ";
            if (comboBoxType1.Text != "全部")
            {
                strExp += " where 代码1='"+comboBoxType1.Text.Substring(0, 6)+"'";
            }
            strExp += " order by 代码2";

            OracelData linkData = new OracelData(strConn);
            comboBoxType2.Items.Clear();
            try
            {
                DataTable dt = linkData.SelectDataBase(strExp);
                comboBoxType2.Items.Add("全部");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    comboBoxType2.Items.Add(dt.Rows[i][0].ToString() + " " + dt.Rows[i][1].ToString());
                }
                comboBoxType2.Text = "全部";
            }
            catch (Exception ex)
            {
                ExToLog(ex, "comboBoxType1_SelectedIndexChanged");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                comboBoxType2.Enabled = checkBox1.Checked;
                checkBox2.Enabled = checkBox1.Checked;
                textBoxKey.Enabled = checkBox1.Checked;
                if (checkBox1.Checked == false)
                {
                    radioButton1.Enabled = true;
                    radioButton2.Enabled = true;
                    dataGridView1.AllowUserToAddRows = false;
                    dataGridView1.AllowUserToDeleteRows = false;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "checkBox1_CheckedChanged");
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                textBoxKey.Enabled = checkBox2.Checked;

                radioButton1.Enabled = !checkBox2.Checked;
                radioButton2.Enabled = !checkBox2.Checked;

                dataGridView1.DataSource = null;
                button3.Enabled = false;
                button4.Enabled = false;
                //setAddDel();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "checkBox2_CheckedChanged");
            }
        }

        private void setAddDel()
        {
            try
            {
                dataGridView1.AllowUserToAddRows = checkBox2.Checked;
                dataGridView1.AllowUserToDeleteRows = checkBox2.Checked;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setAddDel");
            }
        }

        OracleDataAdapter da = null;
        DataTable dt = null;
        private void button1_Click(object sender, EventArgs e)
        {
            string sql = "";
            if (checkBox1.Checked)
            {
                if (checkBox2.Checked)
                {
                    sql = "select * from 警综案别";

                    if (comboBoxType1.Text != "全部")
                    {
                        sql += " where 代码1='" + comboBoxType1.Text.Substring(0, 6) + "'";
                    }
                    if (comboBoxType2.Text != "全部")
                    {
                        if (comboBoxType1.Text == "全部")
                        {
                            sql += " where 代码2='" + comboBoxType2.Text.Substring(0, 6) + "'";
                        }
                        else
                        {
                            sql += " and 代码2='" + comboBoxType2.Text.Substring(0, 6) + "'";
                        }
                    }

                    if (textBoxKey.Text.Trim() == "") {
                        DialogResult dResult = MessageBox.Show("未输入关键词,是否查询全部?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if(dResult== DialogResult.No) return;
                    }
                    else{
                        if(sql.IndexOf("where")>-1){
                            sql+=" and 名称3 like '%"+textBoxKey.Text.Trim()+"%'";
                        }
                        else{
                            sql+=" where 名称3 like '%"+textBoxKey.Text.Trim()+"%'";
                        }
                    }
                }
                else
                {
                    sql = "select distinct(代码2),名称2 from 警综案别 t";
                    if (comboBoxType1.Text != "全部")
                    {
                        sql += " where 代码1='" + comboBoxType1.Text.Substring(0, 6) + "'";
                    }

                    if (comboBoxType2.Text != "全部")
                    {
                        if (comboBoxType1.Text == "全部")
                        {
                            sql += " where 代码2='" + comboBoxType2.Text.Substring(0, 6) + "'";
                        }
                        else
                        {
                            sql += " and 代码2='" + comboBoxType2.Text.Substring(0, 6) + "'";
                        }
                    }

                    sql += " order by 代码2";
                }

            }
            else {
                sql = "select distinct(代码1),名称1 from 警综案别 t";
                if (comboBoxType1.Text != "全部")
                {
                    sql += " where 代码1='" + comboBoxType1.Text.Substring(0, 6) + "'";
                }
                sql += " order by 代码1";
            }

            OracleConnection conn = new OracleConnection(strConn);
            try
            {
                da = new OracleDataAdapter(sql, conn);
                DataSet ds = new DataSet();
                da.Fill(ds, "nowTab");
                dt = ds.Tables["nowTab"];

                dataGridView1.DataSource = dt;

                conn.Close();

                //setAddDel();

                if (checkBox2.Enabled == false || (checkBox2.Enabled == true && checkBox2.Checked == false))
                {
                    radioButton1.Enabled = true;
                    radioButton2.Enabled = true;

                    dataGridView1.AllowUserToAddRows = false;
                    dataGridView1.AllowUserToDeleteRows = false;

                    setEditColumns();
                }
                else {
                    radioButton1.Enabled = false;
                    radioButton2.Enabled = false;

                    foreach (DataGridViewTextBoxColumn col in dataGridView1.Columns)
                    {
                        if (col.HeaderText.ToUpper() == "ID") {
                            col.ReadOnly = true;
                        }
                        else{
                            col.ReadOnly = false;
                        }
                    }

                    dataGridView1.AllowUserToAddRows = true;
                    dataGridView1.AllowUserToDeleteRows = true;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "button1_Click");
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            setEditColumns();
        }

        private void setEditColumns() {
            try
            {
                if (radioButton1.Checked)
                {
                    foreach (DataGridViewTextBoxColumn col in dataGridView1.Columns)
                    {
                        if (col.HeaderText.IndexOf("名称") > -1)
                        {
                            col.ReadOnly = false;
                        }
                        else
                        {
                            col.ReadOnly = true;
                        }
                    }
                }
                else
                {
                    foreach (DataGridViewTextBoxColumn col in dataGridView1.Columns)
                    {
                        if (col.HeaderText.IndexOf("代码") > -1)
                        {
                            col.ReadOnly = false;
                        }
                        else
                        {
                            col.ReadOnly = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setEditColumns");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(dataGridView1.Rows.Count<1) return;
            string sql = "";
            this.Cursor = Cursors.WaitCursor;
            OracleConnection conn = new OracleConnection(strConn);
            try
            {
                if (checkBox1.Checked && checkBox2.Checked)
                {
                    DataRow[] drArr = dt.Select("id is null");
                    if (drArr.Length > 0) {
                        conn.Open();
                        OracleCommand cmd = conn.CreateCommand();
                        cmd.CommandText = "select max(id) from 警综案别";
                        OracleDataReader datareader= cmd.ExecuteReader();
                        int max = 1;
                        if (datareader.HasRows) { 
                            datareader.Read();
                            max += Convert.ToInt32(datareader.GetValue(0));
                        }
                        for (int i = dt.Rows.Count-drArr.Length; i < dt.Rows.Count; i++)
                        {
                            if (dt.Rows[i]["id"].ToString() == "") {
                                dt.Rows[i]["id"] = max;
                                max++;
                            }
                        }
                    }
                    OracleCommandBuilder ocb = new OracleCommandBuilder(da);
                    try
                    {                        
                        da.Update(dt);                        
                    }
                    catch (Exception ex){
                        ExToLog(ex, "button3_Click");
                    }
                    conn.Close();

                    setControlsEnabled();
                    this.Cursor = Cursors.Default;
                    return;
                   
                }
                else
                { //只更新第一,二类
                    conn.Open();
                    OracleCommand cmd = null;
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (radioButton1.Checked)
                        {
                            sql = "update 警综案别 set " + dataGridView1.Columns[1].HeaderText + "='" + dataGridView1.Rows[i].Cells[1].Value.ToString() + "' where " + dataGridView1.Columns[0].HeaderText + "='" + dataGridView1.Rows[i].Cells[0].Value.ToString() + "'";
                        }
                        else
                        {
                            sql = "update 警综案别 set " + dataGridView1.Columns[0].HeaderText + "='" + dataGridView1.Rows[i].Cells[0].Value.ToString() + "' where " + dataGridView1.Columns[1].HeaderText + "='" + dataGridView1.Rows[i].Cells[1].Value.ToString() + "'";
                        }
                        cmd = new OracleCommand(sql, conn);
                        cmd.ExecuteNonQuery();
                    }
                    cmd.Dispose();
                }
                conn.Close();
                setControlsEnabled();
                this.Cursor = Cursors.Default;
            }
            catch(OracleException  ex) {
                if (ex.Code == 1407) {
                    MessageBox.Show("有空值,不能提交更改,请补充!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                this.Cursor = Cursors.Default;
                ExToLog(ex, "button3_Click");
            }
        }

        private void setControlsEnabled()
        {
            try
            {
                button3.Enabled = false;
                button4.Enabled = false;
                if (checkBox2.Enabled == false || (checkBox2.Enabled == true && checkBox2.Checked == false))
                {
                    radioButton1.Enabled = true;
                    radioButton2.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setControlsEnabled");
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                DataSet ds = new DataSet();
                da.Fill(ds, "temp");
                dt = ds.Tables[0];
                dataGridView1.DataSource = dt;
                setControlsEnabled();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "button4_Click");
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                button3.Enabled = true;
                button4.Enabled = true;
                if (checkBox2.Enabled == false || (checkBox2.Enabled == true && checkBox2.Checked == false))
                {
                    radioButton1.Enabled = false;
                    radioButton2.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "dataGridView1_CellValueChanged");
            }
        }

        private void ToolStripMenuItemDelRow_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow.IsNewRow == false)
                {
                    dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
                    button3.Enabled = true;
                    button4.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "ToolStripMenuItemDelRow_Click");
            }

        }

        private void dataGridView1_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
        {
            try
            {
                if (dataGridView1.AllowUserToDeleteRows)
                {
                    if (e.ColumnIndex < 0)
                    {//如果在行头上右键
                        if (e.RowIndex == dataGridView1.SelectedRows[0].Index)  //如果在选择中行上右击
                        {
                            e.ContextMenuStrip = contextMenuStrip1;
                        }
                    }
                }
            }
            catch(Exception ex) {
                ExToLog(ex, "dataGridView1_CellContextMenuStripNeeded");
            }
        }

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFuns">函数名</param>
        private void ExToLog(Exception ex, string sFuns)
        {
            CLC.BugRelated.ExceptionWrite(ex, "LBSgisPoliceEdit-frmCaseType-" + sFuns);
        }
    }
}