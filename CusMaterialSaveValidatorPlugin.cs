using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS.ServiceHelper;
using System.ComponentModel;
using Kingdee.BOS.App.Data;
using System.Data;

namespace KD.MIRACLE.PLUGIN
{
    [Description("销售订单保存校验：校验客户物料对应表是否分发到库存组织")]
    [Kingdee.BOS.Util.HotUpdate]
    public class CusMaterialSaveValidatorPlugin : AbstractOperationServicePlugIn
    {
        DataTable dt;
        //加载
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            //物料编码
            e.FieldKeys.Add("FMATERIALID");
            //客户物料编码
            e.FieldKeys.Add("FMAPID");
            //销售组织
            e.FieldKeys.Add("FSALEORGID");
            //库存组织
            e.FieldKeys.Add("FSTOCKORGID");

        }

        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            base.BeforeExecuteOperationTransaction(e);

            //选择的行循环读取单据体 FSaleOrderEntry
            foreach (ExtendedDataEntity extended in e.SelectedRows)
            {                
                int index = 0;
                DynamicObject dy = extended.DataEntity;
                string FSALEORGID = dy["SaleOrgId_Id"].ToString();
                string FCUSTID = dy["CustId_Id"].ToString();

                DynamicObjectCollection dynamics = dy["SaleOrderEntry"] as DynamicObjectCollection;

                //出错的行集合
                List<int> errLine = new List<int>();

                foreach (DynamicObject sale in dynamics)
                {
                    index++;
                    string FMATERIALID = sale["MaterialId_Id"].ToString();
                    string CUSMATERIALID = sale["MapId_Id"].ToString();
                    //库存组织ID
                    string FSTOCKORGID = sale["StockOrgId_Id"].ToString();

                    //客户物料编码不为空
                    if (CUSMATERIALID != null && CUSMATERIALID != "")
                    {
                        //库存组织ID不等于销售组织ID
                        if (FSTOCKORGID != FSALEORGID)
                        {
                            string selectSql = @"/*dialect*/
                                    SELECT s1.FID 
                                    FROM t_Sal_CustMatMapping s1
                                    inner join t_Sal_CustMatMappingEntry s2 on s1.FID = s2.FID
                                    inner join T_BD_MATERIAL m1 on s2.FMATERIALID = m1.FMATERIALID 
                                    inner join T_V_SAL_CUSTMATMAPPING v1 on v1.fheadfid = s1.FID and s2.FMATERIALID = v1.FMATERIALID
                                    inner join T_SAL_CUSTMATMAPPING_ISSUE issue on s1.FID = issue.FID
                                    where m1.FMATERIALID = '" + FMATERIALID + "' and v1.fid = '" + CUSMATERIALID + "' and issue.FISSUEORGID = '" + FSTOCKORGID + "' and s1.FCUSTOMERID = '" + FCUSTID + "'";
                            dt = DBUtils.ExecuteDataSet(this.Context, selectSql).Tables[0];

                            //查询不到该记录，将该行号存入集合
                            if (dt.Rows.Count <= 0)
                            {
                                errLine.Add(index);
                            }
                        }
                    }
                }

                //循环结束，若存在出错行数，则抛出异常
                if (errLine.Count > 0)
                {
                    string lineList = string.Join("、", errLine);//1、2、3、4、5
                    throw new KDBusinessException("", $@"第{lineList}行物料在客户物料对应表中不存在或未分发到相应库存组织！");
                }
            }
        }
    }
}
