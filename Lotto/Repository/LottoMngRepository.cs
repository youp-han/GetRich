using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Lotto.Models;
using Lotto.Core;
using Newtonsoft.Json;

namespace Lotto.Repository
{
    public class LottoMngRepository : ILottoMngRepository
    {

        private LottoMNGContext context;
        private LottoCore lottoCore;
        private CoreAPICall LottoCon;

        public LottoMngRepository(LottoMNGContext context)
        {
            this.context = context;
            lottoCore = new LottoCore();
            LottoCon = new CoreAPICall();
        }


        #region Get Lotto History List: GetLottoHistories()

        public List<Lotto_History> GetLottoHistoryList()
        {
            return this.GetLottoHistories();
        }

        //get Top 30 or int numbers
        public List<Lotto_History> GetLottoHistoryList(int topCounts)
        {
            return this.GetLottoHistories(topCounts);
        }

        List<Lotto_History> GetLottoHistories()
        {
            return context.Lotto_Histories.ToList();
        }

        List<Lotto_History> GetLottoHistories(int topCounts)
        {     
            return context.Lotto_Histories.OrderByDescending(x => x.seqNo).Take(topCounts).ToList();
        }

        public int GetTopNumber()
        {
            if (GetLottoHistoryList().Count >0)
            {
                return GetLottoHistoryList(1)[0].seqNo;
            }
            else
            {
                return 0;
            }
            
        }

        #endregion

        #region Save Lotto Number : LottoNumberSave()

        public Lotto_History GetUpdateNumbers()
        {

            int getNumTh = 0; 

            if (GetLottoHistoryList(1).Any())
            {
                getNumTh = GetLottoHistoryList(1)[0].seqNo + 1;
                try
                {
                    return getLatestNumber(getNumTh);
                }
                catch (Exception e)
                {

                    throw new Exception("LottoMngRepository 에서 작동 시 문제가 있음." + e.Message);
                }
            }
            else
            {
                return getLatestNumber(getNumTh+1);
            }
                           
            
        }

        /// <summary>
        /// receive the Latest Number
        /// </summary>
        /// <returns></returns>
        Lotto_History getLatestNumber(int getNumTh)
        {
            string result = LottoCon.CallAPI("https://www.dhlottery.co.kr/common.do?method=getLottoNumber&drwNo=" + getNumTh);

            //result = {
            //    "totSellamnt": 88465183000, //총판매금액
            //    "returnValue": "success",
            //    "drwNoDate": "2020-01-18", 
            //    "firstWinamnt": 2377935959,
            //    "drwtNo6": 43,
            //    "drwtNo4": 40,
            //    "firstPrzwnerCo": 9, //1등당첨인원수
            //    "drwtNo5": 41,
            //    "bnusNo": 45,
            //    "firstAccumamnt": 21401423631, //1등 당첨금액
            //    "drwNo": 894,
            //    "drwtNo2": 32,
            //    "drwtNo3": 37,
            //    "drwtNo1": 19
            //}

            //result = {
            //    "returnValue": "fail"
            //}
            
        Lotto_History returnValue = null;

            Dictionary<string, string> latestNumber = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

            if(latestNumber["returnValue"].Equals("success"))
            {
                returnValue = new Lotto_History()
                {
                    seqNo = Int32.Parse(latestNumber["drwNo"]),
                    num1 = Int32.Parse(latestNumber["drwtNo1"]),
                    num2 = Int32.Parse(latestNumber["drwtNo2"]),
                    num3 = Int32.Parse(latestNumber["drwtNo3"]),
                    num4 = Int32.Parse(latestNumber["drwtNo4"]),
                    num5 = Int32.Parse(latestNumber["drwtNo5"]),
                    num6 = Int32.Parse(latestNumber["drwtNo6"]),
                    bonus = Int32.Parse(latestNumber["bnusNo"]),
                    firstPriceTotal = Decimal.Parse(latestNumber["firstAccumamnt"]),
                    eachReceivedFirstPrice = Decimal.Parse(latestNumber["firstWinamnt"]),
                    firstPriceSelected = Int32.Parse(latestNumber["firstPrzwnerCo"]),
                    drawDate = latestNumber["drwNoDate"]
                };

            }
            else
            {
                returnValue = new Lotto_History()
                {
                    num1 = -1
                };
            }

            return returnValue;

        }

        public void LottoNumberSave(Lotto_History item)
        {
            context.Lotto_Histories.Add(item);
            this.Save();
        }

        #endregion
        
        #region Lotto History Common
        void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;


        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }

            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
        
        #region Core Calc
        /// <summary>
        /// it sorts the most occured numbers DESC
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, int> GetNumberCounts()
        {
            //전체 리스트
            var list = this.GetLottoHistoryList();

            //Count 계산된 Array
            // 총 45 개이며, key 값= 0..44 (0=1..44=45)
            int[] results = lottoCore.GetTotalCounts(list);


            //Count 계산된 Array
            // 총 45 개이며, key 값= 0..44 (0=1..44=45)
            //Key, Value 로 Dictionary 에 추가한다.
            Dictionary<int, int> lottoOccurence = lottoCore.BuildDictionary(results);

            //Dictionary 를 Key 값으로 Sort 한다. (DESC)
            var sortedItems = lottoCore.GetDictionarySorted(lottoOccurence, results.Count());

            return lottoOccurence;

        }

        public List<Numbers_NCounts> ConvertDictionaryToList(Dictionary<int, int> convertedSource)
        {
            List<Numbers_NCounts> convertedResult = new List<Numbers_NCounts>();

            //int countsID = 1;
            foreach (var item in convertedSource)
            {
                Numbers_NCounts counts = new Numbers_NCounts();

                //counts.ID = countsID;
                counts.numbers = item.Key;
                counts.nCounts = item.Value;
                //countsID++;
                convertedResult.Add(counts);
            }

            var sortedList = convertedResult.OrderBy(m => m.nCounts).ToList();


            return sortedList;

        }

        public Dictionary<int, int> GetNumberCounts(int recentNumbers)
        {
            //전체 리스트
            var list = this.GetLottoHistoryList(recentNumbers);

            //Count 계산된 Array
            // 총 45 개이며, key 값= 0..44 (0=1..44=45)
            int[] results = lottoCore.GetTotalCounts(list);


            //Count 계산된 Array
            // 총 45 개이며, key 값= 0..44 (0=1..44=45)
            //Key, Value 로 Dictionary 에 추가한다.
            Dictionary<int, int> lottoOccurence = lottoCore.BuildDictionary(results);

            //Dictionary 를 Key 값으로 Sort 한다. (DESC)
            var sortedItems = lottoCore.GetDictionarySorted(lottoOccurence, results.Count());
            return sortedItems;

        }

        #endregion


        #region 사용안하는 코드
        public string GetGetPosNum()
        {

            Dictionary<int, int> possibilities = new Dictionary<int, int>();
            possibilities.Add(1, 0);
            possibilities.Add(2, 0);
            possibilities.Add(3, 0);
            possibilities.Add(4, 0);
            possibilities.Add(5, 0);
            possibilities.Add(6, 0);

            Dictionary<string, int> hitPoint = new Dictionary<string, int>();


            for (int i = 0; i < 1000000; i++)
            {
                //1. Generate Numbers
                int hitCount = 0;
                int[] genNumbers = lottoCore.GetGeneratedLottoNumber();

                Array.Sort(genNumbers);

                Lotto_History objGetNumbers = new Lotto_History()
                {
                    num1 = genNumbers[0],
                    num2 = genNumbers[1],
                    num3 = genNumbers[2],
                    num4 = genNumbers[3],
                    num5 = genNumbers[4],
                    num6 = genNumbers[5]
                };

                //2. Compare with Previous History
                List<Lotto_History> lottoHistories = GetLottoHistoryList();

                foreach (var item in lottoHistories)
                {

                    if (item.num1 == objGetNumbers.num1)
                    {
                        hitCount += 1;
                    }
                    if (item.num2 == objGetNumbers.num2)
                    {
                        hitCount += 1;
                    }
                    if (item.num3 == objGetNumbers.num3)
                    {
                        hitCount += 1;
                    }
                    if (item.num4 == objGetNumbers.num4)
                    {
                        hitCount += 1;
                    }
                    if (item.num5 == objGetNumbers.num5)
                    {
                        hitCount += 1;
                    }
                    if (item.num6 == objGetNumbers.num6)
                    {
                        hitCount += 1;
                    }


                    int count = 0;
                    if (hitCount > 0)
                    {
                        count = possibilities[hitCount];
                        count += 1;
                        possibilities[hitCount] = count;

                    }

                    if (hitCount == 6)
                    {
                        hitPoint.Add(hitCount + ": " + i + ":" + item.seqNo, hitCount);
                        possibilities[6] += 1;
                    }

                    if (hitCount == 5)
                    {
                        hitPoint.Add(hitCount + ": " + i + ":" + item.seqNo, hitCount);
                        possibilities[5] += 1;
                    }

                    if (hitCount == 4)
                    {
                        hitPoint.Add(hitCount + ": " + i + ":" + item.seqNo, hitCount);
                        possibilities[4] += 1;
                    }

                    if (hitCount == 3)
                    {
                        hitPoint.Add(hitCount + ": " + i + ":" + item.seqNo, hitCount);
                        possibilities[3] += 1;
                    }

                    hitCount = 0;

                }
            }

            string result = null;
            string result2 = null;
            /*             
                    {   "hitcount" : "3",
                        "iteration, seqNo" : "0, 293",
                        "seqNo" : "293"     },
             */
            //foreach (var item in hitPoint) //{[3: 3:99,  3]}
            foreach (var item in possibilities) //{[3: 3:99,  3]}
            {
                result = result + item.Key + " 만에 " + item.Value + "가 나왔습니다. ";
                result2 = result + " { \"possibilities\" : " + "\"" + item.Key + "\"" + "," + "\"hitcount\" : " + "\"" + item.Value + "\" }, ";
            }


            return result;


            //3. Do 1&2 until, 1-6 numbers are all matched. (Count Number of Generated Numbers)

            //4. Show Counted Numbers.
        }
        #endregion



    }
}