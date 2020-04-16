using System;
using System.Messaging;
using System.Windows.Forms;

namespace webpageCaller
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        libCommon.clsUtil objUtil;

        System.Threading.Thread mainCaller;
        System.Threading.Thread retryCaller;

        private Type[] objType;
        private string qPath;
        private string qPath_retry;//재시도 큐
        private string encodeType;

        private int sleep;
        private int MaxCallingFailCnt;//실패 재시도 횟수
        private int retryInterval;//재시도 함수 주기
        private int rcvTimeOut;//큐 대기시간

        private void Form1_Load(object sender, EventArgs e)
        {
            objUtil = new libCommon.clsUtil();

            this.objType = new Type[] { typeof(libMyUtil.pageCallingInfo) };
            this.qPath = objUtil.getAppCfg("queuePath");
            this.qPath_retry = objUtil.getAppCfg("queuePath_retry");
            this.encodeType = objUtil.getAppCfg("encodeType").ToUpper();



            this.sleep = objUtil.ToInt32(objUtil.getAppCfg("sleep")) * 1000;
            if (this.sleep == 0)
                this.sleep = 1000;

            this.MaxCallingFailCnt = objUtil.ToInt32(objUtil.getAppCfg("MaxCallingFailCnt"));
            if (this.MaxCallingFailCnt == 0)
                this.MaxCallingFailCnt = 10;

            this.rcvTimeOut = objUtil.ToInt32(objUtil.getAppCfg("rcvTimeout"));
            if (this.rcvTimeOut == 0)
                this.rcvTimeOut = 1;

            this.retryInterval = objUtil.ToInt32(objUtil.getAppCfg("retryInterval")) * 1000;
            if (this.retryInterval == 0)
                this.retryInterval = 60 * 1000;



            

            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);

            MainCallerState.Text = "정지됨";
            RetryCallerState.Text = "정지됨";

            button1.PerformClick();
        }
        /// <summary>
        /// 메인 함수
        /// </summary>
        private void RunCaller()
        {
            libMyUtil.clsMSMQ objMSMQ = new libMyUtil.clsMSMQ();

            while (objMSMQ.canReadQueue(qPath))
                prcRcvObject(objMSMQ.receiveData(this.qPath, this.objType, this.rcvTimeOut));

            libMyUtil.clsThread.SetLabel(MainCallerState, "오류");
            MessageBox.Show(string.Format("해당큐({0})에 접근할 수 없습니다.", qPath));
        }
        /// <summary>
        /// 재시도 함수
        /// </summary>
        private void tryAgain()
        {
            libMyUtil.clsMSMQ objMSMQ = new libMyUtil.clsMSMQ();
            System.Diagnostics.Stopwatch objWatch = new System.Diagnostics.Stopwatch();

            int retryMsgCnt;
            int i;

            while (objMSMQ.canReadQueue(qPath_retry))
            {
                retryMsgCnt = objMSMQ.queueMsgCnt(this.qPath_retry);
                for (i = 0; i < retryMsgCnt; i++)
                {
                    libMyUtil.pageCallingInfo rcvObj = (libMyUtil.pageCallingInfo)objMSMQ.peekData(this.qPath_retry, this.objType, this.rcvTimeOut);
                    if ((System.DateTime.Now - rcvObj.lastTry).Duration().TotalMilliseconds < this.retryInterval)
                        break;
                    prcRcvObject(objMSMQ.receiveData(this.qPath_retry, this.objType, this.rcvTimeOut));
                }

                System.Threading.Thread.Sleep(this.retryInterval);
            }

            libMyUtil.clsThread.SetLabel(RetryCallerState, "오류");
            MessageBox.Show(string.Format("해당큐({0})에 접근할 수 없습니다.", qPath_retry));
        }
        /// <summary>
        /// 수신된 메시지 처리, null수신시 sleep
        /// </summary>
        private void prcRcvObject(object rcvObj)
        {
            if (rcvObj != null)
            {
                System.Diagnostics.Stopwatch objWatch = new System.Diagnostics.Stopwatch();
                objWatch.Start();

                libMyUtil.clsMSMQ objMSMQ = new libMyUtil.clsMSMQ();
                libMyUtil.pageCallingInfo callingInfo = (libMyUtil.pageCallingInfo)rcvObj;

                string Result;
                string url = callingInfo.url;
                string postData = callingInfo.postData;
                int timeOut = callingInfo.timeOut;
                
                if (callingInfo.httpMethod.Equals("POST"))
                    Result = libMyUtil.clsWeb.SendPostData(postData, url, this.encodeType, timeOut * 1000);
                else if (callingInfo.httpMethod.Equals("GET"))
                    Result = libMyUtil.clsWeb.SendQueryString(postData, url, timeOut * 1000);
                else
                    Result = "UNKNOWN HTTP METHOD";

                objUtil.writeLog(string.Format("CALL RESULT : {0}\r\nFKEY:{1}\r\nURL:{2}", Result, callingInfo.FKEY, callingInfo.url));

                if (Result.Equals(callingInfo.callresult))
                    UpdateResult(callingInfo, Result);//성공 결과 DB에 저장
                else if (Result.Equals("FAIL"))
                {
                    //재시도
                    callingInfo.failCnt++;//실패 카운트 증가
                    callingInfo.lastTry = System.DateTime.Now;//시간 기록
                    if (callingInfo.failCnt < this.MaxCallingFailCnt)//설정한 횟수만큼 재시도
                        objMSMQ.sendData(qPath_retry, callingInfo);//재시도 큐에 저장
                    else
                    {
                        callingInfo.writeLog(Result);//최종 실패 메시지 로그 기록
                        UpdateResult(callingInfo, Result);//실패 결과 DB에 저장
                    }
                }
                else
                {
                    callingInfo.writeLog(Result);//최종 실패 메시지 로그 기록
                    UpdateResult(callingInfo, Result);//실패 결과 DB에 저장
                }

                objWatch.Stop();
                if (System.DateTime.Now.Millisecond > 500)
                    objUtil.writeLog(string.Format("TIME SPAN : {0} millisecond", objWatch.Elapsed.TotalMilliseconds.ToString()));
            }
            else
                System.Threading.Thread.Sleep(this.sleep);//큐에 메시지 없으면 슬립
        }
        /// <summary>
        /// tb_aURLresult 테이블 결과 업데이트
        /// </summary>
        /// <param name="Result">전달받은 결과값</param>
        private void UpdateResult(libMyUtil.pageCallingInfo callingInfo, string Result)
        {
            libCommon.clsDB objDB = new libCommon.clsDB();
            System.Data.SqlClient.SqlConnection dbCon;
            System.Data.SqlClient.SqlTransaction TRX;

            string updateResult;

            if (System.Text.Encoding.Default.GetByteCount(Result) > 1000)
                Result = Result.Substring(0, 500);
            
            try
            {
                dbCon = objDB.GetConnection();
                TRX = dbCon.BeginTransaction();
                updateResult = libMyUtil.clsCmnDB.UPDATE_DB(dbCon, TRX, "tb_aURLresult", "RESULT", Result, "aURLidx|FKEY", callingInfo.aURLset_Idx + "|" + callingInfo.FKEY);
                if (updateResult.Equals("FAIL"))
                {
                    TRX.Rollback();
                    objUtil.writeLog(string.Format("FAIL UPDATE URL RESULT\r\nFKEY:{0}\r\nResult:{1}", callingInfo.FKEY, Result));
                }
                else
                    TRX.Commit();
                dbCon.Close();
            }
            catch (Exception ex)
            {
                objUtil.writeLog("ERR UPDATE URL CALL RESULT : " + ex.ToString());
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (mainCaller == null)
            {
                mainCaller = new System.Threading.Thread(new System.Threading.ThreadStart(RunCaller));
                mainCaller.Name = "mainCaller";
                mainCaller.Start();
                MainCallerState.Text = "실행중";
            }
            else
            {
                mainCaller.Abort();
                MainCallerState.Text = "정지됨";
                mainCaller = null;
            }

            if (retryCaller == null)
            {
                retryCaller = new System.Threading.Thread(new System.Threading.ThreadStart(tryAgain));
                retryCaller.Name = "retryCaller";
                retryCaller.Start();
                RetryCallerState.Text = "실행중";
            }
            else
            {                
                retryCaller.Abort();
                RetryCallerState.Text = "정지됨";
                retryCaller = null;
            }
        }
        void Application_ApplicationExit(object sender, EventArgs e)
        {
            try
            {
                mainCaller.Abort();
                retryCaller.Abort();
            }
            catch (Exception ex)
            {
                objUtil.writeLog("ERR ABORT THREAD : " + ex.ToString());
            }
        }



        /// <summary>
        /// 메시지 큐 감시
        /// </summary>
        private IAsyncResult watchMessageQueue(string queuePath, Type[] objectType)
        {
            MessageQueue myQ = new MessageQueue(queuePath);
            
            myQ.Formatter = new XmlMessageFormatter(objectType);
            myQ.ReceiveCompleted += new ReceiveCompletedEventHandler(myQ_ReceiveCompleted);
            
            return myQ.BeginReceive();
        }
        /// <summary>
        /// 메시지 수신 이벤트 발생시 실행
        /// </summary>
        void myQ_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue myQ = (MessageQueue)sender;
            object rcvObj = myQ.EndReceive(e.AsyncResult).Body;
            
            prcRcvObject(rcvObj);
            watchMessageQueue(qPath, objType);
        }
    }
}