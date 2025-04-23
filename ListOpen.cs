using System.Collections.Generic;
using Kingdee.BOS;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List;

namespace KD.MIRACLE.PLUGIN
{
    [Description("销售订单列表打开动态表单")]
    [Kingdee.BOS.Util.HotUpdate]

    //继承表单插件
    public class ListOpen : AbstractListPlugIn
    {
        string formId = "k7f873c3f69db4c10ae642e6aa10280d6";
        public override void BarItemClick(Kingdee.BOS.Core.DynamicForm.PlugIn.Args.BarItemClickEventArgs e)
        {//点击按钮事件
            base.BarItemClick(e);

            //当点击这个按钮,触发
            if (e.BarItemKey.Equals("SCS_Button_BatchModify"))
            {   //调用,动态表单

                IListView listView = (IListView)this.View;
                ListSelectedRowCollection listRows = listView.SelectedRowsInfo;

                if (listRows.Count == 0)
                {
                    this.ListView.ShowMessage("没有选择任何行，请重新选择！");
                    return;
                }

                List<string> fidList = new List<string>();
                List<string> fentryIdList = new List<string>();

                foreach (ListSelectedRow row in listRows)
                {
                    IDataRow dataRow = row.DataRow;

                    // 获取单据头ID（FID）
                    string FID = dataRow["FID"].ToString();

                    // 获取分录ID（FENTRYID）
                    string FENTRYID = dataRow["t2_FENTRYID"].ToString();

                    fidList.Add(FID);
                    fentryIdList.Add(FENTRYID);
                }

                DynamicFormShowParameter param = new DynamicFormShowParameter
                {
                    FormId = formId,
                    ParentPageId = this.View.PageId,
                    CustomComplexParams =
                    {
                        { "FIDList", fidList },      // 传递单据头ID集合
                        { "FEntryIDList", fentryIdList } // 传递分录ID集合
                    }
                };

                // 显示动态表单
                this.View.ShowForm(param);
            }
        }
    }
}
