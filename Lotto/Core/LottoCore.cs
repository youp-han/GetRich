using System;
using System.Collections.Generic;
using System.Linq;
using Lotto.Models;


namespace Lotto.Core
{
    public class LottoCore
    {


        #region Calculate Total Occurence from 1, 45
        //Count 계산된 Array
        // 총 45 개이며, key 값= 0..44 (0=1..44=45)
        public int[] GetTotalCounts(List<Lotto_History> lottoHistories)
        {
            int[] results = new int[45];

            foreach (var item in lottoHistories)
            {
                for (int i = 0; i < results.Count(); i++)
                {
                    if (item.num1.Equals(i + 1))
                    {
                        results[i] += 1;
                    }
                    if (item.num2.Equals(i + 1))
                    {
                        results[i] += 1;
                    }
                    if (item.num3.Equals(i + 1))
                    {
                        results[i] += 1;
                    }
                    if (item.num4.Equals(i + 1))
                    {
                        results[i] += 1;
                    }
                    if (item.num5.Equals(i + 1))
                    {
                        results[i] += 1;
                    }
                    if (item.num6.Equals(i + 1))
                    {
                        results[i] += 1;
                    }
                    if (item.bonus.Equals(i + 1))
                    {
                        results[i] += 1;
                    }
                }
            }

            return results;
        }


        //Count 계산된 Array
        // 총 45 개이며, key 값= 0..44 (0=1..44=45)
        //Key, Value 로 Dictionary 에 추가한다.
        public Dictionary<int, int> BuildDictionary(int[] results)
        {
            Dictionary<int, int> lottoOccurence = new Dictionary<int, int>();
            for (int i = 0; i < results.Count(); i++)
            {
                int k = i + 1;
                lottoOccurence.Add(k, results[i]);
            }

            return lottoOccurence;
        }



        //Dictionary 를 Key 값으로 Sort 한다. (DESC)
        public Dictionary<int, int> GetDictionarySorted(Dictionary<int, int> lottoOccurence, int count)
        {
            var sortedItems = (from entry in lottoOccurence
                               orderby entry.Value
                                   descending
                               select entry).ToDictionary(
                pair => pair.Key,
                pair => pair.Value);

            return sortedItems;
        }

        #endregion
               
        #region GetGeneratedLottoNumber

        public int[] GetGeneratedLottoNumber()
        {
            return GenerateLottoNumber();
        }

        int GetRandomNumber(int minimum, int maximum)
        {
            Random random = new Random();
            return random.Next(minimum, maximum);
        }


        int[] GenerateLottoNumber()
        {
            int[] numSet = { 0, 0, 0, 0, 0, 0 };
            int minimum = 1;
            int maximum = 45;

            for (int i = 0; i < numSet.Length; i++)
            {
                int genNumber = GetRandomNumber(minimum, maximum);
                if (i >= 0)
                {
                    if (!numSet.Contains(genNumber))
                    {
                        numSet[i] = genNumber;
                    }
                    else
                    {
                        i = i - 1;
                    }
                   

                }//end if
            }//end for

            return numSet;
        }// end Method

        #endregion


    }
}