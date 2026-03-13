using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing.Imaging;
using System.Linq;
using System.Web.Mvc.Html;
using Lotto.Models;
using Lotto.Core;
using Microsoft.Ajax.Utilities;
using Microsoft.ML.Transforms.Text;
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
            string url = "https://www.dhlottery.co.kr/lt645/selectPstLt645Info.do?srchLtEpsd=";
            string result = LottoCon.CallAPI(url + getNumTh);

            // 새 API 응답 구조:
            // {"resultCode":null,"resultMessage":null,"data":{"list":[{"ltEpsd":1,"tm1WnNo":10,...}]}}

            Lotto_History returnValue = null;

            var json = Newtonsoft.Json.Linq.JObject.Parse(result);
            var list = json["data"]?["list"] as Newtonsoft.Json.Linq.JArray;

            if (list != null && list.Count > 0)
            {
                var item = list[0];

                // ltRflYmd 형식: "20021207" → "2002-12-07"
                string rawDate = item["ltRflYmd"]?.ToString() ?? "";
                string drawDate = rawDate.Length == 8
                    ? rawDate.Substring(0, 4) + "-" + rawDate.Substring(4, 2) + "-" + rawDate.Substring(6, 2)
                    : rawDate;

                returnValue = new Lotto_History()
                {
                    seqNo = (int)item["ltEpsd"],
                    num1 = (int)item["tm1WnNo"],
                    num2 = (int)item["tm2WnNo"],
                    num3 = (int)item["tm3WnNo"],
                    num4 = (int)item["tm4WnNo"],
                    num5 = (int)item["tm5WnNo"],
                    num6 = (int)item["tm6WnNo"],
                    bonus = (int)item["bnsWnNo"],
                    firstPriceTotal = (decimal)item["rnk1SumWnAmt"],
                    eachReceivedFirstPrice = (decimal)item["rnk1WnAmt"],
                    firstPriceSelected = (int)item["rnk1WnNope"],
                    drawDate = drawDate
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


        public List<Target_Numbers> GetTotalCountByPlace(int topCount)
        {
            //1 get first num
            var list = this.GetLottoHistoryList();
            List<Target_Number> firstSet = GetList(list.Select(f => new Target_Number() {targetNumber = f.num1}).ToList());
            List<Target_Number> secondtSet = GetList(list.Select(f => new Target_Number() { targetNumber = f.num2 }).ToList());
            List<Target_Number> thirdSet = GetList(list.Select(f => new Target_Number() { targetNumber = f.num3 }).ToList());
            List<Target_Number> fourthSet = GetList(list.Select(f => new Target_Number() { targetNumber = f.num4 }).ToList());
            List<Target_Number> fifthSet = GetList(list.Select(f => new Target_Number() { targetNumber = f.num5 }).ToList());
            List<Target_Number> sixthSet = GetList(list.Select(f => new Target_Number() { targetNumber = f.num6 }).ToList());
            List<Target_Number> luckySet = GetList(list.Select(f => new Target_Number() { targetNumber = f.bonus }).ToList());
            
            List<Target_Numbers> allNumbers = new List<Target_Numbers>();


            for (int i = 0; i < topCount; i++)
            {
                Target_Numbers an = new Target_Numbers();

                an.targetNumber1 = firstSet[i].targetNumber;
                an.targetNumber2 = secondtSet[i].targetNumber;
                an.targetNumber3 = thirdSet[i].targetNumber;
                an.targetNumber4 = fourthSet[i].targetNumber;
                an.targetNumber5 = fifthSet[i].targetNumber;
                an.targetNumber6 = sixthSet[i].targetNumber;
                an.targetNumber7 = luckySet[i].targetNumber;

                an.targetNumberCount1 = firstSet[i].targetNumberCount;
                an.targetNumberCount2 = secondtSet[i].targetNumberCount;
                an.targetNumberCount3 = thirdSet[i].targetNumberCount;
                an.targetNumberCount4 = fourthSet[i].targetNumberCount;
                an.targetNumberCount5 = fifthSet[i].targetNumberCount;
                an.targetNumberCount6 = sixthSet[i].targetNumberCount;
                an.targetNumberCount7 = luckySet[i].targetNumberCount;

                allNumbers.Add(an);
            }

            return allNumbers;
        }


        List<Target_Number> GetList(List<Target_Number> result)
        {

            var countResult =lottoCore.GetEachListCounts(result);
            Dictionary<int, int> lottoOccurence = lottoCore.BuildDictionary(countResult);

            List < Target_Number > finalResult = new List<Target_Number>();

            foreach (var items in lottoOccurence)
            {
                Target_Number tNumber = new Target_Number();
                tNumber.targetNumber = items.Key;
                tNumber.targetNumberCount = items.Value;
                finalResult.Add(tNumber);
            }

            return finalResult.OrderByDescending(f=>f.targetNumberCount).ToList();
        }

        #region Weekly Suggested Numbers (10 Sets)

        public List<Target_Numbers> GetWeeklySuggestedNumbers()
        {
            var allHistory     = GetLottoHistoryList();
            var allFreq        = GetFrequencyDict(allHistory);
            var allBonusFreq   = GetBonusFrequencyDict(allHistory);
            var result         = new List<Target_Numbers>();

            // Set 1: 역대 전체 빈도 Top 6
            var nums1 = GetTopFrequent(allFreq, 6);
            result.Add(BuildSet("역대 전체 빈도 Top 6", nums1, allFreq, PickBonus(allBonusFreq, nums1), allBonusFreq));

            // Set 2: 최근 104회 빈도 Top 6 (약 2년)
            var hist104 = GetLottoHistoryList(104);
            var freq104 = GetFrequencyDict(hist104);
            var bonus104Freq = GetBonusFrequencyDict(hist104);
            var nums2 = GetTopFrequent(freq104, 6);
            result.Add(BuildSet("최근 104회 빈도 Top 6 (2년)", nums2, freq104, PickBonus(bonus104Freq, nums2), bonus104Freq));

            // Set 3: 최근 52회 빈도 Top 6 (약 1년)
            var hist52 = GetLottoHistoryList(52);
            var freq52 = GetFrequencyDict(hist52);
            var bonus52Freq = GetBonusFrequencyDict(hist52);
            var nums3 = GetTopFrequent(freq52, 6);
            result.Add(BuildSet("최근 52회 빈도 Top 6 (1년)", nums3, freq52, PickBonus(bonus52Freq, nums3), bonus52Freq));

            // Set 4: 최근 26회 빈도 Top 6 (약 반년)
            var hist26 = GetLottoHistoryList(26);
            var freq26 = GetFrequencyDict(hist26);
            var bonus26Freq = GetBonusFrequencyDict(hist26);
            var nums4 = GetTopFrequent(freq26, 6);
            result.Add(BuildSet("최근 26회 빈도 Top 6 (반년)", nums4, freq26, PickBonus(bonus26Freq, nums4), bonus26Freq));

            // Set 5: 최근 10회 빈도 Top 6
            var hist10 = GetLottoHistoryList(10);
            var freq10 = GetFrequencyDict(hist10);
            var bonus10Freq = GetBonusFrequencyDict(hist10);
            var nums5 = GetTopFrequent(freq10, 6);
            result.Add(BuildSet("최근 10회 빈도 Top 6", nums5, freq10, PickBonus(bonus10Freq, nums5), bonus10Freq));

            // Set 6: Hot 3 (최근 52회 상위) + Cold 3 (역대 하위)
            var hot3  = GetTopFrequent(freq52, 3);
            var cold3 = GetBottomFrequent(allFreq, 3, hot3);
            var nums6 = hot3.Concat(cold3).OrderBy(x => x).ToList();
            result.Add(BuildSet("Hot 3 + Cold 3 혼합", nums6, allFreq, PickBonus(allBonusFreq, nums6), allBonusFreq));

            // Set 7: 역발상 Cold - 역대 가장 적게 나온 번호 6개
            var nums7 = GetBottomFrequent(allFreq, 6, new List<int>());
            result.Add(BuildSet("역발상 Cold Top 6 (비출현)", nums7, allFreq, PickBonus(allBonusFreq, nums7), allBonusFreq));

            // Set 8: 홀짝 균형 - 홀수 3개 + 짝수 3개
            var odd3  = allFreq.Where(x => x.Key % 2 != 0).OrderByDescending(x => x.Value).Take(3).Select(x => x.Key).ToList();
            var even3 = allFreq.Where(x => x.Key % 2 == 0).OrderByDescending(x => x.Value).Take(3).Select(x => x.Key).ToList();
            var nums8 = odd3.Concat(even3).OrderBy(x => x).ToList();
            result.Add(BuildSet("홀짝 균형 (홀 3 + 짝 3)", nums8, allFreq, PickBonus(allBonusFreq, nums8), allBonusFreq));

            // Set 9: 고저 균형 - 1~22 중 3개 + 23~45 중 3개
            var low3  = allFreq.Where(x => x.Key <= 22).OrderByDescending(x => x.Value).Take(3).Select(x => x.Key).ToList();
            var high3 = allFreq.Where(x => x.Key >= 23).OrderByDescending(x => x.Value).Take(3).Select(x => x.Key).ToList();
            var nums9 = low3.Concat(high3).OrderBy(x => x).ToList();
            result.Add(BuildSet("고저 균형 (1~22 중 3 + 23~45 중 3)", nums9, allFreq, PickBonus(allBonusFreq, nums9), allBonusFreq));

            // Set 10: 빈도 가중 랜덤
            var nums10 = GetWeightedRandom(allFreq, 6);
            result.Add(BuildSet("빈도 가중 랜덤", nums10, allFreq, PickBonus(allBonusFreq, nums10), allBonusFreq));

            return result;
        }

        public WeeklySuggestedViewModel GetWeeklySuggestedViewModel()
        {
            var allHistory  = GetLottoHistoryList();
            var hist52      = GetLottoHistoryList(52);
            var hist26      = GetLottoHistoryList(26);
            var hist10      = GetLottoHistoryList(10);

            var ranges = new[]
            {
                (name: "1 ~ 9",  min: 1,  max: 9),
                (name: "10 ~ 19", min: 10, max: 19),
                (name: "20 ~ 29", min: 20, max: 29),
                (name: "30 ~ 39", min: 30, max: 39),
                (name: "40 ~ 45", min: 40, max: 45),
            };

            var distributions = new List<RangeDistribution>();
            foreach (var r in ranges)
            {
                double allRate  = CalcRangeRate(allHistory, r.min, r.max);
                double r52Rate  = CalcRangeRate(hist52,    r.min, r.max);
                double r26Rate  = CalcRangeRate(hist26,    r.min, r.max);
                double r10Rate  = CalcRangeRate(hist10,    r.min, r.max);

                distributions.Add(new RangeDistribution
                {
                    RangeName    = r.name,
                    RangeMin     = r.min,
                    RangeMax     = r.max,
                    AllTimeRate  = allRate,
                    Recent52Rate = r52Rate,
                    Recent26Rate = r26Rate,
                    Recent10Rate = r10Rate,
                    Gap52        = allRate - r52Rate,
                    Gap26        = allRate - r26Rate,
                    Gap10        = allRate - r10Rate,
                });
            }

            var allFreq      = GetFrequencyDict(allHistory);
            var allBonusFreq = GetBonusFrequencyDict(allHistory);
            var freq52       = GetFrequencyDict(hist52);
            var bonus52Freq  = GetBonusFrequencyDict(hist52);

            var sets = new List<Target_Numbers>();

            // Set 1: 역대 평균 분포 기반 — 각 구간 비율대로 6개 배분
            var nums1 = PickByDistribution(distributions.Select(d => (d.RangeMin, d.RangeMax, d.AllTimeRate)).ToList(), allFreq, 6);
            sets.Add(BuildSet("역대 평균 분포 기반", nums1, allFreq, PickBonus(allBonusFreq, nums1), allBonusFreq));

            // Set 2: 최근 52회 부족 구간 보정 — Gap52 기준
            var nums2 = PickByGap(distributions, d => d.Gap52, allFreq, 6);
            sets.Add(BuildSet("최근 52회 부족 구간 보정", nums2, allFreq, PickBonus(allBonusFreq, nums2), allBonusFreq));

            // Set 3: 최근 26회 부족 구간 보정
            var nums3 = PickByGap(distributions, d => d.Gap26, allFreq, 6);
            sets.Add(BuildSet("최근 26회 부족 구간 보정", nums3, allFreq, PickBonus(allBonusFreq, nums3), allBonusFreq));

            // Set 4: 최근 10회 부족 구간 보정
            var nums4 = PickByGap(distributions, d => d.Gap10, allFreq, 6);
            sets.Add(BuildSet("최근 10회 부족 구간 보정", nums4, allFreq, PickBonus(allBonusFreq, nums4), allBonusFreq));

            // Set 5: 최근 10회 Hot 구간 — 최근에 많이 나온 구간 우선
            var nums5 = PickByGap(distributions, d => -d.Gap10, allFreq, 6);
            sets.Add(BuildSet("최근 10회 Hot 구간 집중", nums5, allFreq, PickBonus(allBonusFreq, nums5), allBonusFreq));

            return new WeeklySuggestedViewModel
            {
                RangeDistributions = distributions,
                SuggestedSets      = sets,
                TotalDraws         = allHistory.Count,
            };
        }

        // 구간별 출현 비율 계산
        private double CalcRangeRate(List<Lotto_History> history, int min, int max)
        {
            if (!history.Any()) return 0;
            int total = history.Count * 6;
            int count = history.Sum(h =>
                new[] { h.num1, h.num2, h.num3, h.num4, h.num5, h.num6 }
                    .Count(n => n >= min && n <= max));
            return Math.Round((double)count / total * 100, 1);
        }

        // 역대 평균 분포 비율대로 각 구간에서 번호 배분
        private List<int> PickByDistribution(List<(int min, int max, double rate)> ranges, Dictionary<int, int> freqDict, int total)
        {
            var allocation = AllocateByRate(ranges.Select(r => r.rate).ToList(), total);
            var selected = new List<int>();
            for (int i = 0; i < ranges.Count; i++)
            {
                int n = allocation[i];
                if (n <= 0) continue;
                var candidates = freqDict
                    .Where(x => x.Key >= ranges[i].min && x.Key <= ranges[i].max && !selected.Contains(x.Key))
                    .OrderByDescending(x => x.Value)
                    .Take(n)
                    .Select(x => x.Key);
                selected.AddRange(candidates);
            }
            return selected.OrderBy(x => x).ToList();
        }

        // Gap 기준으로 부족한 구간에 더 많이 배분
        private List<int> PickByGap(List<RangeDistribution> dists, Func<RangeDistribution, double> gapSelector, Dictionary<int, int> freqDict, int total)
        {
            // Gap이 클수록(부족할수록) 더 많이 배분 — 역대 비율에 Gap 보정 추가
            var adjusted = dists.Select(d =>
            {
                double rate = Math.Max(0, d.AllTimeRate + gapSelector(d));
                return (min: d.RangeMin, max: d.RangeMax, rate);
            }).ToList();

            return PickByDistribution(adjusted, freqDict, total);
        }

        // 비율 배열 → 정수 배분 (총합 = total 보장)
        private List<int> AllocateByRate(List<double> rates, int total)
        {
            double sum = rates.Sum();
            var alloc = rates.Select(r => (int)Math.Floor(r / sum * total)).ToList();
            int remainder = total - alloc.Sum();
            // 소수점이 큰 구간부터 나머지 배분
            var order = rates
                .Select((r, i) => (frac: (r / sum * total) - alloc[i], idx: i))
                .OrderByDescending(x => x.frac)
                .Select(x => x.idx)
                .ToList();
            for (int i = 0; i < remainder; i++)
                alloc[order[i]]++;
            return alloc;
        }

        private Dictionary<int, int> GetFrequencyDict(List<Lotto_History> history)
        {
            int[] counts = lottoCore.GetTotalCounts(history);
            return lottoCore.BuildDictionary(counts);
        }

        private Dictionary<int, int> GetBonusFrequencyDict(List<Lotto_History> history)
        {
            int[] counts = lottoCore.GetTotalBonusCounts(history);
            return lottoCore.BuildDictionary(counts);
        }

        private int PickBonus(Dictionary<int, int> bonusFreq, List<int> exclude)
        {
            var bonus = bonusFreq.Where(x => !exclude.Contains(x.Key))
                                 .OrderByDescending(x => x.Value)
                                 .FirstOrDefault();
            return bonus.Key;
        }

        private List<int> GetTopFrequent(Dictionary<int, int> freqDict, int n)
        {
            return freqDict.OrderByDescending(x => x.Value).Take(n).Select(x => x.Key).OrderBy(x => x).ToList();
        }

        private List<int> GetBottomFrequent(Dictionary<int, int> freqDict, int n, List<int> exclude)
        {
            return freqDict.Where(x => !exclude.Contains(x.Key)).OrderBy(x => x.Value).Take(n).Select(x => x.Key).OrderBy(x => x).ToList();
        }

        private List<int> GetWeightedRandom(Dictionary<int, int> freqDict, int n)
        {
            var rng = new Random();
            int total = freqDict.Values.Sum();
            var numbers = freqDict.Keys.ToList();
            var weights = freqDict.Values.ToList();
            var selected = new HashSet<int>();

            while (selected.Count < n)
            {
                int r = rng.Next(total);
                int cumulative = 0;
                for (int i = 0; i < numbers.Count; i++)
                {
                    cumulative += weights[i];
                    if (r < cumulative)
                    {
                        selected.Add(numbers[i]);
                        break;
                    }
                }
            }
            return selected.OrderBy(x => x).ToList();
        }

        private Target_Numbers BuildSet(string label, List<int> numbers, Dictionary<int, int> freqDict, int bonus, Dictionary<int, int> bonusFreqDict)
        {
            var sorted = numbers.OrderBy(x => x).ToList();
            while (sorted.Count < 6) sorted.Add(0);
            return new Target_Numbers
            {
                setLabel = label,
                targetNumber1 = sorted[0], targetNumberCount1 = freqDict.ContainsKey(sorted[0]) ? freqDict[sorted[0]] : 0,
                targetNumber2 = sorted[1], targetNumberCount2 = freqDict.ContainsKey(sorted[1]) ? freqDict[sorted[1]] : 0,
                targetNumber3 = sorted[2], targetNumberCount3 = freqDict.ContainsKey(sorted[2]) ? freqDict[sorted[2]] : 0,
                targetNumber4 = sorted[3], targetNumberCount4 = freqDict.ContainsKey(sorted[3]) ? freqDict[sorted[3]] : 0,
                targetNumber5 = sorted[4], targetNumberCount5 = freqDict.ContainsKey(sorted[4]) ? freqDict[sorted[4]] : 0,
                targetNumber6 = sorted[5], targetNumberCount6 = freqDict.ContainsKey(sorted[5]) ? freqDict[sorted[5]] : 0,
                targetNumber7 = bonus,     targetNumberCount7 = bonusFreqDict.ContainsKey(bonus) ? bonusFreqDict[bonus] : 0,
            };
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