using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;


namespace cl3Color
{
    public partial class frmReport : Form
    {
        public frmReport()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                Microsoft.Office.Interop.Excel.Application xls_exp = new Microsoft.Office.Interop.Excel.ApplicationClass();

                Microsoft.Office.Interop.Excel._Workbook xls_book = xls_exp.Workbooks._Open(@"D:\Sample.xls", Missing.Value, Missing.Value, Missing.Value, Missing.Value
                 , Missing.Value, Missing.Value, Missing.Value, Missing.Value
                 , Missing.Value, Missing.Value, Missing.Value, Missing.Value);
               
                Microsoft.Office.Interop.Excel._Worksheet xls_sheet = (Microsoft.Office.Interop.Excel.Worksheet)xls_book.ActiveSheet;
                Object oMissing = System.Reflection.Missing.Value;  //实例化对象时缺省参数 
              

                DateTime fromTime = new DateTime(2008, 7, 10);
                DateTime endTime = new DateTime(2008, 7, 20);
                clsCal3Color cl3 = new clsCal3Color(fromTime, endTime);
               
                Microsoft.Office.Interop.Excel.Range rng3;


                string[] strArr ={ "", "大良", "容桂", "伦教", "北滘", "陈村", "乐从", "龙江", "勒流", "杏坛", "均安" };
                for (int i = 2; i < 13; i++) {
                    string name = strArr[i - 2];
                    if (name == "")
                    {
                        cl3.getData();
                    }
                    else
                    {
                        cl3.getData(name);
                    }
                    int iRow = i * 2 - 1;
                    rng3 = xls_sheet.get_Range("B" + iRow.ToString(), Missing.Value);
                    if (cl3.BFBA0 <= 0.0)
                    {
                        rng3.Interior.Color = ColorTranslator.ToWin32(Color.Green);
                    }
                    else if (cl3.BFBA0 > 0.0 && cl3.BFBA0 <10.0)
                    {
                        rng3.Interior.Color = ColorTranslator.ToWin32(Color.Yellow);
                    }
                    else
                    {
                        rng3.Interior.Color = ColorTranslator.ToWin32(Color.Red);
                    }
                    rng3.Value2 = string.Format("{0:0.0}", cl3.BFBA0) + "%";

                    xls_sheet.Cells[iRow, 4] = cl3.XAZS.ToString();
                    xls_sheet.Cells[iRow, 5] = cl3.DQQC.ToString();
                    xls_sheet.Cells[iRow, 6] = cl3.DMTC.ToString();
                    xls_sheet.Cells[iRow, 7] = cl3.LRDQ.ToString();
                    xls_sheet.Cells[iRow, 8] = cl3.FCQD.ToString();
                    xls_sheet.Cells[iRow, 9] = cl3.YBQD.ToString();
                    xls_sheet.Cells[iRow, 10] = cl3.QJ.ToString();
                    xls_sheet.Cells[iRow, 11] = cl3.ZP.ToString();
                    xls_sheet.Cells[iRow, 12] = cl3.ZAAJ.ToString();

                    xls_sheet.Cells[iRow + 1, 4] = string.Format("{0:0.0}", cl3.BFBXAZS) + "%";
                    xls_sheet.Cells[iRow + 1, 5] = string.Format("{0:0.0}", cl3.BFBDQQC) + "%";
                    xls_sheet.Cells[iRow + 1, 6] = string.Format("{0:0.0}", cl3.BFBDMTC) + "%";
                    xls_sheet.Cells[iRow + 1, 7] = string.Format("{0:0.0}", cl3.BFBLRDQ) + "%";
                    xls_sheet.Cells[iRow + 1, 8] = string.Format("{0:0.0}", cl3.BFBFCQD) + "%";
                    xls_sheet.Cells[iRow + 1, 9] = string.Format("{0:0.0}", cl3.BFBYBQD) + "%";
                    xls_sheet.Cells[iRow + 1, 10] = string.Format("{0:0.0}", cl3.BFBQJ) + "%";
                    xls_sheet.Cells[iRow + 1, 11] = string.Format("{0:0.0}", cl3.BFBZP) + "%";
                    xls_sheet.Cells[iRow + 1, 12] = string.Format("{0:0.0}", cl3.BFBZAAJ) + "%";
                }
              

                xls_exp.Visible = true;
                int generation = 0;
                xls_exp.UserControl = false;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xls_exp);
                generation = System.GC.GetGeneration(xls_exp);

                xls_exp = null;
                xls_book = null;
                xls_sheet = null;

               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            MessageBox.Show("ok");

            Cursor.Current = Cursors.Default;
          



        }
    }
}