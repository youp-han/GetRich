using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Lotto.Models;
using Lotto.Repository;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;


namespace Lotto.Controllers
{
    public class LottoMngController : BaseController
    {

        private ILottoMngRepository lottoMngRepository;

        public LottoMngController()
        {
            lottoMngRepository = new LottoMngRepository(new LottoMNGContext());
        }

        public ActionResult Analytics()
        {
            return View();
        }

        public ActionResult WeeklySuggestedNumbers()
        {
            return View();
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
                isSuccessful = "success", isMsg = " { " + getNumbers + " } "}, JsonRequestBehavior.AllowGet);
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
            return Json(true, "업데이트 완료", new { isSuccessful = "success", isMsg = "Update가 성공적으로 완료 되었습니다." }, JsonRequestBehavior.AllowGet);

        }


        public JsonResult GetLatestLottoNumber()
        {
            string strResult = null;

            var result = new JsonResult();

            try
            {
                Lotto_History receivedLastestNumbers =
                    lottoMngRepository.GetLottoHistoryList(1).ToList().SingleOrDefault();

                if (receivedLastestNumbers.seqNo <= 0)
                {
                    throw new Exception("아직 최신 결과가 없습니다. 다음에 다시 확인하세요");
                }
                else
                {
                    strResult = "{"
                                + "\"drawDate\": \"" + receivedLastestNumbers.drawDate + "\", "
                                + "\"seqNo\": " + receivedLastestNumbers.seqNo + ", "
                                + "\"num1\": " + receivedLastestNumbers.num1 + ", "
                                + "\"num2\": " + receivedLastestNumbers.num2 + ", "
                                + "\"num3\": " + receivedLastestNumbers.num3 + ", "
                                + "\"num4\": " + receivedLastestNumbers.num4 + ", "
                                + "\"num5\": " + receivedLastestNumbers.num5 + ", "
                                + "\"num6\": " + receivedLastestNumbers.num6 + ", "
                                + "\"bonus\": " + receivedLastestNumbers.bonus
                                + "}";
                }

            }
            catch (Exception e)
            {
                return Json(false, "오류", e.Message, JsonRequestBehavior.AllowGet);

            }

            return Json(true, "완료",  strResult, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetLatestPriceMoney()
        {
            string strResult = null;

            var result = new JsonResult();

            try
            {
                Lotto_History receivedLastestNumbers =
                    lottoMngRepository.GetLottoHistoryList(1).ToList().SingleOrDefault();

                if (receivedLastestNumbers.seqNo <= 0)
                {
                    throw new Exception("아직 최신 결과가 없습니다. 다음에 다시 확인하세요");
                }
                else
                {
                    strResult = "{"
                                + "\"drawDate\": \"" + receivedLastestNumbers.drawDate + "\", "
                                + "\"firstPriceTotal\": " + receivedLastestNumbers.firstPriceTotal + ", "
                                + "\"eachReceivedFirstPrice\": " + receivedLastestNumbers.eachReceivedFirstPrice + ", "
                                + "\"firstPriceSelected\": " + receivedLastestNumbers.firstPriceSelected
                                + "}";
                }

            }
            catch (Exception e)
            {
                return Json(false, "오류", e.Message, JsonRequestBehavior.AllowGet);

            }

            return Json(true, "완료", strResult, JsonRequestBehavior.AllowGet);

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
                [firstPriceTotal] [decimal](18, 0) NULL,
                [eachReceivedFirstPrice] [decimal](18, 0) NULL,
                [firstPriceSelected] [int] NULL,
                [drawDate]
                [nvarchar]
                (max) NULL,
                    CONSTRAINT[PK_dbo.Lotto_History] PRIMARY KEY CLUSTERED
                (
                    [ID] ASC
                )WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
                ) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]
                GO

                    ALTER TABLE[dbo].[Lotto_History] ADD CONSTRAINT[DF_Lotto_History_seqNo]  DEFAULT((0)) FOR[seqNo]
                GO

       */

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