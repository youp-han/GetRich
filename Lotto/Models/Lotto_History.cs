using System.ComponentModel.DataAnnotations;


namespace Lotto.Models
{
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

    }


    public class Numbers_NCounts
    {
        public int ID { get; set; }
        public int numbers { get; set; }
        public int nCounts { get; set; }
    }
}
