using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace clVideo
{
    public partial class frmrecord : Form
    {

       private string Camid = string.Empty;
       private string[] _conStr;
       private string user;

        public frmrecord()
        {
            InitializeComponent();
           
        }

        private void button29_Click(object sender, EventArgs e)
        {
            this.SearchVideo(Camid);          
        }

        /// <summary>
        /// 窗体加载
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void frmrecord_Load(object sender, EventArgs e)
        {
            try
            {
                int Icreate = this.axBabyOnline1.CreateInstance();
                if (Icreate == 0)
                {
                }
                else
                {
                    return;
                }

                this.btnDown.Enabled = false;
                this.btnPlay.Enabled = false;
                this.btnStop.Enabled = false;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "frmrecord_Load");
            }

        }


        private string CamName = string.Empty; 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="emsIp"></param>
        /// <param name="emsPort"></param>
        /// <param name="emsURI"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="camid"></param>
        /// <param name="_strCon">连接数据库参数</param>
        /// <param name="userr"></param>
        public void Initial(string emsIp, string emsPort, string emsURI, string username, string password, string camid, string[] _strCon,string userr)
        {
            try
            {
                this.Camid = camid;
                //this.listBox1.Items.Clear();
                this._conStr = _strCon;
                this.user = userr;
                this.CamName = this.GetNameFromID(camid);
                this.CreatTab();//创建公共变量 临时表
                if (lResult == 1)
                {
                    Boolean LoginFlag = this.LogVideo(emsIp, emsPort, emsURI, username, password);
                    if (LoginFlag)
                    {

                    }
                    else
                    {
                        MessageBox.Show("登录失败", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "Initial");
            }
        }


        DataTable temEditDt = new DataTable();

        private void CreatTab()
        {            
            DataColumn dc = new DataColumn("序列");
            dc.DataType = System.Type.GetType("System.String");
            temEditDt.Columns.Add(dc);

            //列         edit by fisher in 10-02-26
            dc = new DataColumn("录像名称");
            dc.DataType = System.Type.GetType("System.String");
            temEditDt.Columns.Add(dc);

            dc = new DataColumn("录像ID");
            dc.DataType = System.Type.GetType("System.String");
            temEditDt.Columns.Add(dc);

            dc = new DataColumn("摄像机名称");
            dc.DataType = System.Type.GetType("System.String");
            temEditDt.Columns.Add(dc);

            dc = new DataColumn("文件大小");
            dc.DataType = System.Type.GetType("System.String");
            temEditDt.Columns.Add(dc);

            dc = new DataColumn("起始时间");
            dc.DataType = System.Type.GetType("System.String");
            temEditDt.Columns.Add(dc);

            dc = new DataColumn("终止时间");
            dc.DataType = System.Type.GetType("System.String");
            temEditDt.Columns.Add(dc);

            dc = new DataColumn("录像SID");
            dc.DataType = System.Type.GetType("System.String");
            temEditDt.Columns.Add(dc);
        }
        private string GetNameFromID(string id)
        {
            string name = string.Empty;
            Writelog("摄像头id" + id);
            try
            {
                DataTable dt = GetTable("Select 设备名称 from 视频位置VIEW where 设备编号='" + id + "'");

                Writelog("SQL" + "Select 设备名称 from 视频位置VIEW where 设备编号='" + id + "'");
                if (dt != null && dt.Rows.Count > 0)
                    name = dt.Rows[0][0].ToString();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "GetNameFromID");
            }
            Writelog("摄像头名称" + name);
            return name;
        }

        private int lResult=1;
        private Boolean LogVideo(string emsIp,string emsPort ,string emsURI,string username,string password)
        {
            try
            {
                lResult = this.axBabyOnline1.Login(emsIp, emsPort, emsURI, username, password);
                if (lResult == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex) { ExToLog(ex, "LogVideo"); return false; }
        }

        VideoStru[] VideArray = new VideoStru[10];

        private void SearchVideo(string camid)
        {
            try
            {
                string QueryString = this.axBabyOnline1.QueryStorageFile("", camid, this.dateTimePicker1.Value.ToString(), this.dateTimePicker2.Value.ToString());

                //string QueryString = "{file:[{\"taskName\":\"xxx3\",\"taskId\":\"366725175\",\"filepath\":\"/home/sa2119/volume0/366725175_2010-07-01-15-30-00-053.hik\",\"startTime\":\"2010-07-01 15:30:00\",\"endTime\":\"2010-07-01 15:31:10\",\"size\":\"1552736\",\"saId\":\"545AA2CB06527A8115726452DE25C3B2\"},{\"taskName\":\"dddd3\",\"taskId\":\"777408838\",\"filepath\":\"/home/sa2119/volume0/777408838_2010-07-01-15-24-00-051.hik\",\"startTime\":\"2010-07-01 15:24:00\",\"endTime\":\"2010-07-01 15:25:10\",\"size\":\"1548100\",\"saId\":\"545AA2CB06527A8115726452DE25C3B2\"},{\"taskName\":\"bbbbbbbb\",\"taskId\":\"747935506\",\"filepath\":\"/home/sa2119/volume0/747935506_2010-06-30-17-58-00-052.hik\",\"startTime\":\"2010-06-30 17:58:00\",\"endTime\":\"2010-06-30 17:59:10\",\"size\":\"1550876\",\"saId\":\"545AA2CB06527A8115726452DE25C3B2\"},{\"taskName\":\"fff\",\"taskId\":\"336685660\",\"filepath\":\"/home/sa2119/volume0/336685660_2010-06-30-13-51-00-060.hik\",\"startTime\":\"2010-06-30 13:51:00\",\"endTime\":\"2010-06-30 13:52:10\",\"size\":\"4572328\",\"saId\":\"545AA2CB06527A8115726452DE25C3B2\"},{\"taskName\":\"xxx\",\"taskId\":\"164410295\",\"filepath\":\"/home/sa2119/volume0/164410295_2010-06-30-11-10-00-051.hik\",\"startTime\":\"2010-06-30 11:10:00\",\"endTime\":\"2010-06-30 11:12:10\",\"size\":\"8553688\",\"saId\":\"545AA2CB06527A8115726452DE25C3B2\"},{\"taskName\":\"qqqq\",\"taskId\":\"919628951\",\"filepath\":\"/home/sa2119/volume0/919628951_2010-06-30-10-00-00-052.hik\",\"startTime\":\"2010-06-30 10:00:00\",\"endTime\":\"2010-06-30 10:02:10\",\"size\":\"7048212\",\"saId\":\"545AA2CB06527A8115726452DE25C3B2\"},{\"taskName\":\"222\",\"taskId\":\"681571709\",\"filepath\":\"/home/sa2119/volume0/681571709_2010-06-29-16-35-00-054.hik\",\"startTime\":\"2010-06-29 16:35:00\",\"endTime\":\"2010-06-29 16:37:10\",\"size\":\"7457748\",\"saId\":\"545AA2CB06527A8115726452DE25C3B2\"},{\"taskName\":\"1111\",\"taskId\":\"214835771\",\"filepath\":\"/home/sa2119/volume0/214835771_2010-06-29-16-30-00-051.hik\",\"startTime\":\"2010-06-29 16:30:00\",\"endTime\":\"2010-06-29 16:32:10\",\"size\":\"7325600\",\"saId\":\"545AA2CB06527A8115726452DE25C3B2\"},{\"taskName\":\"测试628_1\",\"taskId\":\"933108570\",\"filepath\":\"/home/sa2119/volume0/933108570_2010-06-28-16-18-00-190.hik\",\"startTime\":\"2010-06-28 16:18:00\",\"endTime\":\"2010-06-28 16:20:10\",\"size\":\"7207308\",\"saId\":\"545AA2CB06527A8115726452DE25C3B2\"},{\"taskName\":\"100624测试\",\"taskId\":\"631480099\",\"filepath\":\"/home/sa2119/volume0/631480099_2010-06-24-18-25-00-552.hik\",\"startTime\":\"2010-06-24 18:25:00\",\"endTime\":\"2010-06-24 18:27:10\",\"size\":\"5397824\",\"saId\":\"545AA2CB06527A8115726452DE25C3B2\"}]}";


                WriteEditLog("摄像机名称:" + this.CamName + "摄像机id:" + this.Camid + " 起始时间:" + this.dateTimePicker1.Value.ToString() + " 终止时间:" + this.dateTimePicker2.Value.ToString(), "录像查询", "视频监控");

                string[] fileArray = QueryString.Split('}');

                //this.listBox1.Items.Clear();

                if (QueryString.Length < 1 || fileArray.Length < 1)
                {
                    MessageBox.Show("没有查询到相应的录像信息", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if(this.temEditDt !=null && this.temEditDt.Columns.Count>0 && this.temEditDt.Rows.Count >0)
                  this.temEditDt.Rows.Clear();
              

                VideArray = new VideoStru[fileArray.Length];

                for (int i = 0; i < fileArray.Length; i++)
                {
                    string[] simFile = fileArray[i].Split(',');

                    string taskname = string.Empty;
                    string taskid = string.Empty;
                    string filepath = string.Empty;
                    string starttime = string.Empty;
                    string endtime = string.Empty;
                    string filesize = string.Empty;
                    string said = string.Empty;                   

                    for (int j = 0; j < simFile.Length; j++)
                    {
                        if (simFile[j].IndexOf("taskName") > -1)
                        {
                            taskname = simFile[j].Substring(simFile[j].LastIndexOf(':'), simFile[j].Length - simFile[j].LastIndexOf(':')).Replace('\\', ' ').Replace(':', ' ').Replace('"', ' ').Trim();
                        }

                        if (simFile[j].IndexOf("taskId") > -1)
                        {
                            taskid = simFile[j].Substring(simFile[j].LastIndexOf(':'), simFile[j].Length - simFile[j].LastIndexOf(':')).Replace('\\', ' ').Replace(':', ' ').Replace('"', ' ').Trim();
                        }

                        if (simFile[j].IndexOf("filepath") > -1)
                        {
                            filepath = simFile[j].Substring(simFile[j].LastIndexOf(':'), simFile[j].Length - simFile[j].LastIndexOf(':')).Replace('\\', ' ').Replace(':', ' ').Replace('"', ' ').Trim();
                        }                        

                        if (simFile[j].IndexOf("startTime") > -1)
                        {
                            starttime = simFile[j].Substring(simFile[j].IndexOf(':') + 1, simFile[j].Length - simFile[j].IndexOf(':') - 1).Replace('\\', ' ').Replace('"', ' ').Trim();
                        }

                        if (simFile[j].IndexOf("endTime") > -1)
                        {
                            endtime = simFile[j].Substring(simFile[j].IndexOf(':') + 1, simFile[j].Length - simFile[j].IndexOf(':') - 1).Replace('\\', ' ').Replace('"', ' ').Trim();
                        }

                        if (simFile[j].IndexOf("size") > -1)
                        {
                            filesize = simFile[j].Substring(simFile[j].LastIndexOf(':'), simFile[j].Length - simFile[j].LastIndexOf(':')).Replace('\\', ' ').Replace(':', ' ').Replace('"', ' ').Trim();
                        }

                        if (simFile[j].IndexOf("saId") > -1)
                        {
                            said = simFile[j].Substring(simFile[j].LastIndexOf(':'), simFile[j].Length - simFile[j].LastIndexOf(':')).Replace('\\', ' ').Replace(':', ' ').Replace('"', ' ').Trim();
                        }                       
                    }

                    if (taskname != "" && taskid != "" && filepath != "")
                    {
                        VideArray[i] = new VideoStru(taskname, taskid, filepath, starttime, endtime, filesize, said);

                        DataRow dRow;
                        dRow = temEditDt.NewRow();
                        dRow[0] = i;
                        dRow[1] = taskname;
                        dRow[2] = taskid;
                        dRow[3] = CamName;
                        dRow[4] = filesize;
                        dRow[5] = starttime;
                        dRow[6] = endtime;
                        dRow[7] = said;
                        temEditDt.Rows.Add(dRow);

                        //this.listBox1.Items.Add(i.ToString() + "," + taskname + "," + taskid + "," + this.CamName + "," + filesize + "," + starttime + "," + endtime + "," + said);
                    }
                }

                if (temEditDt !=null)
                    this.dataGridView1.DataSource = temEditDt;
            }
            catch (Exception ex) { ExToLog(ex, "SearchVideo"); }
        }


        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                int r = axBabyOnline1.StopVodFile();
                if (r == 0)
                {
                    this.btnPlay.Enabled = true;
                }
                else
                    MessageBox.Show("播放失败");
            }
            catch (Exception ex) { ExToLog(ex, "btnStop_Click"); }
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            try
            {

                if(this.dataGridView1.CurrentRow == null) return ;

                //string[] filestring = this.listBox1.SelectedItem.ToString().Split(',');
                //this.axBabyOnline1.DownloadStorageFile(filestring[2], filestring[3], "");

                int i = Convert.ToInt32(this.dataGridView1.CurrentRow.Cells[0].Value);

                this.axBabyOnline1.DownloadStorageFile(VideArray[i]._taskid,VideArray[i]._filepath,"");

                //WriteEditLog("摄像机名称:" + this.CamName + "摄像机id:" + this.Camid + " 录像名称:" + filestring[1] + "录像地址:" + filestring[3], "录像下载", "视频监控");
            }
            catch (Exception ex) { ExToLog(ex, "btnDown_Click"); }
        }



        private void btnPlay_Click(object sender, EventArgs e)
        {
            try
            {
                //string[] filestring = this.listBox1.SelectedItem.ToString().Split(',');
                //string filepath = filestring[3];
                //string said = filestring[7];

                if (this.dataGridView1.CurrentRow == null) return;

                int i = Convert.ToInt32(this.dataGridView1.CurrentRow.Cells[0].Value);

                string filepath = VideArray[i]._filepath;
                string said = VideArray[i]._said;

                int r = axBabyOnline1.PlayVodFile(said, filepath);
                if (r == 0)
                {
                    this.btnStop.Enabled = true;
                    spe = 1;
                }
                else
                {
                    MessageBox.Show("播放失败");
                    //spe = 0;
                }

                ShowTip();
            }
            catch (Exception ex) { ExToLog(ex, "btnPlay_Click"); }
        }

        private void ShowTip()
        {
            if (spe == 0.25)
                this.button1.Text = "1/4 速度播放";
            else if (spe == 0.5)
                this.button1.Text = "1/2 速度播放";
            else if (spe == 1)
                this.button1.Text = "正常 速度播放";
            else if (spe == 2)
                this.button1.Text = "2倍 速度播放";
            else if (spe == 4)
                this.button1.Text = "4倍 速度播放";
            else
                this.button1.Text = "播放错误";
        }

        float spe = 1;

        private void button2_Click(object sender, EventArgs e)
        {
            if (spe == 0.25)
            {
                MessageBox.Show("已是最小速度播放..","系统提示",MessageBoxButtons.OK,MessageBoxIcon.Information);
                return;
            }

            spe = (float)(spe / 2);

            long r = axBabyOnline1.SetFileSpeed(spe);           

            ShowTip();   
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (spe == 4)
            {
                MessageBox.Show("已是最大速度播放..", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            spe = (float)(spe * 2);

            long r = axBabyOnline1.SetFileSpeed(spe);           

            ShowTip();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (spe < 0.25 || spe > 4)
            {
                MessageBox.Show("播放速度设置错误..", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            long r = axBabyOnline1.SetFileSpeed(1);
            spe = 1;
            ShowTip();
        }


        private void frmrecord_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                this.Visible = false;
                try
                {
                    axBabyOnline1.StopVodFile();
                }
                catch { }
                e.Cancel = true;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "frmrecord_FormClosing");
            }
        }

        //private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (this.listBox1.SelectedItem.ToString() != "")
        //        {
        //            this.btnDown.Enabled = true;
        //            this.btnPlay.Enabled = true;
        //        }
        //    }
        //    catch (Exception ex) { ExToLog(ex, "listBox1_SelectedIndexChanged"); }
        //}


        //记录操作记录
        private void WriteEditLog(string sql, string method, string tablename)
        {
            try
            {
                string strExe = "insert into 操作记录 values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'视频监控:"+method+"','" + tablename + ":" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "WriteEditLog-记录操作记录");
            }
        }

        /// <summary>
        /// 查询SQL
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>DataTable</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(_conStr[0], _conStr[1], _conStr[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="sql">SQL语句</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(_conStr[0], _conStr[1], _conStr[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //string camid = "43E7520A27FEC73F8DE26B59611A4877";
            //string name = GetNameFromID(camid);

            //WriteEditLog("摄像机名称:"+name + "录像名称:" + "录像地址:", "录像下载", "视频监控");
        }


        private void Writelog(string msg)
        {
            try
            {
                string filepath = Application.StartupPath + "\\Videorec.log";
                msg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ff") + ":" + msg;

                StreamWriter sw = File.AppendText(filepath);
                sw.WriteLine(msg);
                sw.Flush();
                sw.Close();
            }
            catch(Exception ex)
            {
                ExToLog(ex, "Writelog");
            }
        }

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="sFunc"></param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clVideo-frmrecord-" + sFunc);
        }

        private void tabPage4_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                long r = axBabyOnline1.PauseFile();

                if (r == 0)
                    this.button1.Text ="暂停播放成功";
            }
            catch { }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
             try
            {
                if (this.dataGridView1.CurrentRow != null)
                {
                    this.btnDown.Enabled = true;
                    this.btnPlay.Enabled = true;
                }
            }
            catch (Exception ex) { ExToLog(ex, "dataGridView1_SelectionChanged"); }
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                if (this.dataGridView1.Rows.Count != 0)
                {
                    for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                    {
                        if ((i % 2) == 1)
                        {
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.WhiteSmoke;
                        }
                        else
                        {
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.LightBlue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "dataGridView1-16-设置列表背景颜色");
            }
        }
        

     
       
       
    }
}