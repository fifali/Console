using System.Collections.Generic;
namespace ConsoleHydee
{
    public class ObjectList
    {
        #region 请求体-B2B
        public class ReqData_B2BList
        {
            public string orderId { get; set; }//订单编号
            public string buyerCode { get; set; }//客户编号
            public string payType { get; set; }//线上线下付款方式",0线下付款  1线上付款  默认为0
            public string addressId { get; set; }//送货地点id
            public string agentId { get; set; }//委托人id
            public string salerId { get; set; }//业务员id
            public List<ProductList> productList { get; set; }
        }
        #endregion

        #region 返回体-B2B
        public class RetData_B2BList
        {
            public string returnCode { get; set; }//0失败  1成功
            public string returnMsg { get; set; }//失败的情况下，返回失败信息
            public List<Data> data { get; set; }
        }
        #endregion

        #region 返回体Data-B2B
        public class Data
        {
            public string orderId { get; set; }//电商订单id
            public string erpOrderId { get; set; }//erp订单id
            public string buyerCode { get; set; }//客户编号
            public List<RProductList> productList { get; set; }
        }
        #endregion
    }
}