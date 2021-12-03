using System.Runtime.Serialization;
using System.Collections.Generic;
namespace ConsoleHydee
{
    [DataContract]
    #region B2B请求消息明细
    public class ProductList
    {
        [DataMember(Order = 0)]
        public string orderItemId { get; set; }//订单编号细单id

        [DataMember(Order = 1)]
        public string productCode { get; set; }//货品id

        [DataMember(Order = 2)]
        public string amount { get; set; }//数量

        [DataMember(Order = 3)]
        public string price { get; set; }//单价

        [DataMember(Order = 4)]
        public string money { get; set; }//金额

        [DataMember(Order = 5)]
        public string batchNumber { get; set; }//批号
    }
    #endregion
    #region 航天金税请求消息
    public class HeadList
    {
        [DataMember(Order = 0)]
        public string xsddm { get; set; }//单据号
        [DataMember(Order = 1)]
        public string fph { get; set; }//发票号
        [DataMember(Order = 2)]
        public string fpdm { get; set; }//发票代码
        [DataMember(Order = 3)]
        public string kprq { get; set; }//开票日期
        [DataMember(Order = 4)]
        public List<DetailList> detail { get; set; }//单据明细ID列表
    }
    public class DetailList
    {
        [DataMember(Order = 0)]
        public string cpdm { get; set; }//订单编号细单id
    }
    #endregion
    #region B2B返回消息明细
    public class RProductList
    {
        [DataMember(Order = 0)]
        public string erpOrderItemId { get; set; }//erp订单明细id

        [DataMember(Order = 1)]
        public string productCode { get; set; }//产品编码

        [DataMember(Order = 2)]
        public string amount { get; set; }//数量  

        [DataMember(Order = 3)]
        public string price { get; set; }//单价

        [DataMember(Order = 4)]
        public string money { get; set; }//金额

        [DataMember(Order = 5)]
        public string promoMoney { get; set; }//折扣金额为负

        [DataMember(Order = 6)]
        public string batchNumber { get; set; }//批号

        [DataMember(Order = 7)]
        public string validDate { get; set; }//有效期至

        [DataMember(Order = 8)]
        public string checkFile { get; set; }//质检单路径
    }
    #endregion
}