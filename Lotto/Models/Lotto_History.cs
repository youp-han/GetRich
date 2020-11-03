using System.ComponentModel.DataAnnotations;


namespace Lotto.Models
{

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


    public class Lotto_History
    {

        public int ID { get; set; }
        
        [Display(Name = "회차")]
       public int seqNo { get; set; }

        [Display(Name = "1번째 번호")]
        public int num1 { get; set; }
        [Display(Name = "2번째 번호")]
        public int num2 { get; set; }
        [Display(Name = "3번째 번호")]
        public int num3 { get; set; }
        [Display(Name = "4번째 번호")]
        public int num4 { get; set; }
        [Display(Name = "5번째 번호")]
        public int num5 { get; set; }
        [Display(Name = "6번째 번호")]
        public int num6 { get; set; }
        [Display(Name = "+ 보너스")]
        public int bonus { get; set; }

        public decimal firstPriceTotal { get; set; }
        public int firstPriceSelected { get; set; }
        public decimal eachReceivedFirstPrice { get; set; }
        public string drawDate { get; set; }
    }


    public class Numbers_NCounts
    {

        [Display(Name = "SeqNo")]
        public int ID { get; set; }

        [Display(Name = "번호")]
        public int numbers { get; set; }
        [Display(Name = "횟수")]
        public int nCounts { get; set; }
    }


    public class Two_Numbers
    {
        public float Num1 { get; set; }
        public float Num2 { get; set; }
    }

    public class Target_Number
    {
        public int targetNumber { get; set; }
        public int targetNumberCount { get; set; }
    }

    public class Target_Numbers
    {
        public int targetNumber1 { get; set; }
        public int targetNumberCount1 { get; set; }
        public int targetNumber2 { get; set; }
        public int targetNumberCount2 { get; set; }
        public int targetNumber3 { get; set; }
        public int targetNumberCount3 { get; set; }
        public int targetNumber4 { get; set; }
        public int targetNumberCount4 { get; set; }
        public int targetNumber5 { get; set; }
        public int targetNumberCount5 { get; set; }
        public int targetNumber6 { get; set; }
        public int targetNumberCount6 { get; set; }
        public int targetNumber7 { get; set; }
        public int targetNumberCount7 { get; set; }
    }
}
