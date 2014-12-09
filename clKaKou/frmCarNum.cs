using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using MapInfo.Windows.Controls;

namespace clKaKou
{
    public partial class frmCarNum : Form
    {
        public MapControl mapControl;   // 地图控件
        public string getFromNamePath;
        public string LayerName;
        public string[] StrCon;
        public string userName;
        public string strConn;

        public frmCarNum(MapControl mapCon, string[] stCon,string getformPath,string layerName,string user)
        {
            InitializeComponent();

            this.mapControl = mapCon;
            this.getFromNamePath = getformPath;
            this.LayerName = layerName;
            this.StrCon = stCon;
            this.userName = user;
            this.strConn = "data source =" + StrCon[0] + ";user id =" + StrCon[1] + ";password=" + StrCon[2];
        }

        /// <summary>
        /// 添加数据
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-3-2
        /// </summary>
        /// <param name="table">数据源</param>
        /// <param name="tableName">表名</param>
        public void setfrmInfo(DataTable table)
        {
            try
            {
                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                this.dataGridView1.Rows.Clear();

                foreach (DataRow row in table.Rows)
                {
                    this.dataGridView1.Rows.Add(new object[] { row[2] + ":",row[1], row[3] });
                }

                setSize();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setfrmInfo");
            }
        }

        /// <summary>
        /// 设置窗体大小
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-3-2
        /// </summary>
        private void setSize()
        {
            try
            {
                for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
                {
                    if (this.dataGridView1.Columns[i].Width > 300)
                    {
                        this.dataGridView1.Columns[2].Width = 300;
                        this.dataGridView1.Columns[2].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
                    }
                }

                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    double width = this.dataGridView1.Columns[2].DefaultCellStyle.Font.Size + 2;

                    int n = this.dataGridView1.Rows[i].Cells[2].Value.ToString().Length;
                    if (width * n > 195)
                    {
                        n = (int)(width * n);
                        double d = n / 300.0;
                        n = (int)Math.Ceiling(d) + 1;

                        this.dataGridView1.Rows[i].Height = (this.dataGridView1.Rows[i].Height - 6) * n;
                    }
                }

                int cMessageWidth = 0;

                cMessageWidth = this.dataGridView1.Columns[0].Width + this.dataGridView1.Columns[1].Width + this.dataGridView1.Columns[2].Width + 30;

                if (this.dataGridView1.Columns[1].Width == 300)
                {
                    cMessageWidth = this.dataGridView1.Columns[0].Width + 330;
                }

                int cMessageHeight = 0;
                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    cMessageHeight += this.dataGridView1.Rows[i].Height;
                }
                cMessageHeight += 50;
                this.Size = new Size(cMessageWidth, cMessageHeight);  //设置大小
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setSize");
            }
        }

        /// <summary>
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-3-2
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFuns">函数名</param>
        private void ExToLog(Exception ex, string sFuns)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clKaKou-frmCarNum-" + sFuns);
        }

        /// <summary>
        /// 单击事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-3-2
        /// </summary>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            System.Data.OracleClient.OracleConnection oraconn = new System.Data.OracleClient.OracleConnection("User ID=sdgis_cx;Password=!giscx123;" +
                                                                                                              "Data Source=(DESCRIPTION=(ADDRESS_LIST=" +
                                                                                                              "(ADDRESS=(PROTOCOL=TCP)(HOST=10.47.227.29)" +
                                                                                                              "(PORT=1521)))(CONNECT_DATA=(SID=sdgazyk)))");
            //System.Data.OracleClient.OracleConnection oraconn = new System.Data.OracleClient.OracleConnection(strConn);

            if (oraconn.State == ConnectionState.Open)
                oraconn.Close();

            try
            {
                //                                                                                         暂住所详细地址	暂住邮政编码	所有权	初次登记日期	最近定检日期	检验有效期止	强制报废期止	发证机关	管理部门	发牌日期	发行驶证日期	发登记证书日期	发合证日期	保险止日期	补领号牌次数	补领行驶证次数	补_换领证书次数	登记证书编号	制登记证书行数	档案编号	管理辖区	机动车状态	自定义状态	抵押标志	经办人	车辆来源	注册流水号	发动机型号	燃料种类	排量	功率	转向形式	车廓	车外廓宽	车外廓高	货箱内部长度	货箱内部宽度	货箱内部高度	钢板弹簧片数	轴数	轴距	前轮距	后轮距	轮胎规格	轮胎数	总质量	整备质量	核定载质量	核定载客人数	准牵引质量	驾驶室前排载客人数	驾驶室后排载客人数	环保达标情况	出厂日期	获得方式	来历凭证1	凭证编号1	来历凭证2	凭证编号2	销售单位	销售价格	销售日期	进口凭证	进凭证编号	合格证编号	纳税证明	纳税证明编号																											
                //XH as 机动车序号,HPZL as 号牌种类,	HPHM,号牌号码,	CLPP1 as 中文品牌,	CLXH as 车辆型号,	CLPP2 as 英文品牌,	GCJK as 过程_进口,	ZZG as 制造国,,	ZZCMC as 制造厂名称 ,	CLSBDH as 车辆识别代号 ,	FDJH as 发动机号,
                // CLLX 车辆类型,	CSYS 车身颜色,	SYXZ 使用性质,	SFZMHM 身份证明号码,	SFZMMC身份证明种类名称,	SYR机动车所有人,	ZSXZQH住所行政区划,	ZSXXDZ登记住所详细地址,	YZBM1住所邮政编码,	LXDH联系电话,	ZZZ居住暂住证明,
                //ZZXZQH暂住行政区划,	ZZXXDZ	YZBM2	SYQ	CCDJRQ	DJRQ	YXQZ	QZBFQZ	FZJG	GLBM	FPRQ	FZRQ	FDJRQ	FHGZRQ	BXZZRQ	BPCS	BZCS	BDJCS	DJZSBH	ZDJZSHS	DABH	XZQH	ZT	ZDYZT	DYBJ	JBR	CLLY	LSH	FDJXH	RLZL	PL	GL	ZXXS	CWKC	CWKK	CWKG	HXNBCD	HXNBKD	HXNBGD	GBTHPS	ZS	ZJ	QLJ	HLJ	LTGG	LTS	ZZL	ZBZL	HDZZL	HDZK	ZQYZL	QPZK	HPZK	HBDBQK	CCRQ	HDFS	LLPZ1	PZBH1	LLPZ2	PZBH2	XSDW	XSJG	XSRQ	JKPZ	JKPZHM	HGZBH	NSZM	NSZMBH	GXRQ	XGZL	QMBH	HMBH	BZ	JYW	WFCS	LJJF	SGCS	LJJJSS	CLYT	YTSX	DZYX	XSZBH	SJHM	JYHGBZBH	CYRY	DPHGZBH	SQDM	YXH	TYBIP	AZDM	ZDRUSJ	BJRUSJ	CSIP	CSBS	RMJYW

                string fields = "XH as 机动车序号,HPZL as 号牌种类,	HPHM as 号牌号码,	CLPP1 as 中文品牌,	CLXH as 车辆型号,	CLPP2 as 英文品牌,	GCJK as 过程_进口,	ZZG as 制造国,	ZZCMC as 制造厂名称 ,	CLSBDH as 车辆识别代号 ,	FDJH as 发动机号," +
                "CLLX as 车辆类型,	CSYS as  车身颜色,	SYXZ as 使用性质,	SFZMHM as 身份证明号码,	SFZMMC as 身份证明种类名称,	SYR as 机动车所有人,	ZSXZQH as 住所行政区划,	ZSXXDZ as 登记住所详细地址,	YZBM1 as 住所邮政编码,	LXDH as 联系电话,	ZZZ as 居住暂住证明 ";
                oraconn.Open();
                string cpStr = this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();   //  号牌号码
                string hpStr = this.dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();   //  号牌种类

                cpStr = cpStr.Substring(0, cpStr.Length - 1);

                OracleCommand cmd = new OracleCommand("Select " + fields + " from gis_vehicle where HPHM='" + cpStr + "' and HPZL='" + hpStr + "'", oraconn);
                OracleDataAdapter Adp = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                Adp.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    frmCarInfo frmcar = new frmCarInfo();
                    frmcar.setInfo(dt.Rows[0], this.StrCon, this.userName);
                    frmcar.mapControl = this.mapControl;
                    frmcar.mysql = this.strConn;
                    frmcar.layerName = this.LayerName;
                    frmcar.getFromNamePath = this.getFromNamePath;
                    frmcar.TopMost = true;
                    frmcar.Visible = false;
                    frmcar.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "dataGridView1_CellContentClick-查看车辆");
            }
            finally
            {
                if (oraconn.State == ConnectionState.Open)
                    oraconn.Close();
            }
        }
    }
}