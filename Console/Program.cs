using System;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Net;
using System.Text;
using swiftpass.utils;
using System.Web.Script.Serialization;
namespace ConsoleHydee
{
    class Program
    {
        static HttpListener httpobj;

        static void Main(string[] args)
        {
            Console.WriteLine("");
            PublicBll bll = new PublicBll();
            //提供一个简单的、可通过编程方式控制的 HTTP 协议侦听器。此类不能被继承。
            httpobj = new HttpListener();
            //定义url及端口号，通常设置为配置文件
            //string url = ConfigurationManager.AppSettings["Url"];
            string url = "";
            bll.geturlParms(Environment.CurrentDirectory + "\\DBConn\\001.xml", out url);
            Console.WriteLine($"URL：{url}\r\n");
            httpobj.Prefixes.Add(url);
            //启动监听器
            httpobj.Start();
            //异步监听客户端请求，当客户端的网络请求到来时会自动执行Result委托
            //该委托没有返回值，有一个IAsyncResult接口的参数，可通过该参数获取context对象
            httpobj.BeginGetContext(Result, null);
            Console.WriteLine($"服务端初始化完毕，正在等待客户端请求,时间：{DateTime.Now.ToString()}\r\n");
            Console.ReadKey();
        }

        private static void Result(IAsyncResult ar)
        {
            //当接收到请求后程序流会走到这里

            //继续异步监听
            httpobj.BeginGetContext(Result, null);
            var guid = Guid.NewGuid().ToString();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"接到新的请求:{guid},时间：{DateTime.Now.ToString()}\r\n");
            //获得context对象
            var context = httpobj.EndGetContext(ar);
            var request = context.Request;
            var response = context.Response;
            context.Response.ContentType = "text/plain;charset=UTF-8";//告诉客户端返回的ContentType类型为纯文本格式，编码为UTF-8
            context.Response.AddHeader("Content-type", "text/plain");//添加响应头信息
            context.Response.ContentEncoding = Encoding.UTF8;
            string returnObj = null;//定义返回客户端的信息
            if (request.HttpMethod == "POST" && request.InputStream != null)
            {
                //处理客户端发送的请求并返回处理信息
                returnObj = HandleRequest(request, response);
            }
            else
            {
                returnObj = $"不是post请求或者传过来的数据为空\r\n";
            }
            var returnByteArr = Encoding.UTF8.GetBytes(returnObj);//设置客户端返回信息的编码
            try
            {
                using (var stream = response.OutputStream)
                {
                    //把处理信息返回到客户端
                    stream.Write(returnByteArr, 0, returnByteArr.Length);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"网络蹦了：{ex.ToString()}\r\n");
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"请求处理完成：{guid},时间：{ DateTime.Now.ToString()}\r\n");
        }

        private static string HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            string data = null;
            string dodata = null;
            try
            {
                var byteList = new List<byte>();
                var byteArr = new byte[2048];
                int readLen = 0;
                int len = 0;
                //接收客户端传过来的数据并转成字符串类型
                do
                {
                    readLen = request.InputStream.Read(byteArr, 0, byteArr.Length);
                    len += readLen;
                    byteList.AddRange(byteArr);
                } while (readLen != 0);
                data = Encoding.UTF8.GetString(byteList.ToArray(), 0, len);
                dodata = HydeeInterfaces(data, request.RawUrl);
                //获取得到数据data可以进行其他操作
            }
            catch (Exception ex)
            {
                response.StatusDescription = "404";
                response.StatusCode = 404;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"在接收数据时发生错误:{ex.ToString()}\r\n");
                return $"在接收数据时发生错误:{ex.ToString()}\r\n";//把服务端错误信息直接返回可能会导致信息不安全，此处仅供参考
            }
            response.StatusDescription = "200";//获取或设置返回给客户端的 HTTP 状态代码的文本说明。
            response.StatusCode = 200;// 获取或设置返回给客户端的 HTTP 状态代码。
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"接收数据完成:{data.Trim()},时间：{DateTime.Now.ToString()}\r\n");
            Console.WriteLine($"数据处理结果：{dodata}\r\n");
            //return $"接收数据完成";
            return dodata;
        }

        public static string HydeeInterfaces(string ReqHeadJson, string ReqType)
        {
            #region 变量定义
            ObjectList.ReqData_B2BList _reqData_B2BList;
            ObjectList.RetData_B2BList _retData_B2BList;
            PublicBll bll = new PublicBll();
            JObject jObject = null;
            JavaScriptSerializer js = new JavaScriptSerializer();
            List<ProductList> _productList = null;
            List<RProductList> _rproductList = null;
            List<HeadList> _HeadList = null;
            RProductList _rproduct = null;
            List<ObjectList.Data> _datalist = null;
            ObjectList.Data _data = null;
            string[] ls_inparamtype, ls_inparamvalue, ls_outparam, ls_outparamtype, ls_inparam;
            string ls_retmsg = "TRUE";
            string _product = null;
            string _head = null;
            string ls_billno = null;
            string ls_compid = "101";//健一行
            string ls_busno = "1010000";//浙江健一行医药科技有限公司
            string ls_billtype = "ACB";
            string ls_htjs_bill = null;
            string ls_htjs_fph = null;
            string ls_htjs_kprq = null;
            string ls_htjs_fpdm = null;
            string ls_htjs_cpdm = null;
            string ls_ybyorderno = null;
            _reqData_B2BList = new ObjectList.ReqData_B2BList();
            DataTable dt = null;
            #endregion
            try
            {
                #region 航天金税
                if (ReqType == "/Commit_Invoice")
                {
                    #region 反序列化请求
                    jObject = JObject.Parse(ReqHeadJson);
                    _head = jObject["head"].ToString();
                    _HeadList = new List<HeadList>();
                    _HeadList = PublicClass.JsonStringToList<HeadList>(_head);
                    #endregion
                }
                #endregion
                #region 医保云
                if (ReqType == "/FindPrint")
                {
                    #region 反序列化请求
                    jObject = JObject.Parse(ReqHeadJson);
                    ls_ybyorderno = jObject["orderno"].ToString();
                    #endregion
                }
                #endregion
                #region 炫方B2B
                else
                {
                    #region 反序列化请求productList
                    jObject = JObject.Parse(ReqHeadJson);
                    _product = jObject["productList"].ToString();
                    _productList = new List<ProductList>();
                    _productList = PublicClass.JsonStringToList<ProductList>(_product);
                    #endregion
                    #region 变量初始化
                    _reqData_B2BList.productList = _productList;
                    _reqData_B2BList.orderId = jObject["orderId"].ToString();
                    _reqData_B2BList.salerId = jObject["salerId"].ToString();
                    _reqData_B2BList.payType = jObject["payType"].ToString();
                    _reqData_B2BList.buyerCode = jObject["buyerCode"].ToString();
                    _reqData_B2BList.addressId = jObject["addressId"].ToString();
                    _reqData_B2BList.agentId = jObject["agentId"].ToString();
                    #endregion
                }
                #endregion
                #region 获取数据库连接
                if (!bll.getcnParms(Environment.CurrentDirectory + "\\DBConn\\" + "002.xml", out ls_retmsg))
                {
                    bll.dao.RollbackTrans();
                    _retData_B2BList = new ObjectList.RetData_B2BList();
                    _retData_B2BList.returnCode = "0";
                    _retData_B2BList.returnMsg = ls_retmsg;
                    return js.Serialize(_retData_B2BList);
                }
                #endregion
                #region 验证身份
                ls_retmsg = bll.checkUserValid(bll.FunctionId, bll.InterfaceUserID, bll.InterfacePassWord, bll.OperUserID, bll.OperPassWord, ls_retmsg);
                if (ls_retmsg != "TRUE")
                {
                    #region 错误返回
                    bll.dao.RollbackTrans();
                    _retData_B2BList = new ObjectList.RetData_B2BList();
                    _retData_B2BList.returnCode = "-1";
                    _retData_B2BList.returnMsg = ls_retmsg;
                    return js.Serialize(_retData_B2BList);
                    #endregion
                }
                #endregion
                #region  业务处理
                switch (ReqType)
                {
                    case "/CreateOrder"://创建订单
                        #region 数据验证
                        if (string.IsNullOrEmpty(_reqData_B2BList.buyerCode))
                        {
                            ls_retmsg = "【客户编码】不能为空";
                            #region 错误返回
                            bll.dao.RollbackTrans();
                            _retData_B2BList = new ObjectList.RetData_B2BList();
                            _retData_B2BList.returnCode = "-1";
                            _retData_B2BList.returnMsg = ls_retmsg;
                            return js.Serialize(_retData_B2BList);
                            #endregion
                        }
                        dt = bll.dao.GetDataTable("select 1 from t_vencus where compid = " + ls_compid + @" and vencusno = " + _reqData_B2BList.buyerCode);
                        if (dt.Rows.Count <= 0)
                        {
                            ls_retmsg = "无法识别的【客户编码】";
                            #region 错误返回
                            bll.dao.RollbackTrans();
                            _retData_B2BList = new ObjectList.RetData_B2BList();
                            _retData_B2BList.returnCode = "-1";
                            _retData_B2BList.returnMsg = ls_retmsg;
                            return js.Serialize(_retData_B2BList);
                            #endregion
                        }
                        if (string.IsNullOrEmpty(_reqData_B2BList.agentId))
                        {
                            _reqData_B2BList.agentId = "999";
                        }
                        if (string.IsNullOrEmpty(_reqData_B2BList.salerId))
                        {
                            _reqData_B2BList.salerId = "999";
                        }
                        //if (string.IsNullOrEmpty(_reqData_B2BList.salerId))
                        //{
                        //ls_retmsg = "【业务员id】不能为空";
                        //#region 错误返回
                        //bll.dao.RollbackTrans();
                        //_retData_B2BList = new ObjectList.RetData_B2BList();
                        //_retData_B2BList.returnCode = "0";
                        //_retData_B2BList.returnMsg = ls_retmsg;
                        //return js.Serialize(_retData_B2BList);
                        //#endregion
                        //}
                        //else
                        //{
                        //dt = bll.dao.GetDataTable("select 1 from t_vencus_saler where compid = " + ls_compid + @" and vencusno = " + _reqData_B2BList.buyerCode + " and saler = " + _reqData_B2BList.salerId + "");
                        //if (dt.Rows.Count <= 0)
                        //{
                        //    ls_retmsg = "无法识别的【业务员id】";
                        //    #region 错误返回
                        //    bll.dao.RollbackTrans();
                        //    _retData_B2BList = new ObjectList.RetData_B2BList();
                        //    _retData_B2BList.returnCode = "0";
                        //    _retData_B2BList.returnMsg = ls_retmsg;
                        //    return js.Serialize(_retData_B2BList);
                        //    #endregion
                        //}
                        //}
                        dt = bll.dao.GetDataTable("select ''||f_get_serial('" + ls_billtype + "','" + ls_compid + "') billno from dual");
                        ls_billno = dt.Rows[0]["billno"].ToString();
                        dt = bll.dao.GetDataTable("select 1 from t_b2b_order_status where order_id = '" + _reqData_B2BList.orderId + "'");
                        if (dt.Rows.Count > 0)
                        {
                            ls_retmsg = "已经存在的【订单编号" + _reqData_B2BList.orderId.ToString() + "】";
                            #region 错误返回
                            bll.dao.RollbackTrans();
                            _retData_B2BList = new ObjectList.RetData_B2BList();
                            _retData_B2BList.returnCode = "-1";
                            _retData_B2BList.returnMsg = ls_retmsg;
                            return js.Serialize(_retData_B2BList);
                            #endregion
                        }
                        #endregion
                        #region 插入批发申请单头
                        ls_retmsg = bll.dao.SqlDataTable(@"INSERT INTO t_batsaleapply_h
                                                              (applyno,
                                                               srcbillno,
                                                               billcode,
                                                               compid,
                                                               vencusno,
                                                               vencusname,
                                                               subitemid,
                                                               busno,
                                                               paytype,
                                                               cashtype,
                                                               saler,
                                                               ownerid,
                                                               reckonerid,
                                                               accchked,
                                                               invoicetype,
                                                               addrid,
                                                               whlgroupid,
                                                               lastmodify,
                                                               lasttime,
                                                               status,
                                                               checkbit1,
                                                               checkbit2,
                                                               checkbit3,
                                                               checkbit4,
                                                               checkbit5,
                                                               createuser,
                                                               createtime,
                                                               indentflag,
                                                               account_date,
                                                               credited,
                                                               sum_whlprice,
                                                               NOTES,
                                                               src,
                                                               srcpaytype,
                                                               srcagentId
                                                                )
                                                            VALUES
                                                              ('" + ls_billno + @"',
                                                               '" + _reqData_B2BList.orderId + @"',
                                                               '" + ls_billtype + @"',
                                                               " + ls_compid + @",
                                                               " + _reqData_B2BList.buyerCode + @",
                                                               (select vencusname from t_vencus where compid = " + ls_compid + @" and vencusno = " + _reqData_B2BList.buyerCode + @"),
                                                               0,
                                                               " + ls_busno + @",
                                                               (select paytype from t_vencus where compid = " + ls_compid + @" and vencusno = " + _reqData_B2BList.buyerCode + @" and rownum = 1),
                                                               (select cashtype from t_vencus where compid = " + ls_compid + @" and vencusno = " + _reqData_B2BList.buyerCode + @" and rownum = 1),
                                                               " + _reqData_B2BList.salerId + @",
                                                               '01',
                                                               '01',
                                                               0,
                                                               (select invoicetype from t_vencus where compid = " + ls_compid + @" and vencusno = " + _reqData_B2BList.buyerCode + @" and rownum = 1),
                                                               " + _reqData_B2BList.addressId + @",
                                                               (select whlgroupid from t_vencus_saler where compid = " + ls_compid + @" and vencusno = " + _reqData_B2BList.buyerCode + @" and rownum = 1),
                                                               168,
                                                               sysdate,
                                                               1,
                                                               1,
                                                               1,
                                                               1,
                                                               1,
                                                               1,
                                                               168,
                                                               sysdate,
                                                               0,
                                                               sysdate,
                                                               null,
                                                               null,
                                                               '来自炫方B2B',
                                                               1,
                                                               '" + _reqData_B2BList.payType + @"',
                                                               '" + _reqData_B2BList.agentId + @"')
                                                            ");
                        if (ls_retmsg != "TRUE")
                        {
                            #region 错误返回
                            bll.dao.RollbackTrans();
                            _retData_B2BList = new ObjectList.RetData_B2BList();
                            _retData_B2BList.returnCode = "-1";
                            _retData_B2BList.returnMsg = ls_retmsg;
                            return js.Serialize(_retData_B2BList);
                            #endregion
                        }
                        #endregion
                        #region 插入批发申请单明细
                        for (int i = 0; i < _reqData_B2BList.productList.Count; i++)
                        {
                            dt = bll.dao.GetDataTable("select 1 from t_ware where compid = " + ls_compid + @" and warecode = " + _reqData_B2BList.productList[i].productCode);
                            if (dt.Rows.Count <= 0)
                            {
                                ls_retmsg = "无法识别的【货品id】";
                                #region 错误返回
                                bll.dao.RollbackTrans();
                                _retData_B2BList = new ObjectList.RetData_B2BList();
                                _retData_B2BList.returnCode = "-1";
                                _retData_B2BList.returnMsg = ls_retmsg;
                                return js.Serialize(_retData_B2BList);
                                #endregion
                            }
                            ls_retmsg = bll.dao.SqlDataTable(@"INSERT INTO t_batsaleapply_d
                                                          (applyno,
                                                           rowno,
                                                           wareid,
                                                           wareqty,
                                                           checkqty,
                                                           purprice,
                                                           purtax,
                                                           saleprice,
                                                           whlprice,
                                                           maxwhlprice,
                                                           maxqty,
                                                           midqty,
                                                           avgpurprice,
                                                           redeemsum,
                                                           lastwhlprice,
                                                           MAKENO)
                                                        VALUES
                                                          ('" + ls_billno + @"',
                                                           " + _reqData_B2BList.productList[i].orderItemId + @",
                                                           (select wareid from t_ware where compid = " + ls_compid + @" and warecode = " + _reqData_B2BList.productList[i].productCode + @" and rownum = 1),
                                                           " + _reqData_B2BList.productList[i].amount + @",
                                                           " + _reqData_B2BList.productList[i].amount + @",
                                                           (select lastpurprice from t_ware where compid = " + ls_compid + @" and warecode = " + _reqData_B2BList.productList[i].productCode + @" and rownum = 1),
                                                           (select purtax from t_ware where compid = " + ls_compid + @" and warecode = " + _reqData_B2BList.productList[i].productCode + @" and rownum = 1),
                                                           0,
                                                           " + _reqData_B2BList.productList[i].price + @",
                                                           0,
                                                           (select maxqty from t_ware where compid = " + ls_compid + @" and warecode = " + _reqData_B2BList.productList[i].productCode + @" and rownum = 1),
                                                           (select midqty from t_ware where compid = " + ls_compid + @" and warecode = " + _reqData_B2BList.productList[i].productCode + @" and rownum = 1),
                                                           0,
                                                           0,
                                                           (select lastwhlprice from t_ware where compid = " + ls_compid + @" and warecode = " + _reqData_B2BList.productList[i].productCode + @" and rownum = 1),
                                                            '" + _reqData_B2BList.productList[i].batchNumber + @"')
                                                        ");
                            if (ls_retmsg != "TRUE")
                            {
                                #region 错误返回
                                bll.dao.RollbackTrans();
                                _retData_B2BList = new ObjectList.RetData_B2BList();
                                _retData_B2BList.returnCode = "-1";
                                _retData_B2BList.returnMsg = ls_retmsg;
                                return js.Serialize(_retData_B2BList);
                                #endregion
                            }
                        }
                        #endregion
                        #region 自动转单
                        ls_inparam = new string[2];
                        ls_inparam[0] = "p_applyno";
                        ls_inparam[1] = "p_lastmodify";

                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "int";

                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = ls_billno;
                        ls_inparamvalue[1] = "168";

                        ls_outparam = new string[1];
                        ls_outparam[0] = "out_batsaleno";
                        ls_outparamtype = new string[1];
                        ls_outparamtype[0] = "varchar";
                        ls_retmsg = bll.dao.Doprocedure("proc_batsaleapply2batsale_b2b", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, false);
                        if (ls_retmsg != "TRUE")
                        {
                            #region 错误返回
                            bll.dao.RollbackTrans();
                            _retData_B2BList = new ObjectList.RetData_B2BList();
                            _retData_B2BList.returnCode = "-1";
                            _retData_B2BList.returnMsg = ls_retmsg;
                            return js.Serialize(_retData_B2BList);
                            #endregion
                        }
                        #endregion
                        #region 插入单据状态表
                        ls_retmsg = bll.dao.SqlDataTable("INSERT INTO t_b2b_order_status(order_id,status,update_date) values('" + _reqData_B2BList.orderId + "',2,sysdate)");
                        if (ls_retmsg != "TRUE")
                        {
                            #region 错误返回
                            bll.dao.RollbackTrans();
                            _retData_B2BList = new ObjectList.RetData_B2BList();
                            _retData_B2BList.returnCode = "-1";
                            _retData_B2BList.returnMsg = ls_retmsg;
                            return js.Serialize(_retData_B2BList);
                            #endregion
                        }
                        #endregion
                        #region 返回
                        if (ls_retmsg != "TRUE")
                        {
                            #region 错误返回
                            bll.dao.RollbackTrans();
                            _retData_B2BList = new ObjectList.RetData_B2BList();
                            _retData_B2BList.returnCode = "-1";
                            _retData_B2BList.returnMsg = ls_retmsg;
                            return js.Serialize(_retData_B2BList);
                            #endregion
                        }
                        else
                        {
                            #region 成功返回
                            bll.dao.CommitTrans();
                            _retData_B2BList = new ObjectList.RetData_B2BList();
                            _rproductList = new List<RProductList>();
                            _datalist = new List<ObjectList.Data>();
                            dt = bll.dao.GetDataTable(@"SELECT rowno erpOrderItemId,
                                                   f_get_warecode(wareid, " + ls_compid + @") productCode,
                                                   wareqty AS amount,
                                                   whlprice AS price,
                                                   wareqty * whlprice AS money,
                                                   0 AS promoMoney,
                                                   makeno AS batchnumber,
                                                   invalidate AS validDate,
                                                   NULL AS checkFile
                                              FROM t_batsale_d where BATSALENO in(select BATSALENO from t_batsaleapply_h where applyno = '" + ls_billno + "')");
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                _rproduct = new RProductList();
                                _rproduct.erpOrderItemId = dt.Rows[i]["erpOrderItemId"].ToString();
                                _rproduct.productCode = dt.Rows[i]["productCode"].ToString();
                                _rproduct.amount = dt.Rows[i]["amount"].ToString();
                                _rproduct.price = dt.Rows[i]["price"].ToString();
                                _rproduct.money = dt.Rows[i]["money"].ToString();
                                _rproduct.promoMoney = dt.Rows[i]["promoMoney"].ToString();
                                _rproduct.batchNumber = dt.Rows[i]["batchNumber"].ToString();
                                _rproduct.validDate = dt.Rows[i]["validDate"].ToString();
                                _rproduct.checkFile = dt.Rows[i]["checkFile"].ToString();
                                _rproductList.Add(_rproduct);
                            }
                            _data = new ObjectList.Data();
                            _data.orderId = _reqData_B2BList.orderId;
                            _data.erpOrderId = ls_billno;
                            _data.buyerCode = _reqData_B2BList.buyerCode;
                            _data.productList = _rproductList;
                            _datalist.Add(_data);
                            _retData_B2BList.returnCode = "1";
                            _retData_B2BList.returnMsg = "OK";
                            _retData_B2BList.data = _datalist;
                            return js.Serialize(_retData_B2BList);
                            #endregion
                        }
                    #endregion
                    case "/CancelOrder"://取消订单
                        #region 返回
                        bll.dao.RollbackTrans();
                        _retData_B2BList = new ObjectList.RetData_B2BList();
                        _retData_B2BList.returnCode = "0";
                        _retData_B2BList.returnMsg = "此接口开发中......";
                        return js.Serialize(_retData_B2BList);
                    #endregion
                    case "/Commit_Invoice"://发票回写
                        #region 回写处理
                        bll.dao.RollbackTrans();
                        _retData_B2BList = new ObjectList.RetData_B2BList();
                        _retData_B2BList.returnCode = "0";
                        //_retData_B2BList.returnMsg ="收到入参：单据号：["+ls_htjs_bill+"]  发票号：["+ls_htjs_fph+"]  发票代码：[ "+ls_htjs_fpdm+"]  开票日期：["+ls_htjs_kprq +"]  航天金税接口开发中......";
                        for (int i = 0; i < _HeadList.Count; i++)
                        {
                            _retData_B2BList.returnMsg += _HeadList[i].detail[0].cpdm.ToString();
                            ls_htjs_bill = null;
                            ls_htjs_fph = null;
                            ls_htjs_fpdm = null;
                            ls_htjs_kprq = null;
                            ls_htjs_bill = _HeadList[i].xsddm.ToString();
                            ls_htjs_fph = _HeadList[i].fph.ToString();
                            ls_htjs_fpdm = _HeadList[i].fpdm.ToString();
                            ls_htjs_kprq = _HeadList[i].kprq.ToString();
                            for (int j = 0; j < _HeadList[i].detail.Count; j++)
                            {
                                ls_htjs_cpdm = null;
                                ls_htjs_cpdm = _HeadList[i].detail[j].cpdm.ToString();
                                #region 插入
                                ls_retmsg = bll.dao.SqlDataTable(@"INSERT INTO t_interface_htjs(xsddm,fph,fpdm,kprq,cpdm,operdate) values('" + ls_htjs_bill + @"',
                                    '" + ls_htjs_fph + "','" + ls_htjs_fpdm + "', to_date('"+ ls_htjs_kprq + "','yyyy-mm-dd hh24:mi:ss'),'"+ls_htjs_cpdm+"',sysdate)");
                                if (ls_retmsg != "TRUE")
                                {
                                    #region 错误返回
                                    bll.dao.RollbackTrans();
                                    _retData_B2BList = new ObjectList.RetData_B2BList();
                                    _retData_B2BList.returnCode = "-1";
                                    _retData_B2BList.returnMsg = ls_retmsg;
                                    return js.Serialize(_retData_B2BList);
                                    #endregion
                                }
                                #endregion
                            }
                        }
                        #endregion
                        #region 返回
                        if (ls_retmsg != "TRUE")
                        {
                            #region 错误返回
                            bll.dao.RollbackTrans();
                            _retData_B2BList = new ObjectList.RetData_B2BList();
                            _retData_B2BList.returnCode = "-1";
                            _retData_B2BList.returnMsg = ls_retmsg;
                            return js.Serialize(_retData_B2BList);
                            #endregion
                        }
                        else
                        {
                            #region 成功返回
                            bll.dao.CommitTrans();
                            _retData_B2BList = new ObjectList.RetData_B2BList();
                            _retData_B2BList.returnCode = "0";
                            _retData_B2BList.returnMsg = ls_retmsg;
                            return js.Serialize(_retData_B2BList);
                            #endregion
                        }
                    #endregion
                    default:
                        #region 异常返回
                        bll.dao.RollbackTrans();
                        _retData_B2BList = new ObjectList.RetData_B2BList();
                        _retData_B2BList.returnCode = "0";
                        _retData_B2BList.returnMsg = "无法识别的功能地址";
                        return js.Serialize(_retData_B2BList);
                        #endregion
                }
            }
            #endregion
            catch (Exception ex)
            {
                #region 异常返回
                bll.dao.RollbackTrans();
                _retData_B2BList = new ObjectList.RetData_B2BList();
                _retData_B2BList.returnCode = "0";
                _retData_B2BList.returnMsg = ex.Message.ToString();
                return js.Serialize(_retData_B2BList);
                #endregion
            }
        }
    }
}
