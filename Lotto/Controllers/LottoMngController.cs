using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Lotto.Models;
using Lotto.Repository;


namespace Lotto.Controllers
{
    public class LottoMngController : BaseController
    {

        private ILottoMngRepository lottoMngRepository;

        public LottoMngController()
        {
            lottoMngRepository = new LottoMngRepository(new LottoMNGContext());
        }


        #region View Recent History (top 30)
        /// <summary>
        ///  RecentHistory View
        /// </summary>
        /// <returns></returns>
        public ActionResult RecentHistory()
        {
            List<Lotto_History> reCentHistories = GetListOfHistory(30);
            ViewBag.topNumber = lottoMngRepository.GetTopNumber();

            return View(reCentHistories);
        }

        #endregion

        #region Possibilities of GeneratedNumbers : API (work in progress) domain action required

        public JsonResult GetPossibilities()
        {
            string getNumbers = lottoMngRepository.GetGetPosNum();

            return Json(true, "", new
            {
                isSuccessful = "success",
                isMsg = " { " + getNumbers + " } "
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region View History
        /// <summary>
        ///  History View
        /// </summary>
        /// <returns></returns>
        public ActionResult History()
        {
            List<Lotto_History> lottoHistories = GetListOfHistory(0);
            return View(lottoHistories);
        }


        List<Lotto_History> GetListOfHistory(int topNumbers)
        {
            List<Lotto_History> result = new List<Lotto_History>(); 

            if (topNumbers > 0)
            {
                result = lottoMngRepository.GetLottoHistoryList(topNumbers);
            }
            else
            {
                result = lottoMngRepository.GetLottoHistoryList();
            }
            
            return result;
        }

        #endregion

        #region Update History :API (work in progress)

        /// <summary>
        /// Lotto 번호 저장
        /// </summary>
        /// <param name="historyItems"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public JsonResult GetLatestLottoNumbers()
        {
            try
            {
                Lotto_History receivedLastestNumbers = lottoMngRepository.GetUpdateNumbers();               

                if (receivedLastestNumbers.num1 >= 0)
                {
                    SaveLottoNumbers(receivedLastestNumbers);
                    //
                    this.GetLatestLottoNumbers();
                }
                else
                {
                    throw new Exception("아직 최신 결과가 없습니다. 다음에 다시 확인하세요");
                }

            }
            catch (Exception e)
            {
                return Json(false, "최신결과 없음", new { isSuccessful = "fail", isMsg = e.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(true, "", new { isSuccessful = "success", isMsg = "success: " }, JsonRequestBehavior.AllowGet);

        }

        #endregion

        #region Save History : A Part of Update Process
        //lotto log methods
        void SaveLottoNumbers(Lotto_History historyItem)
        {
            try
            {
                lottoMngRepository.LottoNumberSave(historyItem);
            }
            catch (Exception e)
            {
                throw new Exception("DB 저장 중 문제가 발생했습니다." + e.Message);
            }
        }

        #endregion

        #region DB Initiation
        /*         
            USE [GetRich]
            GO
            SET ANSI_NULLS ON
            GO
            SET QUOTED_IDENTIFIER ON
            GO
            CREATE TABLE[dbo].[Lotto_History]
            (
            [ID][int] IDENTITY(1,1) NOT NULL,
            [num1] [int] NOT NULL,
            [num2] [int] NOT NULL,
            [num3] [int] NOT NULL,
            [num4] [int] NOT NULL,
            [num5] [int] NOT NULL,
            [num6] [int] NOT NULL,
            [bonus] [int] NOT NULL,
            [seqNo] [int] NOT NULL,
            CONSTRAINT[PK_dbo.Lotto_History] PRIMARY KEY CLUSTERED
            (
                [ID] ASC
            )WITH
            (
                PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
            ) ON[PRIMARY]
            GO
            ALTER TABLE[dbo].[Lotto_History] ADD CONSTRAINT[DF_Lotto_History_seqNo]  DEFAULT((0)) FOR[seqNo]
            GO
        
       */

        /// <summary>
        /// POSTMAN 으로 데이터 밀어 넣는 작업을 위한 함수
        /// GetLatestLottoNumbers 재귀 호출로 인하여 필요 없게 되버림.
        /// </summary>
        /// <param name="historyItems"></param>
        /// <returns></returns>
        //public bool SaveLottoHistoryResult(List<Lotto_History> historyItems)
        //{

        //    try
        //    {
        //        foreach (Lotto_History items in historyItems)
        //        {
        //            this.SaveLottoNumbers(items);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("저장하는 중 오류 발생 " + e.Message);
        //    }
        //    return true;
        //}


        #endregion

        #region A Result of Full Counted Numbers

        void saveFullCountedNumbers()
        {
            Dictionary<int, int> getCountedNumbers = GetFullCountedNumbers();

        }


        Dictionary<int, int> GetFullCountedNumbers()
        {
            Dictionary<int, int> sortedItems = lottoMngRepository.GetNumbers();
            return sortedItems;
            
        }


        /// <summary>
        /// Test Method
        /// </summary>
        /// <param name="sortedItems">check Dictionary Contents</param>
        /// <returns></returns>

        string CheckResults(Dictionary<int, int> sortedItems)
        {

            string resultCount2 = null;
            foreach (var item in sortedItems)
            {
                resultCount2 = (resultCount2 + "(" + item.Key + ", " + item.Value + "), ");
            }

            return resultCount2;
        }

        #endregion

    }
}